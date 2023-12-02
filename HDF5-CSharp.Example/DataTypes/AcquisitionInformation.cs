using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5GroupName("acquisition_information")]
    public class AcquisitionInformation : Hdf5BaseFile
    {
        private string Protocol { get; set; }

        public AcquisitionInformation(AcquisitionProtocolParameters acquisitionProtocol, long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "acquisition_information", logger)
        {
            Protocol = acquisitionProtocol.AsJson();
        }

        public AcquisitionInformation()
        {
        }
    }
}