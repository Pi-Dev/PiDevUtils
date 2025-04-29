using System;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Simple time utility extensions for working with Unix time in seconds or milliseconds.
 * Converts standard DateTime values to Unix epoch-based timestamps.
 *
 * ============= Usage =============
 * long seconds = myDateTime.ToUnixTimeSeconds();
 * long millis = myDateTime.ToUnixTimeMS();
 */

namespace PiDev
{
    public static partial class Utils
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimeSeconds(this DateTime value)
        {
            DateTimeOffset dto = new DateTimeOffset(value);
            return dto.ToUnixTimeSeconds();
        }

        public static long ToUnixTimeMS(this DateTime value)
        {
            DateTimeOffset dto = new DateTimeOffset(value);
            return dto.ToUnixTimeMilliseconds();
        }
    }
}