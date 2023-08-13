using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PvBackUps.Configs;
using PvBackUps.FileEventHandles.Interfaces;
using PvBackUps.Utils.Extensions;

namespace PvBackUps.FileEventHandles {
    /// <summary>
    /// File event handler to write it on local storage
    /// </summary>
    public class LocalBackUpFileHandler : IFileEventHandler {
        private const string GET_USB_DEVICES = @"SELECT * FROM Win32_DiskDrive";
        private const string GET_PARTITIONS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='{0}'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
        private const string GET_DISKS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='{0}'} WHERE AssocClass = Win32_LogicalDiskToPartition";
        
        private readonly ILogger<LocalBackUpFileHandler> _logger;
        private readonly string _diskSerialNumber;
        private string _destinationPath;
        
        private DirectoryInfo? _directoryInfo;

        public LocalBackUpFileHandler(ILogger<LocalBackUpFileHandler> logger, CommandLineOptions options) {
            _logger = logger;

            _diskSerialNumber = options.DiskSerialNumber;
            _destinationPath = options.DestinationPath;
        }

        /// <summary>
        /// Handle rename event for file in specified folder 
        /// </summary>
        public void OnRenamed(object sender, RenamedEventArgs e) {
            if (!TryGetDestinationDirectory(out _directoryInfo)) {
                _logger.LogWarning("Can't create save folder: '{Path}'. So file '{File}' won't be saved to local storage!", _destinationPath, e.FullPath);
                return;
            }
            
            _logger.LogInformation("Find directory '{Directory}' to save file '{File}'", _directoryInfo.FullName, e.Name);
            
            // Run in new thread to not block FileSystemWatcher
            Task.Run(() => {
                var destFileName = Path.Combine(_directoryInfo.FullName, e.Name!);
                try {
                    if(File.Exists(destFileName)) {
                        _logger.LogWarning("File '{DestinationFile}' will be overwritten", destFileName);
                    }
                    
                    File.Copy(e.FullPath, destFileName, true);
                } catch (Exception ex){
                    _logger.LogError("Local copy '{File}' failed with exception {Exception}", e.Name, ex.Message);
                    throw;
                }
                return destFileName;
            }).ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    string msg = task.Exception?.Message ?? "No exception";
                    _logger.LogError("Failed save file '{File}' with Exception: {Ex}", e.FullPath, msg);
                    return;
                }
                _logger.LogInformation("Save file '{File}' was successful!", task.Result);
            });
        }
        
        /// <summary>
        /// Try get Directory from directory path or/and storage serial number 
        /// </summary>
        private bool TryGetDestinationDirectory([MaybeNullWhen((false))] out DirectoryInfo directoryInfo) {
            // If we previously found directory, then return it
            if (_directoryInfo?.Exists ?? false) {
                directoryInfo = _directoryInfo;
                return true;
            }
            
            // If we DON'T have destination path, then return false
            directoryInfo = null;
            if (string.IsNullOrEmpty(_destinationPath)) {
                _logger.LogWarning("Local save folder is empty!");
                return false;
            }

            // Is destination path is valid, then return it
            if(AbsolutePathIsValid(in _destinationPath)) {
                directoryInfo = new DirectoryInfo(_destinationPath);
                if (!Directory.Exists(_destinationPath)) {
                    directoryInfo.Create();
                }
                return true;
            }
            
            // In this way we try combine path with disk letter. Disk-letter will be found by disk serial number

            // If we DON'T have disk serial number, then return false
            if (string.IsNullOrEmpty(_diskSerialNumber)) {
                _logger.LogWarning("Disk serial number is empty. Can't create save folder!");
                return false;
            }

            // Try find disk letter by disk serial number
            var letter = GetDriveLetter(_diskSerialNumber)?.Trim();
            
            // If we DON'T have disk letter, then return false
            if (string.IsNullOrEmpty(letter)) {
                _logger.LogWarning("Can't find USB drive with specified SerialNumber");
                return false;
            }

            // Combine path with disk letter
            var newDestinationPath = Path.Combine(letter, _destinationPath);
            
            // If new destination path is valid, then return it
            if(AbsolutePathIsValid(in newDestinationPath)) {
                _destinationPath = newDestinationPath;
                directoryInfo = new DirectoryInfo(_destinationPath);
                if (!Directory.Exists(_destinationPath)) {
                    directoryInfo.Create();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns Drive letter for storageDevice, specified by SerialNumber (or it's part)  
        /// </summary>
        private static string? GetDriveLetter(string deviceSerialNumber) {
            // Try find device by serial number
            using var deviceObject = GetObject(GET_USB_DEVICES, obj => ((string)obj["SerialNumber"]).Contains(deviceSerialNumber));
            if (deviceObject == default) {
                return null;
            }
            
            // Try find partition by device id
            var deviceId =  (string)deviceObject["DeviceID"];
            string partitionsRequest = GET_PARTITIONS_TEMPLATE.Replace("{0}", deviceId);
            using var partitionObject = GetObject(partitionsRequest);
            if (partitionObject == default) {
                return null;
            }
            
            // Try find disk by partition id
            string disksRequest = GET_DISKS_TEMPLATE.Replace("{0}", (string)partitionObject["DeviceID"]);
            using var diskObject = GetObject(disksRequest);
            if (diskObject == default) {
                return null;
            }
            return (string)diskObject["Caption"];
        }

        /// <summary>
        /// Return first ManagementObject from ManagementCollection (or null)
        /// </summary>
        private static ManagementBaseObject? GetObject(string query) {
            using var searcher = new ManagementObjectSearcher(query);
            using var collection = searcher.Get();
            return collection.FirstOrDefault();
        }

        /// <summary>
        /// Retunrs specific ManagementObject from ManagementCollection (or null)
        /// </summary>
        private static ManagementBaseObject? GetObject(string query, Func<ManagementBaseObject, bool> predicate) {
            using var searcher = new ManagementObjectSearcher(query);
            using var collection = searcher.Get();
            return collection.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Check if absolute path is valid
        /// </summary>
        private bool AbsolutePathIsValid(in string path) {
            if (!path.Contains(':')) {
                return false;
            }
            try {
                return !string.IsNullOrEmpty(Path.GetFullPath(path));
            } catch {
                _logger.LogWarning("Path is invalid: '{Path}'", path);
                return false;
            }
        }
    }
}