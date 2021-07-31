using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Text;
using NumberRangeConverter;

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

            bool isEfficientRange = NumberRangeHelper.IsEfficientRanges(numbers, sbcRange, numberOfDigits, out List<string> inEfficientRange);
            bool isEfficientRange2 = NumberRangeHelper.IsEfficientRanges(numbers, efficientSbcRange, numberOfDigits, out List<string> inEfficientRange2);

            Assert.False(isEfficientRange);
            Assert.Equal(1, inEfficientRange.Count);

            Assert.True(isEfficientRange2);
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

            // Ardall's numbers contain multiple range
            // but one of the SBC range is too general which cover outside his number ranges
            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(123440, 123550, "Ardall")
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(123740, 123950, "Ardall")
            });

            // Leo's range contain multiple range
            // but the second range have no association with any Ribbon SBC range
            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(223450, 223900, "Leo")
            });

            loopupNumberRange.Add(new LoopupNumberRange
            {
                Numbers = NumberHelper.SetNumbers(1415251000, 1415990201, "Leo")
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

            // Recalculate the ranges so we get the proper SBC ranges
            // if there is no expcetion, meaning the final ranges is created succesfully and still cover all of users ranges
            NumberRangeHelper.RecalculateRange(sbcRange, loopupNumberRange);
            var exception = Record.Exception(() => NumberRangeHelper.RecalculateRange(sbcRange, loopupNumberRange));
            Assert.Null(exception);
        }
    }
}
