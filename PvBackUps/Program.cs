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

        private static CommandLineOptions _options;
        public static async Task<int> Main(string[] args) {
            return await Parser.Default.ParseArguments<CommandLineOptions>(args) //ToDo: current directory from args
                               .MapResult(async (opts) => {
                                              await CreateHostBuilder(args, opts).Build().RunAsync();
                                              return 0;
                                          },
                                          errs => Task.FromResult(-1)); // Invalid arguments
        }

        private static IHostBuilder CreateHostBuilder(string[] args, CommandLineOptions opts) {
            
            _options = opts;
            
            // Set current directory with executive path from options
            Directory.SetCurrentDirectory(opts.ExecutivePath);
            
            
            return Host.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices((hostContext, services) => {
                    services.Configure<RemoteStorageSettings>(hostContext.Configuration.GetSection(nameof(RemoteStorageSettings)));
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

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services) {
            
            // Configure remote storage settings
            services.Configure<RemoteStorageSettings>(hostContext.Configuration.GetSection(nameof(RemoteStorageSettings)));
            
            // Register backup worker and Configure logging 
            services.AddHostedService<BackUpWorker>()
                    .Configure<EventLogSettings>(config => {
                        config.LogName = "1c BackUp Service";
                        config.SourceName = "1c BackUp Service Source";
                    });
            
            // Register file event handlers, that log information about directory and Configure logging
            services.AddTransient<IFileEventHandler, LogFileHandler>()
                    .Configure<EventLogSettings>(config => {
                        config.LogName = "1c BackUp Service";
                        config.SourceName = "Test (LOG) handler";
                    });
            
            // Register file event handlers, that save files to local storage and Configure logging
            services.AddTransient<IFileEventHandler, LocalBackUpFileHandler>()
                    .Configure<EventLogSettings>(config => {
                        config.LogName = "1c BackUp Service";
                        config.SourceName = "Local BackUp handler";
                    });
            
            if (TryRunYandexAuthClient()) {
                
                // Register file event handlers, that save files to remote YD-storage and Configure logging
                services.AddTransient<IFileEventHandler, RemoteBackUpFileHandler>()
                        .Configure<EventLogSettings>(config => {
                            config.LogName = "1c BackUp Service";
                            config.SourceName = "Remote BackUp handler";
                        });
            }

            services.AddTransient<FileEventHandlerResolver>(
                provider => provider.GetServices<IFileEventHandler>);
        }
        
        /// <summary>
        /// Try run YandexDiskAuth.exe
        /// </summary>
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
        
        /// <summary>
        /// Configure logging
        /// </summary>
        /// <remarks>
        /// Set log level to Information
        /// </remarks>
        private static void ConfigureLogging(ILoggingBuilder loggingBuilder) {
            loggingBuilder.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information);
        }
        
    }
}