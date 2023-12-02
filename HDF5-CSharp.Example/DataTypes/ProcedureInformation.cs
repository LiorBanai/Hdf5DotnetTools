using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5Attributes(new[] { "General information of the procedure" })]
    [Hdf5GroupName("procedure_information")]
    public class ProcedureInformation : Hdf5BaseFile, IEquatable<ProcedureInformation>
    {
        [Hdf5EntryName("procedure_directory")] public string ProcedureDirectory { get; set; }

        [Hdf5("the type of procedure")]
        [Hdf5EntryName("procedure_type")]
        public string ProcedureType { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.ReadOnly)][Hdf5EntryName("start_datetime")] public long StartDateTimeTimestamp { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.ReadOnly)][Hdf5EntryName("end_datetime")] public long EndDateTimestamp { get; set; }
        [Hdf5EntryName("start_datetime")] public DateTime StartDateTime { get; set; }
        [Hdf5EntryName("end_datetime")] public DateTime EndDateTime { get; set; }
        [Hdf5EntryName("procedure_institute")] public string ProcedureInstitute { get; set; }

        [Hdf5EntryName("procedure_geolocation")]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5000)]
        public string GeolocationData;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        [Hdf5EntryName("procedure_guid")]
        public string guid;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10000)]
        [Hdf5EntryName("procedure_protocol")]
        public string ProcedureProtocol;


        // Timezone information
        [Hdf5EntryName("local_timezone")]
        public string LocalTimeZone { get; set; }

        [Hdf5EntryName("utc_offset")]
        public int UtcOffset { get; set; }

        [Hdf5EntryName("is_daylight_saving")]
        public bool IsDaylightSaving { get; set; }

        [Hdf5EntryName("reviewer")]
        public string Reviewer { get; set; }





        public ProcedureInformation(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "procedure_information", logger)
        {
            ProcedureDirectory = string.Empty;
            ProcedureType = string.Empty;
            GeolocationData = string.Empty;
            ProcedureInstitute = string.Empty;
            Reviewer = string.Empty;
        }

        public ProcedureInformation()
        {
        }

        public bool Equals(ProcedureInformation other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ProcedureDirectory == other.ProcedureDirectory && ProcedureType == other.ProcedureType &&
                   StartDateTime.EqualsUpToMilliseconds(other.StartDateTime) && EndDateTime.EqualsUpToMilliseconds(other.EndDateTime) &&
                   guid == other.guid;
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

            return Equals((ProcedureInformation)obj);
        }

        public override int GetHashCode()
        {
            return (ProcedureDirectory.GetHashCode() * 397) ^ (ProcedureType.GetHashCode() * 397) ^
                   (StartDateTime.GetHashCode() * 397) ^ (EndDateTime.GetHashCode() * 397) ^
                   (guid.GetHashCode() * 397);
        }

        public override string ToString() => $"{nameof(ProcedureDirectory)}: {ProcedureDirectory}, {nameof(ProcedureType)}: {ProcedureType}, {nameof(StartDateTime)}: {StartDateTime}, {nameof(EndDateTime)}: {EndDateTime}, {nameof(guid)}: {guid}";
    }
}