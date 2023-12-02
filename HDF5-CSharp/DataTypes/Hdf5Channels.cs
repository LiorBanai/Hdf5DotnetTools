namespace HDF5CSharp.DataTypes
{
    [Hdf5GroupName("Channels")]
    public class Hdf5Channels
    {
        public Hdf5Channels(int length)
        {
            Labels = new string[length];
            Dimensions = new string[length];
            Amplifications = new double[length];
            Offsets = new double[length];
            SamplingRates = new double[length];
            NrOfSamples = new int[length];
        }
        public string[] Labels { get; set; }
        public string[] Dimensions { get; set; }
        public double[] Amplifications { get; set; }
        public double[] Offsets { get; set; }
        public double[] SamplingRates { get; set; }
        public int[] NrOfSamples { get; set; }
    }
}