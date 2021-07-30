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
            var back = RangeToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
            Assert.Equal(back.Count, totalNumbers);
            Assert.Equal(44, res.Count);

            rangeStart = 340601000;
            rangeEnd = 340761009;
            totalNumbers = (rangeEnd - rangeStart + 1);
            res = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            back = RangeToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
            Assert.Equal(back.Count, totalNumbers);
            Assert.Equal(26, res.Count);

            rangeStart = 340600500;
            rangeEnd = 340760009;
            totalNumbers = (rangeEnd - rangeStart + 1);
            res = RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            back = RangeToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
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
            List<int> numbers = new List<int>();
            var rangeStart = 12340;
            var rangeEnd = 12350;

            for (var m = rangeStart; m <= rangeEnd; m++)
            {
                numbers.Add(m);
            }

            var sbcRange = new List<string> { "123" };
            var numberOfDigits = rangeStart.ToString().Length;
            var effectiveSbcRange = new List<string> { "1234", "123450" };

            Assert.False(IsEffectiveRange(numbers, sbcRange, numberOfDigits));
            Assert.True(IsEffectiveRange(numbers, effectiveSbcRange, numberOfDigits));
        }

        public bool IsEffectiveRange(List<int> existingNumbers, List<string> sbcRange, int numberOfDigits)
        {
            var totalNumbersInRibbonSBCRange = RangeToNumbers(sbcRange, numberOfDigits);

            // this compromise till 10%, less than consider as ineffective range
            // note: this temporary definition of effective range
            return (existingNumbers.Count / totalNumbersInRibbonSBCRange.Count) > 0.1;
        }

        /// <summary>
        /// This method to translate back the Ribbon SBC range to actual numbers
        /// </summary>
        /// <param name="sbcRange"></param>
        /// <param name="numberOfDigits"></param>
        /// <returns></returns>
        public List<string> RangeToNumbers(List<string> sbcRange, int numberOfDigits)
        {
            List<string> numbers = new List<string>();
            sbcRange.ForEach(r =>
            {
                var rangeLength = r.ToString().Length;
                var degree = Math.Pow(10, numberOfDigits - rangeLength);
                var _number = int.Parse(r) * degree;
                var _upperlimit = _number + (degree - 1);
                for (var y = _number; y <= _upperlimit; y++)
                {
                    numbers.Add(y.ToString());
                }
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
