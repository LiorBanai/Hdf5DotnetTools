using System;
using System.Collections.Generic;

namespace HDF5CSharp.Interfaces
{
    public interface IHdf5ReaderWriter
    {
        (int Success, long CreatedgroupId) WriteFromArray<T>(long groupId, string name, Array dset);
        (bool Success, IEnumerable<string>) ReadStrings(long groupId, string name, string alternativeName, bool mandatory);
        (bool Success, Array Result) ReadToArray<T>(long groupId, string name, string alternativeName, bool mandatory);
        (int Success, long CreatedgroupId) WriteStrings(long groupId, string name, IEnumerable<string> collection, string datasetName = null);
    }
}