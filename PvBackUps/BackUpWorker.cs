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
    /// <summary>
    /// Base worker that handle file-rename events and notifies about it
    /// </summary>
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
            
            // Add required extensions to filter
            foreach (var fileExtension in options.Extensions) {
                _watcher.Filters.Add($"*.{fileExtension}");
            }

            // save extensions
            _extensions = options.Extensions;
            // save concatenated extensions for logging
            _concatenatedExtensions = $"[{string.Join(',', _extensions)}]";
            
            // Create all file event handlers
            _fileEventHandlers = fileEventHandlerResolver();
            
            // Add event handler for file-rename event
            _watcher.Renamed += OnRenamed;  
        }
        
        /// <summary>
        /// Start listening file-rename events in specific folder for specific files' extensions 
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _logger.LogInformation("Service started");
            _watcher.EnableRaisingEvents = true;

            // Wait for stop signal
            var tcs = new TaskCompletionSource<bool>();
            stoppingToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            
            // Run our task
            await tcs.Task;
            
            _logger.LogInformation("Service stopped");
        }

        /// <summary>
        /// Handle fire-rename event. Check appropriate extensions and notifies 
        /// </summary>
        private void OnRenamed(object sender, RenamedEventArgs e) {
            // handle extension of changed file
            var extension = e.FullPath.Split('.').Last();
            
#if DEBUG
            if (!_extensions.Contains(extension)) {
                _logger.LogInformation(
                    "Handle rename from '{OldName}' to '{NewName}', but extension is not appropriate {Extensions}",
                    e.OldFullPath, e.FullPath, _concatenatedExtensions
                    );
                return;
            }
#endif
            
            foreach (var eventHandler in _fileEventHandlers) {
                eventHandler.OnRenamed(sender, e);
            }
        }

        public override void Dispose() {
            _watcher.Dispose();
            
            foreach (var eventHandler in _fileEventHandlers) {
                if (eventHandler is IDisposable disposableEventHandler) {
                    disposableEventHandler.Dispose();
                }
            }
            base.Dispose();
        }
    }
}