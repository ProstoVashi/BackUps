using System.Collections.Generic;
using PvBackUps.FileEventHandles.Interfaces;

namespace PvBackUps.FileEventHandles {
    public delegate IEnumerable<IFileEventHandler> FileEventHandlerResolver();
}