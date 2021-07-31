using System;
using System.Collections.Generic;
using System.Text;

namespace NumberRangeConverter
{
    public class NumberHelper
    {
        public static List<PhoneNumber> SetNumbers(uint RangeStart, uint RangeEnd, string Customer)
        {
            List<PhoneNumber> result = new List<PhoneNumber>();

            for (var m = RangeStart; m <= RangeEnd; m++)
            {
                result.Add(new PhoneNumber
                {
                    Customer = Customer,
                    Number = m
                });
            }

            return result;
        }

    }
}
