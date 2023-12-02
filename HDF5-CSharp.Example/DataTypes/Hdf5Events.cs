using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5GroupName("system_events")]
    public class Hdf5Events
    {
        [Hdf5EntryName("timestamps")] public long[] timestamps;
        [Hdf5EntryName("type")] public string[] type;
        [Hdf5EntryName("description")] public string[] description;
        [Hdf5EntryName("data")] public string[] data;

        public Hdf5Events()
        {
            timestamps = Array.Empty<long>();
            type = Array.Empty<string>();
            description= Array.Empty<string>();
            data= Array.Empty<string>();
        }
        public Hdf5Events(List<SystemEvent> eventsData)
        {
            timestamps = new long[eventsData.Count];
            type = new string[eventsData.Count];
            description = new string[eventsData.Count];
            data = new string[eventsData.Count];
            for (int i = 0; i < eventsData.Count; i++)
            {
                timestamps[i] = eventsData[i].timestamp;
                type[i] = eventsData[i].type;
                description[i] = eventsData[i].description;
                data[i] = eventsData[i].data;
            }
        }
    }
}