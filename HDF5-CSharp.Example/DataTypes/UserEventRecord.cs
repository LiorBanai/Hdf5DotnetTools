using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HDF5CSharp.Example.DataTypes
{
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct UserEventRecord
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        [Hdf5EntryName("routes")] public string Route;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        [Hdf5EntryName("events")] public string Event;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 400)]
        [Hdf5EntryName("description")] public string Description;
        [Hdf5EntryName("timestamps")] public long Timestamp;
        public UserEventRecord(string route, string eventData, string description, long timestamp)
        {
            Route = route;
            Description = description;
            Event = eventData;
            Timestamp = timestamp;
        }
        public override string ToString() => $"time: {Timestamp}: {nameof(Route)}: {Route}, {nameof(Event)}: {Event}.";
    }
}