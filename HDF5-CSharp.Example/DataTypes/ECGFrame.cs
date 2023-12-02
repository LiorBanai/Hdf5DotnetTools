using MessagePack;
using System;
using System.Collections.Generic;

namespace HDF5CSharp.Example.DataTypes
{
    [Serializable]
    [MessagePackObject(keyAsPropertyName: false)]

    public class ECGFrame
    {
        [Key("packetId")] public UInt64 PacketId { get; set; }
        [Key("timestamp")] public long Timestamp { get; set; }
        [Key("KalpaClock")] public UInt64 KalpaClock { get; set; }
        [Key("Data")] public List<List<float>> FrameData { get; set; }
        [Key("DataFiltered")] public List<List<float>> FilteredFrameData { get; set; }


        [IgnoreMember] public bool IsValid;// Prior the stabilization (normalization need to be done), the frame will be raised as invalid
        [IgnoreMember] public bool IsTriggerToInjection;

        public ECGFrame()
        {
            FrameData = new List<List<float>>();
            FilteredFrameData = new List<List<float>>();
            IsValid = true;
        }
        public ECGFrame(List<List<float>> filteredData, List<List<float>> unFilteredData, long timestamp, UInt64 packetId)
        {
            PacketId = packetId;
            FilteredFrameData = filteredData;
            FrameData = unFilteredData;
            IsTriggerToInjection = false;
            Timestamp = timestamp;
        }
    }
}