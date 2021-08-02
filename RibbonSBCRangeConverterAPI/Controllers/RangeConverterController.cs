using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RibbonSBCRangeConverterAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting.Server;
using NumberRangeConverter;

namespace RibbonSBCRangeConverterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RangeConverterController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<RangeConverterController> _logger;
        private readonly SampleData _sampleData;

        public RangeConverterController(ILogger<RangeConverterController> logger, SampleData sampleData)
        {
            _logger = logger;
            _sampleData = sampleData;
        }

        [HttpGet("treeview")]
        public JsonResult GetOverview()
        {
            var perCustomer = from p in _sampleData.NumbersRanges
                              group p by p.Customer into g
                              select new
                              {
                                  Customer = g.Key,
                                  NumbersRange = g.ToList(),
                              };

            return new JsonResult(perCustomer);
        }

        [HttpGet("loopupNumbers")]
        public List<LoopupNumberRange> GetLoopupNumbers()
        {
            return _sampleData.LoopupNumberRanges;
        }

        [HttpGet("sbcRanges")]
        public List<NumbersRange> GetNumberRange()
        {
            return _sampleData.NumbersRanges;
        }
    }
}
