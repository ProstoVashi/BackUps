using System;
using Microsoft.Win32;

namespace Tools.Utils {
    public static class RegisterHelper {
        public static RegistryKey Root => Registry.LocalMachine;

        private const StringSplitOptions SPLIT_OPTIONS =
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        /// <summary>
        /// Get value, started from <paramref name="root"/> through path <paramref name="path"/> 
        /// </summary>
        public static string? GetValue(this RegistryKey root, in string path) {
            ProcessRegistryPath(in path, out var subKeys, out var valueName);
            return root.DeepWork(in subKeys, 0, false, reg => reg.GetValue(valueName)?.ToString());
        }

        /// <summary>
        /// Set specific value <paramref name="value"/> to RegistryKey, started from <paramref name="root"/> through path <paramref name="path"/> 
        /// </summary>
        public static bool SetValue(this RegistryKey root, in string path, string value) {
            ProcessRegistryPath(in path, out var subKeys, out var valueName);
            return root.DeepWork(in subKeys, 0, true, reg => {
                                     try {
                                         reg.SetValue(valueName, value);
                                         return true;
                                     } catch {
                                         return false;
                                     }
            });
        }

        /// <summary>
        /// Form from path registry keys and value name.
        /// </summary>
        /// <param name="path">Keys, splitted by '/' and value, as as second part after '.'. Example: 'root_key/sub_key.value_key'</param>
        /// <param name="subKeys">Path registry keys</param>
        /// <param name="valueName">value key name</param>
        private static void ProcessRegistryPath(in string path, out string[] subKeys, out string valueName) {
            string[] subValues = path.Split('.', SPLIT_OPTIONS);
            valueName = subValues[1];
            subKeys = subValues[0].Split('/', SPLIT_OPTIONS);
        }

        /// <summary>
        /// Goes deeper through registry-key names array. Last registry key passes to <paramref name="work"/>-function and returns value from it
        /// </summary>
        /// <param name="root">Income registry key</param>
        /// <param name="subKeys">Registry-key names array</param>
        /// <param name="index">Current index of array. New registry-key's name is "subKeys[index]"</param>
        /// <param name="writable">Open new RegisterKey with writable access or not</param>
        /// <param name="work">Some foo, which result returns</param>
        private static T DeepWork<T>(this RegistryKey root, in string[] subKeys, int index, bool writable, Func<RegistryKey, T> work) {
            T result;
            if (index == subKeys.Length) {
                result = work(root);
                return result;
            }

            using var node = writable
                ? root.OpenOrCreateSubKey(in subKeys[index])
                : root.OpenSubKey(subKeys[index], false);
            if (node == null) {
                return default;
            }

            result = node.DeepWork(in subKeys, index + 1, writable, work);
            node.Close();

            return result;
        }

        private static RegistryKey OpenOrCreateSubKey(this RegistryKey root, in string subKey, bool writable = true) {
            return root.OpenSubKey(subKey, writable) ?? root.CreateSubKey(subKey, writable);
        }
    }
}