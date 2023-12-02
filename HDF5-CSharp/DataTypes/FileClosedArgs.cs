using System;

namespace HDF5CSharp.DataTypes
{
    public class FileClosedArgs : EventArgs
    {
        public string ClosedFile { get; }
        public bool CancelRequested { get; set; }

        public FileClosedArgs(string fileName)
        {
            ClosedFile = fileName;
        }
    }
}