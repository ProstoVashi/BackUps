using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Sandbox {
    static class Program {
        private const string GET_USB_DEVICES = @"SELECT * FROM Win32_DiskDrive WHERE InterfaceType LIKE 'USB%'";
        private const string GET_PARTITIONS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='{0}'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
        private const string GET_DISKS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='{0}'} WHERE AssocClass = Win32_LogicalDiskToPartition";
        
        static void Main(string[] args) {
            IterateUsbDevices();
            //var letter = GetDriveLetter("7R6PP7AA1LXW-DW");
            // if (string.IsNullOrEmpty(letter)) {
            //     Console.WriteLine("DISK NOT FOUND!");
            // } else {
            //     Console.WriteLine($"DISK's letter: {letter}");
            // }
            Console.ReadKey();
        }

        private static string GetDriveLetter(string deviceSerialNumber) {
            using var usbDevicesSearcher = new ManagementObjectSearcher(GET_USB_DEVICES);
            using var usbDeviceCollection = usbDevicesSearcher.Get();
            var deviceObject = usbDeviceCollection.FirstOrDefault(obj => obj["SerialNumber"].ToString().StartsWith(deviceSerialNumber));
            if (deviceObject == default) {
                return null;
            }
            var deviceId =  (string)deviceObject["DeviceID"];
            string partitionsRequest = GET_PARTITIONS_TEMPLATE.Replace("{0}", deviceId);
            
            using var partitionsSearcher = new ManagementObjectSearcher(partitionsRequest);
            using var partitionsCollection = partitionsSearcher.Get();
            var partitionObject = partitionsCollection.FirstOrDefault();
            if (partitionObject == default) {
                return null;
            }
            
            string disksRequest = GET_DISKS_TEMPLATE.Replace("{0}", (string)partitionObject["DeviceID"]);
            using var diskSearcher = new ManagementObjectSearcher(disksRequest);
            using var disksCollection = diskSearcher.Get();
            var diskObject = disksCollection.FirstOrDefault();
            if (diskObject == default) {
                return null;
            }
            return (string)diskObject["Caption"];
        }

        private static ManagementBaseObject FirstOrDefault(this ManagementObjectCollection collection) {
            return collection.FirstOrDefault(_ => true);
        }

        private static ManagementBaseObject FirstOrDefault(this ManagementObjectCollection collection, Func<ManagementBaseObject, bool> predicate) {
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
                        Console.WriteLine($"{partitionProperty.Name} : {partitionProperty.Value} | {partitionProperty.Type}");
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