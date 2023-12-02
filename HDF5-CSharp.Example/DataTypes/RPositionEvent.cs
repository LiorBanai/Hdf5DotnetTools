using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HDF5CSharp.Example.DataTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RPositionEvent
    {
        [Hdf5EntryName("timestamp")] public ulong timestamp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)] [Hdf5EntryName("name")] public string name;
        [Hdf5EntryName("x")] public float x;
        [Hdf5EntryName("y")] public float y;
        [Hdf5EntryName("z")] public float z;
        [Hdf5EntryName("direction_x")] public float direction_x;
        [Hdf5EntryName("direction_y")] public float direction_y;
        [Hdf5EntryName("direction_z")] public float direction_z;
        public RPositionEvent(ulong timestamp, string name, float x, float y, float z, float direction_x, float direction_y, float direction_z)
        {
            this.timestamp = timestamp;
            this.name = name;
            this.x = x;
            this.y = y;
            this.z = z;
            this.direction_x = direction_x;
            this.direction_y = direction_y;
            this.direction_z = direction_z;
        }
    }
}