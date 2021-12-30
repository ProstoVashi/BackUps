using System.IO;

namespace PvBackUps.FileEventHandles.Interfaces {
    /// <summary>
    /// Interface for file event handler
    /// </summary>
    public interface IFileEventHandler {
        void OnRenamed(object sender, RenamedEventArgs e);
    }
}