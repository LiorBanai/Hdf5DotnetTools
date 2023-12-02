using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5GroupName("system_information")]
    public class SystemInformation : Hdf5BaseFile, IEquatable<SystemInformation>
    {
        [Hdf5EntryName("system_id")] public string SystemId { get; set; }
        [Hdf5EntryName("boards_id")] public string[] BoardIds { get; set; }
        [Hdf5EntryName("data_format_version")] public string DataFormatVersion { get; set; }
        [Hdf5EntryName("software_version")] public string SoftwareVersion { get; set; }
        [Hdf5EntryName("hdf5_version")] public string H5Version { get; set; }
        [Hdf5EntryName("hostname")] public string MachineName { get; set; }
        [Hdf5EntryName("mac_address")] public string MacAddress { get; set; }
        [Hdf5EntryName("ip_address")] public string IPAddress { get; set; }
        [Hdf5EntryName("assemblies_information")] public AssemblyInformationRecord[] Assemblies { get; set; }
        [Hdf5EntryName("system_type")] public string SystemType { get; set; }
        public SystemInformation(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "system_information", logger)
        {
            Assemblies = new AssemblyInformationRecord[0];
            SystemId = "N/A";
            BoardIds = new[] { "N/A", "N/A" };
            DataFormatVersion = "3.0"; //v3 - ecg format changes
            H5Version = "4.1"; //v4 - ecg format changes, V4.1: add System type
            SoftwareVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MachineName = Environment.MachineName;
            MacAddress = GetMacAddress();
            IPAddress = GetLocalIPAddress();
            SystemType = string.Empty;
        }

        public SystemInformation()
        {
            BoardIds = Array.Empty<string>();
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (Exception)
            {
                return "0.0.0.0";
            }

            return "0.0.0.0";
        }

        /// <summary>
        /// Finds the MAC address of the NIC with maximum speed.
        /// </summary>
        /// <returns>The MAC address.</returns>
        private string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }

        public bool Equals(SystemInformation other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return SystemId == other.SystemId && BoardIds.SequenceEqual(other.BoardIds) &&
                   DataFormatVersion == other.DataFormatVersion && SoftwareVersion == other.SoftwareVersion &&
                   MachineName == other.MachineName && MacAddress == other.MacAddress && IPAddress == other.IPAddress &&
                   SystemType == other.SystemType;
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

            return Equals((SystemInformation)obj);
        }

        public override int GetHashCode()
        {
            return (SystemId.GetHashCode() * 397) ^ (DataFormatVersion.GetHashCode() * 397) ^
                   (SoftwareVersion.GetHashCode() * 397) ^ (MachineName.GetHashCode() * 397) ^
                   (MacAddress.GetHashCode() * 397) ^ (IPAddress.GetHashCode() * 397);
        }

        public override string ToString() => $"{nameof(SystemId)}: {SystemId}, {nameof(DataFormatVersion)}: {DataFormatVersion}, {nameof(SoftwareVersion)}: {SoftwareVersion}, {nameof(MachineName)}: {MachineName}, {nameof(MacAddress)}: {MacAddress}, {nameof(IPAddress)}: {IPAddress}. {nameof(SystemType)}: {SystemType}";
    }
}