using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace NetCoreConfigDemo
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly IConfiguration _configuration;
        private readonly Options1 _settings1;
        private readonly Options2 _settings2;

        public ConfigController(
            ILogger<ConfigController> logger,
            IConfiguration configuration,
            IOptionsMonitor<Options1> settings1,
            IOptionsMonitor<Options2> settings2)
        {
            _logger = logger;
            _configuration = configuration;
            _settings1 = settings1.CurrentValue;
            _settings2 = settings2.CurrentValue;
        }

        [HttpGet]
        public string Get()
        {
            _logger.LogInformation($"=====Options1======{Newtonsoft.Json.JsonConvert.SerializeObject(_settings1)}======");
            _logger.LogInformation($"=====Options2======{Newtonsoft.Json.JsonConvert.SerializeObject(_settings2)}======");
            _logger.LogInformation($"=====Raw With ConnectionStr======{_configuration.GetConnectionString("Default")}======");
            _logger.LogInformation($"=====Raw With Other======{_configuration["other"]}======");

            return "ok";
        }
    }
}
