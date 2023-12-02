namespace HDF5CSharp.DataTypes
{
    public class TabularData<T>
    {
        public string HDF5Name { get; set; }
        public T[,] Data { get; set; }
        public bool ReadSuccessful { get; set; }
    }
}