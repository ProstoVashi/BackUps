using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YandexDiskAuth {
    public static class Program {
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) {
            var hostPort = GetConfig().GetHostPort();
            
            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(webBuilder => {
                           webBuilder.UseStartup<Startup>()
                           .ConfigureKestrel((_, options) => {
                               options.Limits.MaxConcurrentConnections = 5;
                               options.Limits.MaxConcurrentUpgradedConnections = 5;
                               //kestrelOptions.Limits.MaxRequestBodySize = 10 * 1024;
                               options.Listen(IPAddress.Any, hostPort);
                           });
                       });
        }
        
        private static IConfigurationRoot GetConfig() {
            const string appSettingsConfigName = "appsettings";
            return new ConfigurationBuilder()
                   .AddJsonFile($"{appSettingsConfigName}.json", false, false)
                   .Build();
        }
        
        private static ushort GetHostPort(this IConfigurationRoot configuration) {
            return configuration.GetValue<ushort>("HostSettings:Port");
        }
    }
}