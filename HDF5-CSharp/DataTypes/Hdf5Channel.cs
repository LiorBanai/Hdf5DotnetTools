using System.Runtime.InteropServices;

namespace HDF5CSharp.DataTypes
{
    [Hdf5GroupName("Channel")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Hdf5Channel
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string Label;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string Dimension;
        public double Amplification;
        public double Offset;
        public double SamplingRate;
        public ulong NrOfSamples;
    }
}