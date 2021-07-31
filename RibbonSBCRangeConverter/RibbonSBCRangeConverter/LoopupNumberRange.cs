using System;
using System.Collections.Generic;
using System.Text;

namespace RibbonSBCRangeConverter
{
    public class LoopupNumberRange
    {
        public Guid Id { get; } = Guid.NewGuid();

        public List<PhoneNumber> Numbers { get; set; }
    }
}
