using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HDF5CSharp.Example.DataTypes
{
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct AssemblyInformationRecord
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
        [Hdf5EntryName("file_name")] public readonly string FileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        [Hdf5EntryName("file_version")] public readonly string FileVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        [Hdf5EntryName("product_version")] public readonly string ProductVersion;
        [Hdf5EntryName("date_modified")] public readonly long DateModified;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        [Hdf5EntryName("date_modified_time")] public readonly string DateModifiedDisplayName;
        public AssemblyInformationRecord(string filename, string fileVersion, string productVersion, long dateModified)
        {
            FileName = filename;
            FileVersion = fileVersion;
            ProductVersion = productVersion;
            DateModified = dateModified;
            DateModifiedDisplayName = DateTimeOffset.FromUnixTimeMilliseconds(dateModified).ToString();
        }

        public bool Equals(AssemblyInformationRecord other) => FileName == other.FileName && FileVersion == other.FileVersion && ProductVersion == other.ProductVersion && DateModified == other.DateModified;

        public override bool Equals(object obj) => obj is AssemblyInformationRecord other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FileVersion != null ? FileVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ProductVersion != null ? ProductVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DateModified.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"{nameof(FileName)}: {FileName}, {nameof(FileVersion)}: {FileVersion}, {nameof(ProductVersion)}: {ProductVersion}, {nameof(DateModified)}: {DateModified}";
    }
}