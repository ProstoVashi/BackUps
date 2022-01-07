using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Tools.Extensions;

namespace YandexDiskAuth {
    public static class Program {
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) {
            string executionPath = args.Length == 0 
                ? Directory.GetCurrentDirectory() 
                : args[0];
            var configurationRoot = ConfigurationExtensions.GetConfigurationRoot(executionPath, "appsettings.json");
            var hostPort = configurationRoot.Value<ushort>("HostSettings:Port"); 
            
            Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}. Execution path: {executionPath}");
            
            return Host.CreateDefaultBuilder(args)
                       .UseContentRoot(executionPath)
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
    }
}