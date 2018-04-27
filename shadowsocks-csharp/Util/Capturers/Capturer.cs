using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shadowsocks.Controller.Strategy;
using Shadowsocks.Model;

namespace Shadowsocks.Util.Capturers
{
    public class Capturer
    {
        static Capturer()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), true, true);
            var configuration = configurationBuilder.Build();
            ConfigurationManager.UseConfiguration(configuration);
        }
        private static readonly HttpClient HttpClient = new HttpClient();

        public Server[] ParseFromSource(params Source[] sources)
        {
            var configs = new List<Server>();
            foreach (var source in sources)
            {
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                var html = HttpClient.GetAsync(source.Url).Result.Content.ReadAsStringAsync().Result;
                htmlDocument.LoadHtml(html);
                var nodes = htmlDocument.DocumentNode.SelectNodes(source.Selector);
                foreach (var node in nodes)
                {
                    var ip = node.SelectSingleNode(source.IpSelector)?.InnerText.Trim();
                    var port = node.SelectSingleNode(source.PortSelector)?.InnerText.Trim();
                    var password = node.SelectSingleNode(source.PwdSelector)?.InnerText.Trim();
                    var method = node.SelectSingleNode(source.MethodSelector)?.InnerText.Split(':')?.LastOrDefault()?.Trim();
                    var server = new Server
                    {
                        server = ip,
                        server_port = int.Parse(port ?? "0"),
                        password = password,
                        method = method,
                        plugin = "",
                        plugin_opts = "",
                        remarks = "",
                        timeout = 5
                    };
                    configs.Add(server);
                }
            }
            return configs.ToArray();
        }

        public Configuration RefreshConfiguration(Configuration configuration)
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{DateTime.Now:yyyy-MM}.log");
            try
            {
                var sources = ConfigurationManager.Get<List<Source>>("sources")?
                                                  .ToArray();
                var servers = ParseFromSource(sources);
                configuration.configs.Clear();
                configuration.configs.AddRange(servers);
                configuration.strategy = HighAvailabilityStrategy.HighAvailabilityStrategyId;
                File.AppendAllText(logPath, $"{DateTime.Now:yyyy MMMM dd HH:mm:ss} Update Successful {Environment.NewLine}");
            }
            catch (Exception e)
            {
                File.AppendAllText(logPath, $"{DateTime.Now:yyyy MMMM dd HH:mm:ss} Update failed {e.Message} {Environment.NewLine} {e.StackTrace} {Environment.NewLine}");
            }
            return configuration;
        }

    }
}
