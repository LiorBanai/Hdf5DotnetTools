using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace HDF5CSharp.Example
{
    public class DateTimeComparerUpToMilliseconds : IEqualityComparer<DateTime>
    {
        public bool Equals(DateTime x, DateTime y) => x.EqualsUpToMilliseconds(y);

        public int GetHashCode(DateTime obj) => (obj.Year * 397) ^ (obj.Month * 397) ^ (obj.Day * 397) ^ (obj.Hour * 397) ^ (obj.Minute * 397) ^ (obj.Second * 397) ^ (obj.Second * 397);
    }
    public static class GeneralUtils
    {
        public static DateTimeComparerUpToMilliseconds DateTimeComparerUpToMilliseconds { get; } = new DateTimeComparerUpToMilliseconds();
        public static bool EqualsUpToMilliseconds(this DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day &&
                   dt1.Hour == dt2.Hour && dt1.Minute == dt2.Minute && dt1.Second == dt2.Second && dt1.Millisecond == dt2.Millisecond;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (bool NonZeroFile, string Message) CheckFileSize(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            if (fileInfo.Length > 0)
            {
                string msg = $"File {filename} has size: {fileInfo.Length}";
                return (true, msg);
            }
            else
            {
                string msg = $"File {filename} has zero size";
                return (false, msg);
            }
        }
    }
}