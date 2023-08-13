using System;
using System.Management;

namespace PvBackUps.Utils.Extensions {
    internal static class ManagementExtensions {
        /// <summary>
        /// Returns first element from ManagementCollection (or default)
        /// </summary>
        internal static ManagementBaseObject? FirstOrDefault(this ManagementObjectCollection collection) {
            return collection.FirstOrDefault(null);
        }

        /// <summary>
        /// Returns first element from ManagementCollection, that satisfies predicate (or default)
        /// </summary>
        internal static ManagementBaseObject? FirstOrDefault(this ManagementObjectCollection collection, Func<ManagementBaseObject, bool>? predicate) {
            if (collection.Count == 0) {
                return default;
            }
            
            // If predicate is null, return first element
            if (predicate == null) {
                foreach (var managementObject in collection) {
                    return managementObject;
                }

                // or default
                return default;
            }
            
            // Else return first element, that satisfies predicate
            foreach (var managementObject in collection) {
                if (predicate(managementObject)) {
                    return managementObject;
                }
            }
            
            // or default
            return default;
        }
    }
}