using HDF5CSharp.DataTypes;
using System;
using System.Linq;

namespace HDF5CSharp.Example.DataTypes
{
    public class ECGData : IEquatable<ECGData>
    {
        [Hdf5EntryName("start_datetime")] public DateTime StartDateTime { get; set; }
        [Hdf5EntryName("end_datetime")] public DateTime EndDateTime { get; set; }
        [Hdf5EntryName("sampling_rate")] public int SamplingRate { get; set; }
        [Hdf5EntryName("Signals_unfiltered")] public double[,] UnfilteredSignal { get; set; }
        [Hdf5EntryName("Signals_filtered")] public double[,] FilteredSignal { get; set; }
        [Hdf5EntryName("timestamps")] public long[,] Timestamps { get; set; }
        [Hdf5EntryName("packetids")] public long[,] PacketIds { get; set; }

        public ECGData()
        {
            Timestamps = new long[0, 0];
            FilteredSignal = new double[0, 0];
            UnfilteredSignal = new double[0, 0];
            PacketIds = new long[0, 0];
        }

        public bool Equals(ECGData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return StartDateTime.EqualsUpToMilliseconds(other.StartDateTime) &&
                   EndDateTime.EqualsUpToMilliseconds(other.EndDateTime) &&
                   SamplingRate == other.SamplingRate &&


                   UnfilteredSignal.Rank == other.UnfilteredSignal.Rank &&
                   Enumerable.Range(0, UnfilteredSignal.Rank).All(dimension =>
                       UnfilteredSignal.GetLength(dimension) == other.UnfilteredSignal.GetLength(dimension)) &&
                   UnfilteredSignal.Cast<double>().SequenceEqual(other.UnfilteredSignal.Cast<double>()) &&

                   FilteredSignal.Rank == other.FilteredSignal.Rank &&
                   Enumerable.Range(0, FilteredSignal.Rank).All(dimension =>
                       FilteredSignal.GetLength(dimension) == other.FilteredSignal.GetLength(dimension)) &&
                   FilteredSignal.Cast<double>().SequenceEqual(other.FilteredSignal.Cast<double>()) &&
                   Timestamps.Rank == other.Timestamps.Rank &&
                   Enumerable.Range(0, Timestamps.Rank).All(dimension =>
                       Timestamps.GetLength(dimension) == other.Timestamps.GetLength(dimension)) &&
                   Timestamps.Cast<long>().SequenceEqual(other.Timestamps.Cast<long>());
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

            return Equals((ECGData)obj);
        }

        public override int GetHashCode()
        {
            return (StartDateTime.GetHashCode() * 397) ^ (EndDateTime.GetHashCode() * 397) ^ (SamplingRate * 397);
        }
    }
}