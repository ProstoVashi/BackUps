using System.IO;

namespace PvBackUps.FileEventHandles.Interfaces {
    public interface IFileEventHandler {
        void OnRenamed(object sender, RenamedEventArgs e);
    }
}