using MessagePack;
using System;
using System.Runtime.CompilerServices;

namespace HDF5CSharp.Example.DataTypes
{
    [Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class ElectrodeFrame
    {
        public (float Re, float Im)[] ComplexVoltageMatrix { get; set; }    // CHANNELS * CHANNELS entries
        public (float Re, float Im)[] ComplexCurrentMatrix { get; set; }    // CHANNELS * CHANNELS entries

        public long timestamp;  // timestamp in unix time (milliseconds since 1.1.1970 00:00:00 -  less accurate)

        public ulong PacketId = ulong.MaxValue; // serial number of packet

        public ulong SaturationMask;

        public void GenerateDummyData(int electrodeNum)
        {
            ComplexVoltageMatrix = new ValueTuple<float, float>[electrodeNum * electrodeNum];
            ComplexCurrentMatrix = new ValueTuple<float, float>[electrodeNum * electrodeNum];

            Random r = new Random();

            for (int i = 0; i < electrodeNum * electrodeNum; i++)
            {
                ComplexVoltageMatrix[i].Re = r.Next(0, 1000) / 1000.0f;
                ComplexVoltageMatrix[i].Im = r.Next(0, 1000) / 1000.0f;
                ComplexCurrentMatrix[i].Im = r.Next(0, 1000) / 1000.0f;
                ComplexCurrentMatrix[i].Re = r.Next(0, 1000) / 1000.0f;
            }

            PacketId = 5;
            SaturationMask = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Serialize(ElectrodeFrame fr) => MessagePackSerializer.Serialize(fr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ElectrodeFrame Deserialize(byte[] array) => MessagePackSerializer.Deserialize<ElectrodeFrame>(array);
    }
}