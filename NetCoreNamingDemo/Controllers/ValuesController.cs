using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetCoreNamingDemo.Controllers
{
    [Route("")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly Nacos.V2.INacosNamingService _svc;
        private readonly IHttpClientFactory _factory;

        public ValuesController(Nacos.V2.INacosNamingService svc, IHttpClientFactory factory)
        {
            _svc = svc;
            _factory = factory;
        }

        // GET /
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /a
        [HttpGet("a")]
        public async Task<string> TestAsync()
        {
            // 找出一个健康的实例
            var instance = await _svc.SelectOneHealthyInstance("NetCoreNamingDemo", "DEFAULT_GROUP");
            var host = $"{instance.Ip}:{instance.Port}";

            // 根据 secure 来判断服务要不要用 https，
            // 这里是约定，参考了 spring cloud 那边，不是强制的，也可以用其他标识
            var baseUrl = instance.Metadata.TryGetValue("secure", out _)
                ? $"https://{host}"
                : $"http://{host}";

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "empty";
            }

            var url = $"{baseUrl}";

            var client = _factory.CreateClient();

            var resp = await client.GetAsync(url);
            return await resp.Content.ReadAsStringAsync();
        }
    }
}
