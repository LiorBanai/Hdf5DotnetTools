using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5GroupName("injection")]
    public class InjectionGroup : Hdf5BaseFile
    {
        public InjectionGroup(in long fileId, in long groupRoot, ILogger logger) : base(fileId, groupRoot, "injection", logger)
        {
        }
    }
}