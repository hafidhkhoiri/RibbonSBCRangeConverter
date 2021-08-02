using System;
using System.Collections.Generic;
using System.Linq;

namespace NumberRangeConverter
{
    public class RibbonNumberRange
    {
        public string RibbonSbcRange { get; set; }

        public RangeStatus Status { get; set; }

        public string Customer { get; set; }

        private int _numberOfDigits;
        public int NumberOfDigits
        {
            get
            {
                return _numberOfDigits > 0 ? _numberOfDigits : RangeStart.ToString().Length;
            }
            set
            {
                _numberOfDigits = value;
            }
        }

        private List<UInt64> _coverage = null;

        public List<UInt64> Coverage
        {
            get
            {
                if (_coverage != null)
                    return _coverage;
                _coverage = RangeToNumbers().Distinct().ToList();

                return _coverage;
            }
        }

        public UInt64 RangeStart { get; set; }

        public UInt64 RangeEnd { get; set; }

        /// <summary>
        /// Helper to translate range to numbers, need to improve performance
        /// </summary>
        /// <returns></returns>
        public List<UInt64> RangeToNumbers()
        {
            List<UInt64> numbers = new List<UInt64>();
            var rangeLength = RibbonSbcRange.Length;
            var degree = (UInt64)Math.Pow(10, NumberOfDigits - rangeLength);
            var _number = UInt64.Parse(RibbonSbcRange) * degree;
            var _upperlimit = _number + (degree - 1);

            for (var y = _number; y <= _upperlimit; y++)
            {
                numbers.Add(y);
            }

            return numbers;
        }
    }
}
