using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RibbonSBCRangeConverter
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

        private List<int> _coverage = null;

        public List<int> Coverage
        {
            get
            {
                if (_coverage != null)
                    return _coverage;
                _coverage = RangeToNumbers().Distinct().ToList();

                return _coverage;
            }
        }

        public int RangeStart { get; set; }

        public int RangeEnd { get; set; }

        public List<int> Numbers { get; private set; }

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
        public List<int> RangeToNumbers()
        {
            List<int> numbers = new List<int>();
            var rangeLength = RibbonSbcRange.Length;
            var degree = (int)Math.Pow(10, NumberOfDigits - rangeLength);
            var _number = int.Parse(RibbonSbcRange) * degree;
            var _upperlimit = _number + (degree - 1);
            for (var y = _number; y <= _upperlimit; y++)
            {
                numbers.Add(y);
            }

            return numbers;
        }
    }
}
