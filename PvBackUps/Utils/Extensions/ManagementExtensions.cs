using System;
using System.Management;

namespace PvBackUps.Utils.Extensions {
    internal static class ManagementExtensions {
        internal static ManagementBaseObject FirstOrDefault(this ManagementObjectCollection collection) {
            return collection.FirstOrDefault(_ => true);
        }

        internal static ManagementBaseObject FirstOrDefault(this ManagementObjectCollection collection, Func<ManagementBaseObject, bool> predicate) {
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
    }
}