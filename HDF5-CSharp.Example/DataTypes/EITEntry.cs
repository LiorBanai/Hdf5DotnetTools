using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HDF5CSharp.Example.DataTypes
{
    public class EITEntry : IEquatable<EITEntry>
    {
        private IReadOnlyList<long> _timestampsRaw;
        [Hdf5EntryName("configuration")] public string Configuration { get; set; }
        [Hdf5EntryName("start_datetime")] public DateTime StartDateTime { get; set; }
        [Hdf5EntryName("end_datetime")] public DateTime EndDateTime { get; set; }
        [Hdf5EntryName("voltages.re")] public float[,] VoltagesReal { get; set; }
        [Hdf5EntryName("voltages.im")] public float[,] VoltagesIm { get; set; }
        [Hdf5EntryName("currents.re")] public float[,] CurrentsReal { get; set; }
        [Hdf5EntryName("currents.im")] public float[,] CurrentsIm { get; set; }
        [Hdf5EntryName("saturations")] public ulong[,] Saturation { get; set; }
        [Hdf5EntryName("timestamps")] public long[,] Timestamps { get; set; }
        [Hdf5EntryName("packetids")] public ulong[,] PacketIds { get; set; }

        public EITEntry()
        {
            Configuration = string.Empty;
            _timestampsRaw = Array.Empty<long>();
        }

        public bool Equals(EITEntry other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var equal = Configuration == other.Configuration &&
                        StartDateTime.EqualsUpToMilliseconds(other.StartDateTime) &&
                        EndDateTime.EqualsUpToMilliseconds(other.EndDateTime) &&

                        VoltagesReal.Rank == other.VoltagesReal.Rank &&
                        Enumerable.Range(0, VoltagesReal.Rank).All(dimension =>
                            VoltagesReal.GetLength(dimension) == other.VoltagesReal.GetLength(dimension)) &&
                        VoltagesReal.Cast<float>().SequenceEqual(other.VoltagesReal.Cast<float>()) &&

                        VoltagesIm.Rank == other.VoltagesIm.Rank &&
                        Enumerable.Range(0, VoltagesIm.Rank).All(dimension =>
                            VoltagesIm.GetLength(dimension) == other.VoltagesIm.GetLength(dimension)) &&
                        VoltagesIm.Cast<float>().SequenceEqual(other.VoltagesIm.Cast<float>()) &&

                        CurrentsReal.Rank == other.CurrentsReal.Rank &&
                        Enumerable.Range(0, VoltagesIm.Rank).All(dimension =>
                            CurrentsReal.GetLength(dimension) == other.CurrentsReal.GetLength(dimension)) &&
                        CurrentsReal.Cast<float>().SequenceEqual(other.CurrentsReal.Cast<float>()) &&

                        CurrentsIm.Rank == other.CurrentsIm.Rank &&
                        Enumerable.Range(0, CurrentsIm.Rank).All(dimension =>
                            CurrentsIm.GetLength(dimension) == other.CurrentsIm.GetLength(dimension)) &&
                        CurrentsIm.Cast<float>().SequenceEqual(other.CurrentsIm.Cast<float>()) &&

                        Saturation.Rank == other.Saturation.Rank &&
                        Enumerable.Range(0, Saturation.Rank).All(dimension =>
                            Saturation.GetLength(dimension) == other.Saturation.GetLength(dimension)) &&
                        Saturation.Cast<ulong>().SequenceEqual(other.Saturation.Cast<ulong>()) &&


                        Timestamps.Rank == other.Timestamps.Rank &&
                        Enumerable.Range(0, Timestamps.Rank).All(dimension =>
                            Timestamps.GetLength(dimension) == other.Timestamps.GetLength(dimension)) &&
                        Timestamps.Cast<long>().SequenceEqual(other.Timestamps.Cast<long>());

            if (PacketIds != null && other.PacketIds != null)
            {
                equal = equal && PacketIds.Rank == other.PacketIds.Rank &&
                        Enumerable.Range(0, PacketIds.Rank).All(dimension =>
                            PacketIds.GetLength(dimension) == other.PacketIds.GetLength(dimension)) &&
                        PacketIds.Cast<ulong>().SequenceEqual(other.PacketIds.Cast<ulong>());
            }
            return equal;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((EITEntry)obj);
        }

        public override int GetHashCode()
        {
            return (Configuration.GetHashCode() * 397) ^ (StartDateTime.GetHashCode() * 397) ^
                   (EndDateTime.GetHashCode() * 397);
        }
    }
}