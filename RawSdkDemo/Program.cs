using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nacos.V2;
using Nacos.V2.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace RawSdkDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = InitServiceProvider();

            INacosConfigService configSvc = serviceProvider.GetService<INacosConfigService>();
            INacosNamingService namingSvc = serviceProvider.GetService<INacosNamingService>();

            DemoConfigListener listener = new DemoConfigListener();
            await PublishConfig(configSvc);
            await GetConfig(configSvc);
            await RemoveConfig(configSvc);
            await ListenConfig(configSvc, listener);

            DemoEventListener eventListener = new DemoEventListener();
            await RegisterInstance(namingSvc);
            await GetAllInstances(namingSvc);
            await DeregisterInstance(namingSvc);
            await Subscribe(namingSvc, eventListener);
            
            Console.ReadKey();
        }

        #region 服务相关操作示例
        static async Task RegisterInstance(INacosNamingService svc, int port = 9999)
        {
            await Task.Delay(500);

            var instace = new Nacos.V2.Naming.Dtos.Instance
            {
                ServiceName = "demo-svc1",
                ClusterName = Nacos.V2.Common.Constants.DEFAULT_CLUSTER_NAME,
                Ip = "127.0.0.1",
                Port = port,
                Enabled = true,
                Ephemeral = true,
                Healthy = true,
                Weight = 100,
                InstanceId = $"demo-svc1-127.0.0.1-{port}",
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "m1", "v1" },
                    { "m2", "v2" },
                }
            };

            // 注册实例有很多重载，选适合自己的即可。
            await svc.RegisterInstance(instace.ServiceName, Nacos.V2.Common.Constants.DEFAULT_GROUP, instace);
            Console.WriteLine($"======================注册实例成功");
        }

        static async Task GetAllInstances(INacosNamingService svc)
        {
            await Task.Delay(500);

            // 获取全部实例有很多重载，选适合自己的即可。最后一个参数表明要不要订阅这个服务
            // SelectInstances, SelectOneHealthyInstance 是另外的方法可以获取服务信息。
            var list = await svc.GetAllInstances("demo-svc1", Nacos.V2.Common.Constants.DEFAULT_GROUP, false);
            Console.WriteLine($"======================获取实例成功，{Newtonsoft.Json.JsonConvert.SerializeObject(list)}");
        }

        static async Task DeregisterInstance(INacosNamingService svc)
        {
            await Task.Delay(500);

            // 注销实例有很多重载，选适合自己的即可。
            await svc.DeregisterInstance("demo-svc1", Nacos.V2.Common.Constants.DEFAULT_GROUP, "127.0.0.1", 9999);
            Console.WriteLine($"======================注销实例成功");
        }

        static async Task Subscribe(INacosNamingService svc, IEventListener listener)
        {
            // 订阅服务变化
            await svc.Subscribe("demo-svc1", Nacos.V2.Common.Constants.DEFAULT_GROUP, listener);

            // 模拟服务变化，listener会收到变更信息
            await RegisterInstance(svc, 9997);

            await Task.Delay(3000);

            // 取消订阅
            await svc.Unsubscribe("demo-svc1", Nacos.V2.Common.Constants.DEFAULT_GROUP, listener);

            // 服务变化后，listener不会收到变更信息
            await RegisterInstance(svc);

            await Task.Delay(1000);
        }

        class DemoEventListener : IEventListener
        {
            public Task OnEvent(IEvent @event)
            {
                if (@event is Nacos.V2.Naming.Event.InstancesChangeEvent e)
                {
                    Console.WriteLine($"==========收到服务变更事件=======》{Newtonsoft.Json.JsonConvert.SerializeObject(e)}");
                }

                return Task.CompletedTask;
            }
        } 
        #endregion

        #region 配置相关操作示例
        static async Task PublishConfig(INacosConfigService svc)
        {
            var dataId = "demo-dateid";
            var group = "demo-group";
            var val = "test-value-" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

            await Task.Delay(500);
            var flag = await svc.PublishConfig(dataId, group, val);
            Console.WriteLine($"======================发布配置结果，{flag}");
        }

        static async Task GetConfig(INacosConfigService svc)
        {
            var dataId = "demo-dateid";
            var group = "demo-group";

            await Task.Delay(500);
            var config = await svc.GetConfig(dataId, group, 5000L);
            Console.WriteLine($"======================获取配置结果，{config}");
        }

        static async Task RemoveConfig(INacosConfigService svc)
        {
            var dataId = "demo-dateid";
            var group = "demo-group";

            await Task.Delay(500);
            var flag = await svc.RemoveConfig(dataId, group);
            Console.WriteLine($"=====================删除配置结果，{flag}");
        }

        static async Task ListenConfig(INacosConfigService svc, IListener listener)
        {
            var dataId = "demo-dateid";
            var group = "demo-group";

            // 添加监听
            await svc.AddListener(dataId, group, listener);

            await Task.Delay(500);

            // 模拟配置变更，listener会收到变更信息
            await PublishConfig(svc);

            await Task.Delay(500);
            await PublishConfig(svc);

            await Task.Delay(500);

            // 移除监听
            await svc.RemoveListener(dataId, group, listener);

            // 配置变更后，listener不会收到变更信息
            await PublishConfig(svc);
        }

        class DemoConfigListener : IListener
        {
            public void ReceiveConfigInfo(string configInfo)
            {
                Console.WriteLine($"================收到配置变更信息了 ===》{configInfo}");
            }
        }
        #endregion

        #region 初始化
        static IServiceProvider InitServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Config(x =>
            {
                x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs-test";

                /*x.UserName = "nacos";
               x.Password = "nacos";*/

                // swich to use http or rpc
                x.ConfigUseRpc = true;
            });

            services.AddNacosV2Naming(x =>
            {
                x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs-test";

                /*x.UserName = "nacos";
               x.Password = "nacos";*/

                // swich to use http or rpc
                x.NamingUseRpc = true;
            });

            services.AddLogging(builder => { builder.AddConsole(); });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        } 
        #endregion
    }
}
