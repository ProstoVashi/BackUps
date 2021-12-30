using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tools.Utils {
    public static class ProcessHelper {
        public static Process OpenUrl(string url) {
            try {
                return Process.Start(url);
            } catch {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    url = url.Replace("&", "^&");
                    return Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    return Process.Start("xdg-open", url);
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    return Process.Start("open", url);
                } else {
                    throw;
                }
            }
        }
    }
}