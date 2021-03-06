using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using PvBackUps.Configs;
using PvBackUps.FileEventHandles;
using PvBackUps.FileEventHandles.Interfaces;

namespace PvBackUps {
    public class Program {
        public static async Task<int> Main(string[] args) {
            return await Parser.Default.ParseArguments<CommandLineOptions>(args) //ToDo: current directory from args
                               .MapResult(async (opts) => {
                                              await CreateHostBuilder(args, opts).Build().RunAsync();
                                              return 0;
                                          },
                                          errs => Task.FromResult(-1)); // Invalid arguments
        }

        private static IHostBuilder CreateHostBuilder(string[] args, CommandLineOptions opts) {
            Directory.SetCurrentDirectory(opts.ExecutivePath);
            return Host.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(configureLogging =>
                                      configureLogging.AddFilter<EventLogLoggerProvider>(
                                          level => level >= LogLevel.Information))
                .ConfigureServices((hostContext, services) => {
                    services.Configure<RemoteStorageSettings>(
                        hostContext.Configuration.GetSection(nameof(RemoteStorageSettings)));
                    services.AddSingleton(opts);
                    services.AddHostedService<BackUpWorker>()
                            .Configure<EventLogSettings>(config => {
                                config.LogName = "1c BackUp Service";
                                config.SourceName = "1c BackUp Service Source";
                            });
                    services.AddTransient<IFileEventHandler, LogFileHandler>()
                            .Configure<EventLogSettings>(config => {
                                config.LogName = "1c BackUp Service";
                                config.SourceName = "Test (LOG) handler";
                            });
                    services.AddTransient<IFileEventHandler, LocalBackUpFileHandler>()
                            .Configure<EventLogSettings>(config => {
                                config.LogName = "1c BackUp Service";
                                config.SourceName = "Local BackUp handler";
                            });
                    if (TryRunYandexAuthClient()) {
                        services.AddTransient<IFileEventHandler, RemoteBackUpFileHandler>()
                                .Configure<EventLogSettings>(config => {
                                    config.LogName = "1c BackUp Service";
                                    config.SourceName = "Remote BackUp handler";
                                });
                    }

                    services.AddTransient<FileEventHandlerResolver>(
                        provider => provider.GetServices<IFileEventHandler>);
                })
                .UseWindowsService();
        }

        private static bool TryRunYandexAuthClient() {
            try {
                const string yandexDiskAuthRelativePath = "..\\YandexDiskAuth";
                var path = Path.GetFullPath(yandexDiskAuthRelativePath);
                return Process.Start(  $"{path}\\YandexDiskAuth.exe", path) != null;
            } catch (Exception ex){
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}