using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Text;

namespace RibbonSBCRangeConverter
{
    public class RibbonSCBConverterTest
    {
        [Fact]
        public void ConvertingToRibbonSBCAlsoCanConvertToActualNumbers()
        {
            var rangeStart = 340601009;
            var rangeEnd = 340761009;
            var totalNumbers = (rangeEnd - rangeStart + 1);
            var res = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            var back = RangesToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
            Assert.Equal(back.Count, totalNumbers);
            Assert.Equal(44, res.Count);

            rangeStart = 340601000;
            rangeEnd = 340761009;
            totalNumbers = (rangeEnd - rangeStart + 1);
            res = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            back = RangesToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
            Assert.Equal(back.Count, totalNumbers);
            Assert.Equal(26, res.Count);

            rangeStart = 340600500;
            rangeEnd = 340760009;
            totalNumbers = (rangeEnd - rangeStart + 1);
            res = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            back = RangesToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
            Assert.Equal(back.Count, totalNumbers);
            Assert.Equal(30, res.Count);
        }

        [Fact]
        public void ConvertingToRibbonSBCCorreclty()
        {
            var rangeStart = 12340;
            var rangeEnd = 12350;
            var res = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            Assert.Equal(2, res.Count);
            Assert.Equal(new List<string> { "1234", "12350" }, res);
        }

        [Fact]
        public void CanDetectInEffectiveRange()
        {
            List<uint> numbers = new List<uint>();
            uint rangeStart = 123400;
            uint rangeEnd = 123500;

            for (var m = rangeStart; m <= rangeEnd; m++)
            {
                numbers.Add(m);
            }

            var sbcRange = new List<string> { "123", "1234" };
            var numberOfDigits = rangeStart.ToString().Length;
            var efficientSbcRange = new List<string> { "1234", "123450" };

            bool isEffectiveRange = IsEfficientRanges(numbers, sbcRange, numberOfDigits, out List<string> inEfficientRange);
            bool isEffectiveRange2 = IsEfficientRanges(numbers, efficientSbcRange, numberOfDigits, out List<string> inEfficientRange2);

            Assert.False(isEffectiveRange);
            Assert.Equal(1, inEfficientRange.Count);

            Assert.True(isEffectiveRange2);
            Assert.Empty(inEfficientRange2);
        }

        [Fact]
        public void ShouldRecalculateRangeCorrectly()
        {
            var loopupNumberRange = new List<LoopupNumberRange>();

            // unassigned numbers
            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(121340, 121550, null)
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(122340, 122350, null)
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(123440, 123550, "Ardall")
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(123740, 123950, "Ardall")
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(223450, 223900, "Leo")
            });

            var sbcRange = new List<RibbonNumberRange> {
                new RibbonNumberRange{
                    RibbonSbcRange = "123",
                    Customer = "Ardall",
                    NumberOfDigits = 6,
                },
                new RibbonNumberRange{
                    RibbonSbcRange = "12345",
                    Customer = "Ardall",
                    NumberOfDigits = 6,
                },
                new RibbonNumberRange{
                    RibbonSbcRange = "22345",
                    Customer = "Leo",
                    NumberOfDigits = 6,
                },
            };

            RecalculateRange(sbcRange, loopupNumberRange);
            var exception = Record.Exception(() => RecalculateRange(sbcRange, loopupNumberRange));
            Assert.Null(exception);
        }

        public void RecalculateRange(List<RibbonNumberRange> existingSbcRange, List<LoopupNumberRange> existingLoopUpNumberRange)
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
                VerifyRecalculation(finalRange, numberOfDigits, allCustomersNumberRange);
            });
        }

        private void VerifyRecalculation(List<RibbonNumberRange> finalRange, int numberOfDigits, IEnumerable<LoopupNumberRange> allCustomersNumberRange)
        {
            var allCustomerNumbers = allCustomersNumberRange.SelectMany(a => a.Numbers);
            var allTranslatedRangeToNumbers = RangesToNumbers(finalRange.Select(m => m.RibbonSbcRange).ToList(), numberOfDigits);
            if (allTranslatedRangeToNumbers.Count != allCustomerNumbers.Count())
            {
                throw new Exception("Unmatch");
            }
        }

        /// <summary>
        /// Get unallocated numbers to range that could be bigger than it should
        /// The left part if the range cover number below the min number
        /// The right part if the range cover number above the max number
        /// </summary>
        /// <param name="existingNumbers"></param>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public Tuple<List<uint>, List<uint>> GetUnallocatedNumbers(List<uint> existingNumbers, string sbcRange, int numberOfDigits)
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

        public uint GetTotalUnallocatedNumbers(List<uint> existingNumbers, string sbcRange, int numberOfDigits)
        {
            uint unallocatedNumbers = default;

            var translatedNumbers = RangeToNumbers(sbcRange, numberOfDigits);

            if (translatedNumbers.Max() < existingNumbers.Min() || translatedNumbers.Min() > existingNumbers.Max()) {
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
        /// </summary>
        /// <param name="existingNumbers"></param>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <param name="inEfficientRange"></param>
        /// <returns></returns>
        public bool IsEfficientRanges(List<uint> existingNumbers, List<string> sbcRange, int numberOfDigits, out List<string> inEfficientRange)
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
        /// Helper to translate range to numbers, need to improve performance
        /// </summary>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public List<uint> RangeToNumbers(string sbcRange, int numberOfDigits)
        {
            List<uint> numbers = new List<uint>();
            var rangeLength = sbcRange.ToString().Length;
            var degree = (uint)Math.Pow(10, numberOfDigits - rangeLength);
            uint _number = uint.Parse(sbcRange) * degree;
            var _upperlimit = _number + (degree - 1);
            for (var y = _number; y <= _upperlimit; y++)
            {
                numbers.Add(y);
            }

            return numbers;
        }

        /// <summary>
        /// This method to translate back the Ribbon SBC range to actual numbers
        /// </summary>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public List<uint> RangesToNumbers(List<string> sbcRange, int numberOfDigits)
        {
            List<uint> numbers = new List<uint>();
            sbcRange.ForEach(r =>
            {
                numbers.AddRange(RangeToNumbers(r, numberOfDigits));
            });

            return numbers;
        }

        /// <summary>
        /// This method is to convert given range into Ribon SBC range
        /// </summary>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <returns></returns>
        public List<string> RangeToRibbonSBC(string rangeStart, string rangeEnd)
        {
            List<string> ranges = new List<string>();
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
                                ranges.Add(newRange.ToString());
                            }
                            else
                            {
                                // iterate from last digit till the upper limit
                                for (var k = lastDigit; k <= upperLimit; k++)
                                {
                                    if (k > 0 && k < upperLimit && k >= lastDigit)
                                    {
                                        ranges.Add(newRange.ToString());
                                    }
                                    else if (k == upperLimit) // last iteration
                                    {
                                        var newStart = string.Concat(prefix, k * (Math.Pow(10, endLeft.Length - 1)));

                                        // if it's not the same value as the end range, do recursive to translate the range
                                        if (newStart.ToString() != rangeEnd)
                                        {
                                            List<string> additionalRange = RangeToRibbonSBC(newStart.ToString(), rangeEnd);
                                            ranges.AddRange(additionalRange);
                                        }
                                        else
                                        {
                                            // otherwise add it as is
                                            ranges.Add(newStart.ToString());
                                        }
                                    }
                                    else if (k == 0 || k == lastDigit)
                                    {
                                        ranges.Add(newRange.ToString());
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
                                    ranges.Add(newRange.ToString());
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

            return ranges;
        }
    }
}
