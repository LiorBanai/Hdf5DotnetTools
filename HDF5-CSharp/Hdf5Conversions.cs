using System;
using System.Runtime.CompilerServices;

namespace HDF5CSharp
{
    public static class Hdf5Conversions
    {
        private static DateTime unix = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FromDatetime(DateTime time, DateTimeType type)
        {
            switch (type)
            {
                case DateTimeType.Ticks:
                    return time.Ticks;
                case DateTimeType.UnixTimeSeconds:
                    return (long)(time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, time.Kind))).TotalSeconds;
                case DateTimeType.UnixTimeMilliseconds:
                    return (long)(time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, time.Kind))).TotalMilliseconds;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToDateTime(long value, DateTimeType type)
        {
            switch (type)
            {
                case DateTimeType.Ticks:
                    return new DateTime(value);
                    break;
                case DateTimeType.UnixTimeSeconds:
                    return unix.AddSeconds(value);
                case DateTimeType.UnixTimeMilliseconds:
                    return unix.AddMilliseconds(value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}