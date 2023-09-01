using HDF.PInvoke;
using System;
using HDF5CSharp.DataTypes;

namespace HDF5CSharp;

public static class Hdf5Errors
{
    internal static int ErrorDelegateMethod(long estack, IntPtr client_data)
    {
        if (!Hdf5.Settings.H5InternalErrorLoggingEnabled)
        {
            return 0;
        }
        H5E.walk(estack, H5E.direction_t.H5E_WALK_DOWNWARD, WalkDelegateMethod, IntPtr.Zero);
        return 0;
    }

    internal static int WalkDelegateMethod(uint n, ref H5E.error_t err_desc, IntPtr client_data)
    {
        string msg = $"{err_desc.desc}. (function: {err_desc.func_name}. Line:{err_desc.line}. File: {err_desc.file_name})";
        Hdf5Utils.LogMessage(msg,Hdf5LogLevel.Error);
        return 0;
    }
}