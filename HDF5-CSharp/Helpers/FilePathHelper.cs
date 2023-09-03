using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HDF5CSharp.Helpers;

/// <summary>
/// File path helper method(s)
/// </summary>
internal static class FilePathHelper
{
    /// <summary>
    /// Converts a long path with possible non-ascii characters to an ascii safe path in the 8.3 safe format.
    /// </summary>
    /// <param name="longPath"></param>
    /// <returns>An 8.3 format path</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static string ToShortPath(this string longPath)
    {
        if(Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            throw new InvalidOperationException("The extension method ToShortPath(this string longPath) cannot be called in a non-windows operating system context.");
        }
        StringBuilder sb = new(255);
        _ = GetShortPathName(longPath, sb, sb.Capacity);
        return sb.ToString();
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GetShortPAthNameW", SetLastError = true)]
    internal extern static uint GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string longpath, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int bufferSize);
}
