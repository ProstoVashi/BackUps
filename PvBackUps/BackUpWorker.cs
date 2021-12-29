using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PvBackUps.Configs;
using PvBackUps.FileEventHandles;
using PvBackUps.FileEventHandles.Interfaces;

namespace PvBackUps {
    public class BackUpWorker : BackgroundService {
        private readonly ILogger<BackUpWorker> _logger;
        private readonly FileSystemWatcher _watcher;

        private readonly string _concatenatedExtensions;
        private readonly IEnumerable<string> _extensions;
        private readonly IEnumerable<IFileEventHandler> _fileEventHandlers;

        public BackUpWorker(ILogger<BackUpWorker> logger, FileEventHandlerResolver fileEventHandlerResolver, CommandLineOptions options) {
            _logger = logger;
            _watcher = new FileSystemWatcher {
                Path = options.SourcePath
            };
            foreach (var fileExtension in options.Extensions) {
                _watcher.Filters.Add($"*.{fileExtension}");
            }

            _extensions = options.Extensions;
            _concatenatedExtensions = $"[{string.Join(',', _extensions)}]";
            
            _fileEventHandlers = fileEventHandlerResolver();
            
            _watcher.Renamed += OnRenamed;  
        }

        private void OnRenamed(object sender, RenamedEventArgs e) {
            var extension = e.FullPath.Split('.').Last();
            if (!_extensions.Contains(extension)) {
                _logger.LogInformation("Handle rename from '{OldName}' to '{NewName}', but extension is not appropriate {Extensions}",
                    e.OldFullPath, e.FullPath, _concatenatedExtensions);
            }
            
            foreach (var eventHandler in _fileEventHandlers) {
                eventHandler.OnRenamed(sender, e);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _logger.LogInformation("Service started");
            _watcher.EnableRaisingEvents = true;

            var tcs = new TaskCompletionSource<bool>();
            stoppingToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            await tcs.Task;

            
            _logger.LogInformation("Service stopped");
        }

        public override void Dispose() {
            _watcher.Dispose();
            foreach (var eventHandler in _fileEventHandlers) {
                if (eventHandler is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
            base.Dispose();
        }
    }
}