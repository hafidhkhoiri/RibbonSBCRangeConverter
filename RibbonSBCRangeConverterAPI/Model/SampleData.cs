using NumberRangeConverter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RibbonSBCRangeConverterAPI.Model
{
    public class SampleData
    {
        public List<RibbonNumberRange> SbcRanges { get;set;}

        public List<NumbersRange> NumbersRanges { get; set; }

        public List<LoopupNumberRange> LoopupNumberRanges { get; set; } = new List<LoopupNumberRange>();

        public SampleData()
        {
        }

        public void SetNumbers()
        {
            var byCustomer = from p in NumbersRanges
                             group p by p.Customer into g
                             select new
                             {
                                 Customer = g.Key,
                                 Numbers = g.ToList(),
                             };

            Parallel.ForEach(byCustomer, i =>
            {
                foreach(var j in i.Numbers)
                {
                    ConcurrentBag<PhoneNumber> _numbers = new ConcurrentBag<PhoneNumber>();
                    for (UInt64 k = j.RangeStart; k <= j.RangeEnd; k++)
                    {
                        _numbers.Add(new PhoneNumber
                        {
                            Customer = i.Customer,
                            Number = k
                        });
                    }

                    this.LoopupNumberRanges.Add(new LoopupNumberRange
                    {
                        Numbers = _numbers.ToList()
                    });
                }
            });
        }
    }

    public class NumbersRange
    {
        public string Customer { get; set; }

        public UInt64 RangeStart { get; set; }

        public UInt64 RangeEnd { get; set; }
    }
}
