using NumberRangeConverter.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberRangeConverter
{
    public class NumberRangeHelper
    {
        /// <summary>
        /// Recalculate all given SBC range against all given loopup number range per each customer
        /// </summary>
        /// <param name="existingSbcRange"></param>
        /// <param name="existingLoopUpNumberRange"></param>
        /// <param name="verifyAfterRecalculation">if true, there will extra step to translate back the final ribbon sbc and compare with the actual numbers to verify it's a correct ribbon sbc</param>
        public static List<RecalculationResult> RecalculateRange(List<RibbonNumberRange> existingSbcRange, List<LoopupNumberRange> existingLoopUpNumberRange, bool verifyAfterRecalculation = true, int MaxDegreeOfParallelism = 5)
        {
            var result = new List<RecalculationResult>();

            // recalculate per customer
            var byCustomer = from p in existingSbcRange
                             group p by p.Customer into g
                             select new
                             {
                                 Customer = g.Key,
                                 SbcRanges = g.ToList(),
                             };

            Parallel.ForEach(byCustomer, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, s =>
            {
                List<RibbonNumberRange> finalRange = new List<RibbonNumberRange>();
                var numberOfDigits = s.SbcRanges.First().NumberOfDigits;
                var allCustomersNumberRange = existingLoopUpNumberRange.Where(e => e.Numbers.All(n => n.Customer == s.Customer));
                var allCustomerNumbers = allCustomersNumberRange.SelectMany(a => a.Numbers);

                ConcurrentBag<RibbonNumberRange> inEfficientRangeAlreadyProcessed = new ConcurrentBag<RibbonNumberRange>();

                // Iterate through all customer number range
                // Unfortunately still can't figure out how to make it parallel, since finalRange matters for next iteration
                // if it's parallel, the other thread may see incorrect value
                allCustomersNumberRange.ToList().ForEach(e => {
                    IEnumerable<RibbonNumberRange> currentSbcRange;// = new List<RibbonNumberRange>();
                    if (finalRange.Count == 0) {
                        currentSbcRange = s.SbcRanges.Select(n => new RibbonNumberRange
                        {
                            RibbonSbcRange = n.RibbonSbcRange,
                            Customer = n.Customer,
                            RangeStart = n.RangeStart,
                            RangeEnd = n.RangeEnd,
                            NumberOfDigits = n.NumberOfDigits
                        }).ToList();
                    } else {
                        currentSbcRange = finalRange.Select(n => new RibbonNumberRange
                        {
                            RibbonSbcRange = n.RibbonSbcRange,
                            Customer = n.Customer,
                            RangeStart = n.RangeStart,
                            RangeEnd = n.RangeEnd,
                            Status = n.Status,
                            NumberOfDigits = n.NumberOfDigits
                        }).ToList();
                    }

                    var existingCustomerPhoneNumbers = e.Numbers.Where(n => n.Customer == s.Customer).Select(m => m.Number).ToList();

                    // check the range efficiency on each iteration (only for the one that not checked yet)
                    foreach(var csb in currentSbcRange.Where(cbc => cbc.Status == RangeStatus.Unset))
                    {
                        csb.Status = IsEfficientRange(existingCustomerPhoneNumbers, csb.RibbonSbcRange, numberOfDigits);
                    }

                    var validRanges = currentSbcRange.Where(c => c.Status == RangeStatus.Efficient).ToList();
                    var invalidRanges = currentSbcRange.Except(validRanges);

                    if (currentSbcRange.All(c => c.Status == RangeStatus.Efficient))
                    {
                        // it maybe all are efficient range, but there could be numbers that have no range associated
                        var totalTranslatedRangeToNumbers = validRanges.Sum(f => f.Coverage.Count);
                        var diff = existingCustomerPhoneNumbers.Count() - totalTranslatedRangeToNumbers;
                        if (diff != 0)
                        {
                            if (diff > 1)
                            {
                                var maxFromTranslatedRanges = validRanges.SelectMany(v => v.Coverage).Max();
                                var minFromTranslatedRanges = validRanges.SelectMany(v => v.Coverage).Min();
                                if (maxFromTranslatedRanges < existingCustomerPhoneNumbers.Min()
                                || minFromTranslatedRanges > existingCustomerPhoneNumbers.Max())
                                {
                                    // found set of numbers that have no range yet, then create new ones
                                    var start = existingCustomerPhoneNumbers.Min();
                                    var end = existingCustomerPhoneNumbers.Max();
                                    var res = RangeToRibbonSBC(start.ToString(), end.ToString());

                                    validRanges.AddRange(res.Select(r => new RibbonNumberRange
                                    {
                                        Customer = s.Customer,
                                        RibbonSbcRange = r,
                                        Status = RangeStatus.Efficient,
                                        RangeStart = existingCustomerPhoneNumbers.Min(),
                                        RangeEnd = existingCustomerPhoneNumbers.Max()
                                    }));
                                }
                            }
                            else
                            {
                                // this could be the end range, s just add it as is
                                validRanges.Add(new RibbonNumberRange
                                {
                                    RibbonSbcRange = existingCustomerPhoneNumbers.Max().ToString(),
                                    Status = RangeStatus.Efficient,
                                    Customer = s.Customer,
                                    RangeStart = existingCustomerPhoneNumbers.Max(),
                                    RangeEnd = existingCustomerPhoneNumbers.Max()
                                });
                            }
                        }
                    }
                    else if (validRanges.Count == 0){
                        // if all invalid range, then just create new ones
                        var rangeStart = existingCustomerPhoneNumbers.Min();
                        var rangeEnd = existingCustomerPhoneNumbers.Max();
                        var newRange = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());

                        foreach (var n in newRange)
                        {
                            validRanges.Add(new RibbonNumberRange
                            {
                                Customer = s.Customer,
                                RibbonSbcRange = n,
                                RangeStart = rangeStart,
                                RangeEnd = rangeEnd,
                                Status = RangeStatus.Efficient
                            });
                        }
                    }
                    else
                    {
                        // adjust some of the invalid ranges
                        foreach(var inv in invalidRanges.Where(inv => inv.Status == RangeStatus.InEfficient && !inEfficientRangeAlreadyProcessed.Contains(inv)))
                        {
                            var newRange = new List<string>();

                            var totalUnallocatedNumbers = GetUnallocatedNumbers(existingCustomerPhoneNumbers, inv.RibbonSbcRange, numberOfDigits);

                            // if there is unnecessary range below the bottom limit of the numbers, then create a proper range
                            if (totalUnallocatedNumbers.Item1.Count > 0)
                            {
                                var rangeStart = existingCustomerPhoneNumbers.Min();
                                var rangeEnd = validRanges.SelectMany(x => x.Coverage).Min();
                                newRange = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());

                                foreach (var n in newRange)
                                {
                                    if (!validRanges.Any(f => n.StartsWith(f.RibbonSbcRange)))
                                    {
                                        validRanges.Add(new RibbonNumberRange
                                        {
                                            Customer = s.Customer,
                                            RibbonSbcRange = n,
                                            RangeStart = rangeStart,
                                            RangeEnd = rangeEnd,
                                            Status = RangeStatus.Efficient
                                        });
                                    }
                                }
                            }

                            // if there is unnecessary range above the upper limit of the numbers, then create a proper range
                            if (totalUnallocatedNumbers.Item2.Count > 0)
                            {
                                var rangeStart = validRanges.SelectMany(x => x.Coverage).Max();
                                var rangeEnd = existingCustomerPhoneNumbers.Max();
                                newRange = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());

                                foreach (var n in newRange)
                                {
                                    if (!validRanges.Any(f => n.StartsWith(f.RibbonSbcRange)))
                                    {
                                        validRanges.Add(new RibbonNumberRange
                                        {
                                            Customer = s.Customer,
                                            RibbonSbcRange = n,
                                            RangeStart = rangeStart,
                                            RangeEnd = rangeEnd,
                                            Status = RangeStatus.Efficient
                                        });
                                    }
                                }
                            }

                            inEfficientRangeAlreadyProcessed.Add(inv);
                        }
                    }

                    finalRange = validRanges;
                });

                if (allCustomerNumbers.Any())
                {
                    if (verifyAfterRecalculation)
                    {
                        // verify the total numbers equal to translated sbc numbers
                        // note: this verify is just for testing purpose
                        VerifyRecalculation(finalRange.ToList(), allCustomerNumbers);
                    }
                }
                else
                {
                    // this customer have no number, shall we just remove the unused range??
                }

                result.Add(new RecalculationResult
                {
                    Customer = s.Customer,
                    RibbonNumberRanges = finalRange
                });
            });

            return result;
        }

        /// <summary>
        /// Get unallocated numbers to range that could be bigger than it should
        /// The left part if the range unnecessarily include number below the min number
        /// The right part if the range unnecessarily include number above the max number
        /// </summary>
        /// <param name="existingNumbers"></param>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public static Tuple<List<UInt64>, List<UInt64>> GetUnallocatedNumbers(List<UInt64> existingNumbers, string sbcRange, int numberOfDigits)
        {
            List<UInt64> LeftPart = new List<UInt64>();
            List<UInt64> RightPart = new List<UInt64>();

            var translatedNumbers = RangeToNumbers(sbcRange, numberOfDigits);
            if (translatedNumbers.Min() < existingNumbers.Min())
            {
                var rate = Math.Log10(existingNumbers.Min() - translatedNumbers.Min());
                if (rate > 1)
                {
                    for (var i = translatedNumbers.Min(); i <= existingNumbers.Min(); i++)
                    {
                        LeftPart.Add(i);
                    }
                }
            }

            if (translatedNumbers.Max() > existingNumbers.Max())
            {
                var rate = Math.Log10(translatedNumbers.Max() - existingNumbers.Max());
                if (rate > 1)
                {
                    for (var i = existingNumbers.Max(); i <= translatedNumbers.Max(); i++)
                    {
                        RightPart.Add(i);
                    }
                }
            }

            return Tuple.Create(LeftPart, RightPart);
        }

        public static UInt64 GetTotalUnallocatedNumbers(List<UInt64> existingNumbers, string sbcRange, int numberOfDigits)
        {
            UInt64 unallocatedNumbers = default;

            var translatedNumbers = RangeToNumbers(sbcRange, numberOfDigits);

            if (translatedNumbers.Count == 1 && translatedNumbers.First() < existingNumbers.Max())
            {
                // it's considered not efficient range, cause full digit and not last number
                throw new NotRangeException();
            }

            if (translatedNumbers.Max() < existingNumbers.Min() || translatedNumbers.Min() > existingNumbers.Max())
            {
                // out of range, skip
                throw new OutOfRangeException();
            }

            if (translatedNumbers.Min() < existingNumbers.Min())
            {
                var rate = Math.Log10(existingNumbers.Min() - translatedNumbers.Min());
                if (rate > 1)
                {
                    unallocatedNumbers += existingNumbers.Min() - translatedNumbers.Min();
                }
            }

            if (translatedNumbers.Max() > existingNumbers.Max())
            {
                var rate = Math.Log10(translatedNumbers.Max() - existingNumbers.Max());
                if (rate > 1)
                {
                    unallocatedNumbers += translatedNumbers.Max() - existingNumbers.Max();
                }
            }

            return unallocatedNumbers;
        }

        public static RangeStatus IsEfficientRange(List<UInt64> existingNumbers, string sbcRange, int numberOfDigits)
        {
            RangeStatus isEfficient = default;
            try
            {
                var totalUnallocatedNumbers = GetTotalUnallocatedNumbers(existingNumbers, sbcRange, numberOfDigits);
                if (totalUnallocatedNumbers > 0)
                {
                    // if the range cover unnessarily numbers, categorize as inefficient
                    isEfficient = RangeStatus.InEfficient;
                }
                else
                {
                    isEfficient = RangeStatus.Efficient;
                }
            }
            catch (NotRangeException)
            {
                // if it's not a range, categorize as inefficient
                isEfficient = RangeStatus.InEfficient;
            }
            catch (OutOfRangeException)
            {
                isEfficient = RangeStatus.OutOfRange;
            }

            return isEfficient;
        }

        /// <summary>
        /// Helper to translate single sbc range to numbers, need to improve performance
        /// </summary>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public static List<UInt64> RangeToNumbers(string sbcRange, int numberOfDigits)
        {
            List<UInt64> numbers = new List<UInt64>();
            var degree = (UInt64)Math.Pow(10, numberOfDigits - sbcRange.Length);
            UInt64 _number = UInt64.Parse(sbcRange) * degree;
            var _upperlimit = _number + (degree - 1);

            for (var y = _number; y <= _upperlimit; y++)
            {
                numbers.Add(y);
            }

            return numbers;
        }

        /// <summary>
        /// This method to translate back the collection of Ribbon SBC range to actual numbers
        /// </summary>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public static List<UInt64> RangesToNumbers(List<string> sbcRange, int numberOfDigits)
        {
            List<UInt64> numbers = new List<UInt64>();

            foreach (var r in sbcRange)
            {
                var _rangeToNumberResult = RangeToNumbers(r, numberOfDigits);
                foreach (var o in _rangeToNumberResult)
                {
                    numbers.Add(o);
                }
            }

            return numbers;
        }

        /// <summary>
        /// Main logic to convert given range into Ribon SBC range
        /// </summary>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <returns></returns>
        public static List<string> RangeToRibbonSBC(string rangeStart, string rangeEnd)
        {
            List<string> finalRanges = new List<string>();
            var prefix = new StringBuilder();

            for (var i = 0; i < rangeStart.Count(); i++)
            {
                if (rangeStart[i] == rangeEnd[i])
                {
                    // getting the prefix by populating the same digit
                    prefix.Append(rangeEnd[i]);
                }
                else
                {
                    // work on splitting the range where the digit start to be different
                    var startLeft = rangeStart.Substring(i, rangeStart.Length - i);
                    var endLeft = rangeEnd.Substring(i, rangeEnd.Length - i);

                    // if the startLeft start with number other than 0, remove the trailing 0
                    // the one that we need to increment is the most significant digit
                    startLeft = int.Parse(startLeft) > 0 ? startLeft.TrimEnd('0') : startLeft;

                    var totalDigit = startLeft.Length;

                    // increment start from last digit backward to first digit
                    var newRange = int.Parse(prefix.ToString()) * Math.Pow(10, totalDigit) + int.Parse(startLeft);
                    for (var j = (totalDigit - 1); j >= 0; j--)
                    {
                        var lastDigit = newRange % 10;

                        // if last iteration (first digit)
                        if (j == 0)
                        {
                            var upperLimit = int.Parse(endLeft[j].ToString());

                            if (totalDigit == 1 && startLeft == "0" && endLeft == "9")
                            {
                                // it's sbc pattern, just skip this one digit and add to collection
                                newRange = Math.Floor(newRange / 10);
                                finalRanges.Add(newRange.ToString());
                            }
                            else
                            {
                                // iterate from last digit till the upper limit
                                for (var k = lastDigit; k <= upperLimit; k++)
                                {
                                    if (k > 0 && k < upperLimit && k >= lastDigit)
                                    {
                                        finalRanges.Add(newRange.ToString());
                                    }
                                    else if (k == upperLimit) // last iteration
                                    {
                                        var newStart = string.Concat(prefix, k * (Math.Pow(10, endLeft.Length - 1)));

                                        // if it's not the same value as the end range, do recursive to translate the range
                                        if (newStart.ToString() != rangeEnd)
                                        {
                                            List<string> additionalRange = RangeToRibbonSBC(newStart.ToString(), rangeEnd);
                                            finalRanges.AddRange(additionalRange);
                                        }
                                        else
                                        {
                                            // otherwise add it as is
                                            finalRanges.Add(newStart.ToString());
                                        }
                                    }
                                    else if (k == 0 || k == lastDigit)
                                    {
                                        finalRanges.Add(newRange.ToString());
                                    }

                                    newRange++;
                                }
                            }
                        }
                        else if (lastDigit == 0)
                        {
                            // if the last digit of the newRange is 0 then continue to move to next digit
                            newRange = Math.Floor(newRange / 10);

                            continue;
                        }
                        else
                        {
                            // just increment to digit 9
                            for (var n = lastDigit; n <= 9; n++)
                            {
                                if (n > 0)
                                {
                                    finalRanges.Add(newRange.ToString());
                                }

                                newRange++;
                            }
                        }

                        // and move to the next digit
                        newRange = Math.Floor(newRange / 10);
                    }

                    break;
                }
            }

            return finalRanges;
        }

        private static void VerifyRecalculation(List<RibbonNumberRange> finalRange, IEnumerable<PhoneNumber> allCustomerNumbers)
        {
            var allTranslatedRangeToNumbers = finalRange.Sum(f => f.Coverage.Count);
            if (finalRange.All(f => f.Status == RangeStatus.Efficient)
                && allTranslatedRangeToNumbers != allCustomerNumbers.Count())
            {
                throw new Exception("Unmatch");
            }
        }
    }
}
