using HDF5CSharp.Interfaces;
using System;
using System.Collections.Generic;

namespace HDF5CSharp
{
    public class Hdf5AttributeRW : IHdf5ReaderWriter
    {
        public (bool Success, Array Result) ReadToArray<T>(long groupId, string name, string alternativeName, bool mandatory)
        {
            return Hdf5.ReadPrimitiveAttributes<T>(groupId, name, alternativeName, mandatory);
        }

        public (bool Success, IEnumerable<string>) ReadStrings(long groupId, string name, string alternativeName, bool mandatory)
        {
            return Hdf5.ReadStringAttributes(groupId, name, alternativeName, mandatory);
        }

        public (int Success, long CreatedgroupId) WriteFromArray<T>(long groupId, string name, Array dset)
        {
            return Hdf5.WritePrimitiveAttribute<T>(groupId, name, dset);
        }

        public (int Success, long CreatedgroupId) WriteStrings(long groupId, string name, IEnumerable<string> collection, string datasetName = null)
        {
            return Hdf5.WriteStringAttributes(groupId, name, (string[])collection, datasetName);
        }
        public (int Success, long CreatedgroupId) WriteAsciiStringAttributes(long groupId, string name, IEnumerable<string> collection, string datasetName = null)
        {
            return Hdf5.WriteAsciiStringAttributes(groupId, name, (string[])collection, datasetName);
        }
    }
}