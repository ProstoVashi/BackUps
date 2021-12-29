#if !(WIN64 || WIN86)
#define UNLOCKED
#endif
using System;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PvBackUps.Configs;
using PvBackUps.FileEventHandles.Interfaces;
using PvBackUps.Utils.Extensions;

namespace PvBackUps.FileEventHandles {
    public class LocalBackUpFileHandler : IFileEventHandler {
        private const string GET_USB_DEVICES = @"SELECT * FROM Win32_DiskDrive WHERE InterfaceType LIKE 'USB%'";
        private const string GET_PARTITIONS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='{0}'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
        private const string GET_DISKS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='{0}'} WHERE AssocClass = Win32_LogicalDiskToPartition";
        
        private readonly ILogger<LocalBackUpFileHandler> _logger;
        private readonly string _diskSerialNumber;
        private string _destinationPath;
        
        private DirectoryInfo _directoryInfo;

        public LocalBackUpFileHandler(ILogger<LocalBackUpFileHandler> logger, CommandLineOptions options) {
            _logger = logger;

            _diskSerialNumber = options.DiskSerialNumber;
            _destinationPath = options.DestinationPath;
        }

        public void OnRenamed(object sender, RenamedEventArgs e) {
            if (!TryGetDirectory(out _directoryInfo)) {
                _logger.LogWarning("Can't create save folder: '{Path}'. So file '{File}' won't be saved to local storage!", _destinationPath, e.FullPath);
                return;
            }
            
        }


        private bool TryGetDirectory(out DirectoryInfo directoryInfo) {
            if (_directoryInfo?.Exists ?? false) {
                directoryInfo = _directoryInfo;
                return true;
            }
            
            directoryInfo = null;
            if (string.IsNullOrEmpty(_destinationPath)) {
                _logger.LogWarning("Local save folder is empty!");
                return false;
            }

            if(PathIsValid(in _destinationPath)) {
                directoryInfo = new DirectoryInfo(_destinationPath);
                if (!Directory.Exists(_destinationPath)) {
                    directoryInfo.Create();
                }
                return true;
            }

            if (string.IsNullOrEmpty(_diskSerialNumber)) {
                _logger.LogWarning("Disk serial number uis empty. Can't create save folder!");
                return false;
            }

            var letter = GetDriveLetter(_diskSerialNumber).Trim();
            if (string.IsNullOrEmpty(letter)) {
                _logger.LogWarning("Can't find USB drive with specified SerialNumber");
                return false;
            }

            var newDestinationPath = Path.Combine(letter, _destinationPath);
            if(PathIsValid(in newDestinationPath)) {
                _destinationPath = newDestinationPath;
                directoryInfo = new DirectoryInfo(_destinationPath);
                if (!Directory.Exists(_destinationPath)) {
                    directoryInfo.Create();
                }
                return true;
            }
            return false;
        }

        private string GetDriveLetter(string deviceSerialNumber) {
            using var deviceObject = GetObject(GET_USB_DEVICES, obj => ((string)obj["SerialNumber"]).StartsWith(deviceSerialNumber));
            if (deviceObject == default) {
                return null;
            }
            
            var deviceId =  (string)deviceObject["DeviceID"];
            string partitionsRequest = GET_PARTITIONS_TEMPLATE.Replace("{0}", deviceId);
            using var partitionObject = GetObject(partitionsRequest);
            if (partitionObject == default) {
                return null;
            }
            
            string disksRequest = GET_DISKS_TEMPLATE.Replace("{0}", (string)partitionObject["DeviceID"]);
            using var diskObject = GetObject(disksRequest);
            if (diskObject == default) {
                return null;
            }
            return (string)diskObject["Caption"];
        }

        private static ManagementBaseObject GetObject(string query) {
            using var searcher = new ManagementObjectSearcher(query);
            using var collection = searcher.Get();
            return collection.FirstOrDefault();
        }

        private static ManagementBaseObject GetObject(string query, Func<ManagementBaseObject, bool> predicate) {
            using var searcher = new ManagementObjectSearcher(query);
            using var collection = searcher.Get();
            return collection.FirstOrDefault(predicate);
        }

        private bool PathIsValid(in string path) {
            try {
                var validPath = Path.GetFullPath(path);

                return !string.IsNullOrEmpty(validPath);
            } catch {
                _logger.LogWarning("Path is invalid: '{Path}'", path);
                return false;
            }
        }
    }
}