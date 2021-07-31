using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumberRangeConverter
{
    public class NumberRangeHelper
    {
        /// <summary>
        /// Recalculate all given SBC range against all given loopup number range per each customer
        /// </summary>
        /// <param name="existingSbcRange"></param>
        /// <param name="existingLoopUpNumberRange"></param>
        public static void RecalculateRange(List<RibbonNumberRange> existingSbcRange, List<LoopupNumberRange> existingLoopUpNumberRange)
        {
            // recalculate per customer
            var byCustomer = from p in existingSbcRange
                             group p by p.Customer into g
                             select new
                             {
                                 Customer = g.Key,
                                 SbcRanges = g.ToList(),
                             };

            byCustomer.ToList().ForEach(s =>
            {
                var finalRange = new List<RibbonNumberRange>();
                var numberOfDigits = s.SbcRanges.First().NumberOfDigits;
                var allCustomersNumberRange = existingLoopUpNumberRange.Where(e => e.Numbers.All(n => n.Customer == s.Customer));

                // per loopup number range
                allCustomersNumberRange
                .ToList()
                .ForEach(e =>
                {
                    List<string> sbcRanges;
                    if (finalRange.Count == 0)
                    {
                        sbcRanges = s.SbcRanges.Select(m => m.RibbonSbcRange).ToList();
                    }
                    else
                    {
                        sbcRanges = finalRange.Select(m => m.RibbonSbcRange).ToList();
                    }

                    var existingPhoneNumbers = e.Numbers.Where(n => n.Customer == s.Customer).Select(m => m.Number).ToList();
                    // is there any inefficient ranges
                    if (!IsEfficientRanges(existingPhoneNumbers, sbcRanges, numberOfDigits, out var inEfficientRange))
                    {
                        // TODO: remove that sbcRange or add to collection to be removed later

                        // reconstruct new range from the inefficient range
                        var validRange = s.SbcRanges.Where(s => !inEfficientRange.Any(i => s.RibbonSbcRange == i)).ToList();
                        finalRange.AddRange(validRange);

                        inEfficientRange.ForEach(e =>
                        {
                            var newRange = new List<RibbonNumberRange>();

                            var totalUnallocatedNumbers = GetUnallocatedNumbers(existingPhoneNumbers, e, numberOfDigits);
                            if (totalUnallocatedNumbers.Item1.Count > 0)
                            {
                                var rangeStart = existingPhoneNumbers.Min();
                                var rangeEnd = validRange.First().Coverage.Min();
                                newRange = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString()).Select(n => new RibbonNumberRange
                                {
                                    Customer = s.Customer,
                                    RibbonSbcRange = n,
                                    RangeStart = rangeStart,
                                    RangeEnd = rangeEnd
                                })
                                .ToList();

                                newRange.ForEach(n =>
                                {
                                    if (!finalRange.Any(f => n.RibbonSbcRange.StartsWith(f.RibbonSbcRange)))
                                    {
                                        finalRange.Add(n);
                                    }
                                });
                            }

                            if (totalUnallocatedNumbers.Item2.Count > 0)
                            {
                                var rangeStart = validRange.Last().Coverage.Min();
                                var rangeEnd = existingPhoneNumbers.Max();
                                newRange = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString()).Select(n => new RibbonNumberRange
                                {
                                    Customer = s.Customer,
                                    RibbonSbcRange = n,
                                    RangeStart = rangeStart,
                                    RangeEnd = rangeEnd
                                })
                                .ToList();

                                newRange.ForEach(n =>
                                {
                                    if (!finalRange.Any(f => n.RibbonSbcRange.StartsWith(f.RibbonSbcRange)))
                                    {
                                        finalRange.Add(n);
                                    }
                                });
                            }
                        });
                    }
                    else
                    {
                        // it maybe an efficient range, but there could be numbers that have no range associated
                        // try to generate the range
                        var res = RangeToRibbonSBC(existingPhoneNumbers.Min().ToString(), existingPhoneNumbers.Max().ToString());
                        finalRange.AddRange(res.Select(r => new RibbonNumberRange
                        {
                            Customer = s.Customer,
                            RibbonSbcRange = r,
                            RangeStart = existingPhoneNumbers.Min(),
                            RangeEnd = existingPhoneNumbers.Max()
                        }));
                    }
                });

                // verify the total numbers equal to translated sbc numbers
                // note: this verify is just for testing purpose
                VerifyRecalculation(finalRange, allCustomersNumberRange);
            });
        }

        /// <summary>
        /// Get unallocated numbers to range that could be bigger than it should
        /// The left part if the range unnecessarily cover number below the min number
        /// The right part if the range unnecessarily cover number above the max number
        /// </summary>
        /// <param name="existingNumbers"></param>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public static Tuple<List<uint>, List<uint>> GetUnallocatedNumbers(List<uint> existingNumbers, string sbcRange, int numberOfDigits)
        {
            List<uint> LeftPart = new List<uint>();
            List<uint> RightPart = new List<uint>();

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

        public static uint GetTotalUnallocatedNumbers(List<uint> existingNumbers, string sbcRange, int numberOfDigits)
        {
            uint unallocatedNumbers = default;

            var translatedNumbers = RangeToNumbers(sbcRange, numberOfDigits);

            if (translatedNumbers.Max() < existingNumbers.Min() || translatedNumbers.Min() > existingNumbers.Max())
            {
                // out of range, skip
                return default;
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

        /// <summary>
        /// Check if the range is an effecient range by measuring number different by the order of log10
        /// If it's more than 1, considered as ineffecient
        /// CAUTION: This does not include where there is still uncovered numbers by the given sbcRange
        /// </summary>
        /// <param name="existingNumbers"></param>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <param name="inEfficientRange"></param>
        /// <returns></returns>
        public static bool IsEfficientRanges(List<uint> existingNumbers, List<string> sbcRange, int numberOfDigits, out List<string> inEfficientRange)
        {
            bool isEfficient = true; ;
            inEfficientRange = new List<string>();
            foreach (var s in sbcRange)
            {
                var totalUnallocatedNumbers = GetTotalUnallocatedNumbers(existingNumbers, s, numberOfDigits);
                if (totalUnallocatedNumbers > 0)
                {
                    isEfficient = false;
                    inEfficientRange.Add(s);
                }
            }

            return isEfficient;
        }

        /// <summary>
        /// Helper to translate single sbc range to numbers, need to improve performance
        /// </summary>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public static List<uint> RangeToNumbers(string sbcRange, int numberOfDigits)
        {
            List<uint> numbers = new List<uint>();
            var degree = (uint)Math.Pow(10, numberOfDigits - sbcRange.Length);
            uint _number = uint.Parse(sbcRange) * degree;
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
        public static List<uint> RangesToNumbers(List<string> sbcRange, int numberOfDigits)
        {
            List<uint> numbers = new List<uint>();
            sbcRange.ForEach(r =>
            {
                numbers.AddRange(RangeToNumbers(r, numberOfDigits));
            });

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

        private static void VerifyRecalculation(List<RibbonNumberRange> finalRange, IEnumerable<LoopupNumberRange> allCustomersNumberRange)
        {
            var allCustomerNumbers = allCustomersNumberRange.SelectMany(a => a.Numbers);
            var allTranslatedRangeToNumbers = finalRange.Sum(f => f.Coverage.Count);
            if (allTranslatedRangeToNumbers != allCustomerNumbers.Count())
            {
                throw new Exception("Unmatch");
            }
        }

    }
}
