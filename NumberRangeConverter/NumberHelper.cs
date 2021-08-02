using System;
using System.Collections.Generic;

namespace NumberRangeConverter
{
    public class NumberHelper
    {
        public static List<PhoneNumber> SetNumbers(UInt64 RangeStart, UInt64 RangeEnd, string Customer)
        {
            List<PhoneNumber> result = new List<PhoneNumber>();
            for (UInt64 m = RangeStart; m <= RangeEnd; m++)
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
