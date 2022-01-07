using System.IO;
using Microsoft.Extensions.Configuration;

namespace Tools.Extensions {
    public static class ConfigurationExtensions {
        public static IConfigurationRoot GetConfigurationRoot(string path, string filename) {
            return new ConfigurationBuilder()
                   .AddJsonFile(Path.Combine(path, filename), false, false)
                   .Build();
        }

        public static T Value<T>(this IConfigurationRoot root, string key = "") {
            if (string.IsNullOrEmpty(key)) {
                key = typeof(T).Name;
            }
            return root.GetValue<T>(key);
        }
    }
}