using System;
using System.Collections.Generic;
using System.Text;

namespace HDF5CSharp.Example.DataTypes
{
    using HDF5CSharp.DataTypes;
    using System.Runtime.InteropServices;

    namespace HDF5Store.DataTypes
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MeansFullECGEvent
        {
            [Hdf5EntryName("index")] public long index;
            [Hdf5EntryName("timestamp")] public long timestamp;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100000)][Hdf5EntryName("data")] public string data;

            public MeansFullECGEvent(long index, long timestamp, string data)
            {
                this.timestamp = timestamp;
                this.data = data;
                this.index = index;
            }
        }
    }
}