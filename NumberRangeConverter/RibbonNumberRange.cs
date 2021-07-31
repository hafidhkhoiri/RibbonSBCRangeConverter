using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumberRangeConverter
{
    public class RibbonNumberRange
    {
        public string RibbonSbcRange { get; set; }

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

        private List<uint> _coverage = null;

        public List<uint> Coverage
        {
            get
            {
                if (_coverage != null)
                    return _coverage;
                _coverage = RangeToNumbers().Distinct().ToList();

                return _coverage;
            }
        }

        public uint RangeStart { get; set; }

        public uint RangeEnd { get; set; }

        public List<uint> Numbers { get; private set; }

        public void SetNumbers()
        {
            for (var m = RangeStart; m <= RangeEnd; m++)
            {
                Numbers.Add(m);
            }
        }

        /// <summary>
        /// Helper to translate range to numbers, need to improve performance
        /// </summary>
        /// <returns></returns>
        public List<uint> RangeToNumbers()
        {
            List<uint> numbers = new List<uint>();
            var rangeLength = RibbonSbcRange.Length;
            var degree = (uint)Math.Pow(10, NumberOfDigits - rangeLength);
            var _number = uint.Parse(RibbonSbcRange) * degree;
            var _upperlimit = _number + (degree - 1);
            for (var y = _number; y <= _upperlimit; y++)
            {
                numbers.Add(y);
            }

            return numbers;
        }
    }
}
