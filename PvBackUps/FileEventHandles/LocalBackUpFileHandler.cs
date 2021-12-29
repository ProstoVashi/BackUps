#if !(WIN64 || WIN86)
#define UNLOCKED
#endif
using System;
using System.IO;
using System.Management;
using Microsoft.Extensions.Logging;
using PvBackUps.Configs;
using PvBackUps.FileEventHandles.Interfaces;

namespace PvBackUps.FileEventHandles {
    public class LocalBackUpFileHandler : IFileEventHandler, IDisposable {
        private const string GET_USB_DEVICES = @"SELECT * FROM Win32_DiskDrive WHERE InterfaceType LIKE 'USB%'";
        private const string GET_PARTITIONS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='{0}'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
        private const string GET_DISKS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='{0}'} WHERE AssocClass = Win32_LogicalDiskToPartition";
        
        private readonly ILogger<LocalBackUpFileHandler> _logger;
        private readonly string _destinationPath;

        private readonly ManagementObjectSearcher _usbDevicesSearcher;

        public LocalBackUpFileHandler(ILogger<LocalBackUpFileHandler> logger, CommandLineOptions options) {
            _logger = logger;
            _destinationPath = options.DestinationPath;
#if UNLOCKED
            _usbDevicesSearcher = new ManagementObjectSearcher(GET_USB_DEVICES);
#endif
        }

        public void OnRenamed(object sender, RenamedEventArgs e) {
            #if !WIN
            
            #endif
        }

#if UNLOCKED
        private string GetDriveLetter() {
            using var deviceObjectCollection = _usbDevicesSearcher.Get();
            foreach (var deviceObject in deviceObjectCollection) {
                    
                var deviceId = (string)deviceObject.Properties["DeviceID"].Value;
                string partitionsRequest = GET_PARTITIONS_TEMPLATE.Replace("{0}", deviceId);

                using var partitionsSearcher = new ManagementObjectSearcher(partitionsRequest);
                using var partitionsCollection = partitionsSearcher.Get();
                foreach (var partitionObject in partitionsCollection) {
                                
                    string disksRequest = GET_DISKS_TEMPLATE.Replace("{0}", (string)partitionObject["DeviceID"]);
                    using var diskSearcher = new ManagementObjectSearcher(disksRequest);
                    using (var disksCollections = diskSearcher.Get()) {
                        //ToDO: return disksCollections.Select(disk => disk["Name"]).
                    }
                }
            }
        }
#endif
        
        
#if UNLOCKED
        public void Dispose() {
            _usbDevicesSearcher?.Dispose();
        }
#endif
    }
}