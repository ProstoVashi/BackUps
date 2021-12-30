using System;
using System.IO;
using System.Management;

namespace Sandbox {
    public static class LocalCopySandbox {
        private const string GET_USB_DEVICES = @"SELECT * FROM Win32_DiskDrive";

        private const string GET_PARTITIONS_TEMPLATE =
            "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='{0}'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";

        private const string GET_DISKS_TEMPLATE =
            "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='{0}'} WHERE AssocClass = Win32_LogicalDiskToPartition";

        private static string _diskSerialNumber;
        private static string _destinationPath;
        
        private static DirectoryInfo _directoryInfo;

        public static void Invoke() {
            IterateUsbDevices();
            // _diskSerialNumber = "";
            // _destinationPath = "Temp\\BackUps";
            //
            // if (!TryGetDirectory(out _directoryInfo)) {
            //     Console.WriteLine($"Can't create save folder: '{_destinationPath}'. So file won't be saved to local storage!");
            //     return;
            // }
            //
            // Console.WriteLine($"Find directory '{_directoryInfo.FullName}' to save file");
            
            Console.ReadKey();
        }
        
        private static bool TryGetDirectory(out DirectoryInfo directoryInfo) {
            if (_directoryInfo?.Exists ?? false) {
                directoryInfo = _directoryInfo;
                return true;
            }
            
            directoryInfo = null;
            if (string.IsNullOrEmpty(_destinationPath)) {
                Console.WriteLine("Local save folder is empty!");
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
                Console.WriteLine("Disk serial number uis empty. Can't create save folder!");
                return false;
            }

            var letter = GetDriveLetter(_diskSerialNumber).Trim();
            if (string.IsNullOrEmpty(letter)) {
                Console.WriteLine("Can't find USB drive with specified SerialNumber");
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

        private static string GetDriveLetter(string deviceSerialNumber) {
            using var deviceObject = GetObject(GET_USB_DEVICES,
                                               obj => ((string)obj["SerialNumber"]).Contains(deviceSerialNumber));
            if (deviceObject == default) {
                return null;
            }

            var deviceId = (string)deviceObject["DeviceID"];
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

        private static ManagementBaseObject FirstOrDefault(this ManagementObjectCollection collection) {
            return collection.FirstOrDefault(_ => true);
        }

        private static ManagementBaseObject FirstOrDefault(this ManagementObjectCollection collection,
            Func<ManagementBaseObject, bool> predicate) {
            if (collection.Count == 0) {
                return default;
            }

            foreach (var managementObject in collection) {
                if (predicate(managementObject)) {
                    return managementObject;
                }
            }

            return default;
        }
        
        private static bool PathIsValid(in string path) {
            if (!path.Contains(":")) {
                return false;
            }
            try {
                return !string.IsNullOrEmpty(Path.GetFullPath(path));
            } catch {
                Console.WriteLine($"Path is invalid: '{path}'");
                return false;
            }
        }

        private static void IterateUsbDevices() {
            Console.WriteLine("===========");
            using var usbDevicesSearcher = new ManagementObjectSearcher(GET_USB_DEVICES);
            using var usbDeviceCollection = usbDevicesSearcher.Get();

            foreach (var deviceObject in usbDeviceCollection) {
                Console.WriteLine("---DEVICE---");
                foreach (var deviceProperty in deviceObject.Properties) {
                    Console.WriteLine($"{deviceProperty.Name} : {deviceProperty.Value} | {deviceProperty.Type}");
                }

                Console.WriteLine("---DEVICE----");
                Console.WriteLine();

                var deviceId = (string)deviceObject.Properties["DeviceID"].Value;
                string partitionsRequest = GET_PARTITIONS_TEMPLATE.Replace("{0}", deviceId);

                using var partitionsSearcher = new ManagementObjectSearcher(partitionsRequest);
                using var partitionsCollection = partitionsSearcher.Get();
                foreach (var partitionObject in partitionsCollection) {
                    Console.WriteLine("---PARTION---");
                    foreach (var partitionProperty in partitionObject.Properties) {
                        Console.WriteLine(
                            $"{partitionProperty.Name} : {partitionProperty.Value} | {partitionProperty.Type}");
                    }

                    Console.WriteLine("---PARTION----");
                    Console.WriteLine();

                    string disksRequest = GET_DISKS_TEMPLATE.Replace("{0}", (string)partitionObject["DeviceID"]);
                    using var diskSearcher = new ManagementObjectSearcher(disksRequest);
                    using (var disksCollection = diskSearcher.Get()) {
                        foreach (var diskObject in disksCollection) {
                            Console.WriteLine("---LOGIC---");
                            foreach (var property in diskObject.Properties) {
                                Console.WriteLine($"{property.Name} : {property.Value} | {property.Type}");
                            }

                            Console.WriteLine("----LOGIC----");
                            Console.WriteLine();
                        }
                        //ToDO: return disksCollections.Select(disk => disk["Name"]).
                    }
                }
            }

            Console.WriteLine("===========");
            Console.WriteLine();
        }
    }
}