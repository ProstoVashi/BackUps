using System.IO;
using Microsoft.Extensions.Logging;
using PvBackUps.FileEventHandles.Interfaces;

namespace PvBackUps.FileEventHandles {
    /// <summary>
    /// File event handler to log out info about it
    /// </summary>
    public class LogFileHandler : IFileEventHandler {
        private readonly ILogger<LogFileHandler> _logger;

        public LogFileHandler(ILogger<LogFileHandler> logger) {
            _logger = logger;
        }

        public void OnRenamed(object sender, RenamedEventArgs e) {
            _logger.LogInformation("File was renamed from '{OldName}' to '{NewName}'. It's event : {ChangeType}",
                                   e.OldFullPath,
                                   e.OldFullPath,
                                   e.ChangeType.ToString());
        }
    }
}