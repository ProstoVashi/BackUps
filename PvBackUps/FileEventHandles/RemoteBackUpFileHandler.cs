using System.IO;
using Microsoft.Extensions.Logging;
using PvBackUps.FileEventHandles.Interfaces;

namespace PvBackUps.FileEventHandles {
    public class RemoteBackUpFileHandler : IFileEventHandler {
        private readonly ILogger<RemoteBackUpFileHandler> _logger;

        public RemoteBackUpFileHandler(ILogger<RemoteBackUpFileHandler> logger) {
            _logger = logger;
        }

        public void OnRenamed(object sender, RenamedEventArgs e) {
            _logger.LogInformation("Handle event for remote HANDLER");
        }
    }
}