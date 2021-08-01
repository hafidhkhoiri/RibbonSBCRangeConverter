using System;
using System.Collections.Generic;

namespace NumberRangeConverter
{
    public class LoopupNumberRange
    {
        public Guid Id { get; } = Guid.NewGuid();

        public List<PhoneNumber> Numbers { get; set; }
    }
}
