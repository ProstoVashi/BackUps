using System;
using System.Management;

namespace Sandbox {
    class Program {
        private const string GET_USB_DEVICES = @"SELECT * FROM Win32_DiskDrive WHERE InterfaceType LIKE 'USB%'";
        private const string GET_PARTITIONS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='{0}'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
        private const string GET_DISKS_TEMPLATE = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='{0}'} WHERE AssocClass = Win32_LogicalDiskToPartition";
        
        static void Main(string[] args) {
            IterateUsbDevices();
        }

        private static void IterateUsbDevices() {
            Console.WriteLine("USB devices:");
            Console.WriteLine("===========");
            using var usbDevicesSearcher = new ManagementObjectSearcher(GET_USB_DEVICES);
            using var usbDeviceCollection = usbDevicesSearcher.Get();

            foreach (var usbDevice in usbDeviceCollection) {
                Console.WriteLine("New usb-device:");
                foreach (var property in usbDevice.Properties) {
                    Console.WriteLine($"{property.Name} : {property.Value} (${property.Type.ToString()})");
                }
                Console.WriteLine("------------");
            }
            Console.WriteLine("===========");
            Console.WriteLine();
        }
    }
}