using System;
using System.Globalization;

namespace Tools.Extensions {
    /// <summary>
    /// Extensions with date
    /// </summary>
    public static class DateTimeExtensions {
        /// <summary>
        /// Returns number of seconds from unix date
        /// </summary>
        public static long DateToSeconds(this DateTime date) {
            return new DateTimeOffset(date).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Returns date time from second passed since unix date
        /// </summary>
        public static DateTime SecondsToDate(this long seconds) {
            return DateTime.UnixEpoch.AddSeconds(seconds);
        }
        
        /// <summary>
        /// Returns date time from milli-second passed since unix date
        /// </summary>
        public static DateTime MilliSecondsToDate(this long ms) {
            return DateTime.UnixEpoch.AddMilliseconds(ms);
        }

        /// <summary>
        /// Convert date to string in specified format
        /// </summary>
        public static string DateToString(this DateTime date, string format) {
            return date.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse date string to date time
        /// </summary>
        public static DateTime StringToDate(this string dateString, string format) {
            return DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
        }
    }
}