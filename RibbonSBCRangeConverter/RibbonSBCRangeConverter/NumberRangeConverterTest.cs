using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Text;
using NumberRangeConverter;
using System.Collections.Concurrent;

namespace NumberRangeConverterTest
{
    public class NumberRangeConverterTest
    {
        [Fact]
        public void ConvertingToRibbonSBCAlsoCanConvertToActualNumbers()
        {
            var rangeStart = 340601009;
            var rangeEnd = 340761009;
            var totalNumbers = (rangeEnd - rangeStart + 1);
            var res = NumberRangeHelper.RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            var back = NumberRangeHelper.RangesToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
            Assert.Equal(back.Count, totalNumbers);
            Assert.Equal(44, res.Count);

            rangeStart = 340601000;
            rangeEnd = 340761009;
            totalNumbers = (rangeEnd - rangeStart + 1);
            res = NumberRangeHelper.RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            back = NumberRangeHelper.RangesToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
            Assert.Equal(back.Count, totalNumbers);
            Assert.Equal(26, res.Count);

            rangeStart = 1415251000;
            rangeEnd = 1415259020;
            totalNumbers = (rangeEnd - rangeStart + 1);
            res = NumberRangeHelper.RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            back = NumberRangeHelper.RangesToNumbers(res, rangeStart.ToString().Length).Distinct().ToList();
            Assert.Equal(back.Count, totalNumbers);
            Assert.Equal(11, res.Count);
        }

        [Fact]
        public void ConvertingToRibbonSBCCorreclty()
        {
            var rangeStart = 12340;
            var rangeEnd = 12350;
            var res = NumberRangeHelper.RangeToRibbonSBC(rangeStart.ToString(), rangeEnd.ToString());
            Assert.Equal(2, res.Count);

            Assert.Equal(new List<string> { "1234", "12350" }, res);
        }
        [Fact]
        public void CanDetectInEfficientRange()
        {
            List<UInt64> numbers = new List<UInt64>();
            UInt64 rangeStart = 123400;
            UInt64 rangeEnd = 123500;

            for (var m = rangeStart; m <= rangeEnd; m++)
            {
                numbers.Add(m);
            }

            var sbcRange = new List<string> { "123", "1234", "999" };
            var numberOfDigits = rangeStart.ToString().Length;
            var efficientSbcRange = new List<string> { "1234", "123450" };

            var checkResult = NumberRangeHelper.IsEfficientRange(numbers, sbcRange[0], numberOfDigits);
            Assert.Equal(RangeStatus.InEfficient, checkResult);

            checkResult = NumberRangeHelper.IsEfficientRange(numbers, sbcRange[1], numberOfDigits);
            Assert.Equal(RangeStatus.Efficient, checkResult);

            checkResult = NumberRangeHelper.IsEfficientRange(numbers, sbcRange[2], numberOfDigits);
            Assert.Equal(RangeStatus.OutOfRange, checkResult);
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

            // Ardall's numbers contain multiple range
            // but one of the SBC range is too general which cover outside his number ranges
            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(123440, 123550, "Ardall")
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(126740, 126950, "Ardall")
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(923450, 923460, "Leo")
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(191340, 192340, "Leo")
            });


            var sbcRange = new List<RibbonNumberRange> {
                new RibbonNumberRange{
                    RibbonSbcRange = "123",
                    Customer = "Ardall",
                    NumberOfDigits = 6,
                },
                new RibbonNumberRange{
                    RibbonSbcRange = "123458",
                    Customer = "Ardall",
                    NumberOfDigits = 6,
                },
                new RibbonNumberRange{
                    RibbonSbcRange = "13345",
                    Customer = "Ardall",
                    NumberOfDigits = 6,
                },
                new RibbonNumberRange{
                    RibbonSbcRange = "92345",
                    Customer = "Leo",
                    NumberOfDigits = 6,
                },
                new RibbonNumberRange{
                    RibbonSbcRange = "22345",
                    Customer = "Range without numbers",
                    NumberOfDigits = 6,
                },
            };

            // Recalculate the ranges so we get the proper SBC ranges
            // if there is no expcetion, meaning the final ranges is created succesfully and still cover all of users ranges
            var exception = Record.Exception(() => NumberRangeHelper.RecalculateRange(sbcRange, loopupNumberRange));
            Assert.Null(exception);
        }

        [Fact]
        public void addNewNumberShouldRecalculateRangeCorrectly()
        {
            var numberOfDigits = 6;

            var loopupNumberRange = new List<LoopupNumberRange>();

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(123450, 123459, "Ardall")
            });

            var sbcRanges = new List<RibbonNumberRange> {
                new RibbonNumberRange{
                    RibbonSbcRange = "12345",
                    Customer = "Ardall",
                    NumberOfDigits = numberOfDigits,
                },
            };

            // add 1 number
            loopupNumberRange.First().Numbers.Add(new PhoneNumber
            {
                Customer = "Ardall",
                Number = 123460
            });

            var res = NumberRangeHelper.RecalculateRange(sbcRanges, loopupNumberRange, MaxDegreeOfParallelism: 1);
            var ardallNewRange = res.FirstOrDefault(c => c.Customer == "Ardall").RibbonNumberRanges.Select(r => r.RibbonSbcRange).ToList();

            Assert.Equal(new List<string> { "12345", "123460" }, ardallNewRange);
        }

        [Fact]
        public void removeNumberShouldRecalculateRangeCorrectly()
        {
            var numberOfDigits = 6;

            var loopupNumberRange = new List<LoopupNumberRange>();

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(123450, 123460, "Ardall")
            });

            var sbcRanges = new List<RibbonNumberRange> {
                new RibbonNumberRange{
                    RibbonSbcRange = "12345",
                    Customer = "Ardall",
                    NumberOfDigits = numberOfDigits,
                },
                new RibbonNumberRange{
                    RibbonSbcRange = "123460",
                    Customer = "Ardall",
                    NumberOfDigits = numberOfDigits,
                },
            };

            // remove 1 number
            foreach(var c in loopupNumberRange)
            {
                c.Numbers = c.Numbers.TakeWhile(n => n.Number != 123460).ToList();
            }

            var res = NumberRangeHelper.RecalculateRange(sbcRanges, loopupNumberRange, MaxDegreeOfParallelism: 1);
            var ardallNewRange = res.FirstOrDefault(c => c.Customer == "Ardall").RibbonNumberRanges.Select(r => r.RibbonSbcRange).ToList();

            Assert.Equal(new List<string> { "12345" }, ardallNewRange);
        }
    }
}
