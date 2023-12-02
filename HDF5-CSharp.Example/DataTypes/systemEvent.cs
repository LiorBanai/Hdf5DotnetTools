using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace HDF5CSharp.Example.DataTypes
{
    public enum SystemEventType
    {
        NewV2RModelReady = 0,
        NewMeshReady = 1,
        Saturation = 2,
        MissAlignment = 3,
        PAQConnectionError = 4,
        PAQConnectionOk = 5,
        PAQHardwareConnectionError = 6,
        PAQHardwareConnectionOk = 7,
        Dummy = 8,
        ECGCycleDescription = 9,
        SheathDetected = 10,
        LeakAnalysis = 11,
        NetworkAvailabilityOn,
        NetworkAvailabilityOff,
        ECGBodyLeadConnected,
        ECGBodyLeadDisconnected,
        FreeSpace,
    }

    public class SystemEventModel : IEquatable<SystemEventModel>
    {
        public SystemEventType SystemEventType { get; set; }
        public long TimeStamp { get; set; }
        public string EventData { get; set; }

        public SystemEventModel()
        {
            EventData = string.Empty;
        }

        public SystemEventModel(SystemEventType systemEventType, long timeStamp, string eventData)
        {
            SystemEventType = systemEventType;
            TimeStamp = timeStamp;
            EventData = eventData;
        }

        public bool Equals(SystemEventModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return SystemEventType == other.SystemEventType && TimeStamp == other.TimeStamp && EventData == other.EventData;
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((SystemEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)SystemEventType;
                hashCode = (hashCode * 397) ^ TimeStamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (EventData != null ? EventData.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    [DebuggerDisplay("Type={type}, Description={description}, Data={data}, TimeStamp = {timestamp}. Is Error: {isError}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemEvent
    {
        [Hdf5EntryName("timestamp")] public long timestamp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)][Hdf5EntryName("type")] public string type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1000)][Hdf5EntryName("description")] public string description;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200000)][Hdf5EntryName("data")] public string data;
        [Hdf5EntryName("error")] public int isError;
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)]
        private static readonly Dictionary<string, SystemEventType> values = Enum.GetNames(typeof(SystemEventType)).ToDictionary(x => x,
        x => (SystemEventType)Enum.Parse(typeof(SystemEventType), x), StringComparer.OrdinalIgnoreCase);

        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private SystemEventType? eventType;
        public SystemEventType SystemEventType
        {
            get
            {
                if (eventType.HasValue)
                {
                    return eventType.Value;
                }

                eventType = GetEventType();
                return eventType.Value;
            }
        }
        public SystemEvent(long timestamp, string type, string description, string data, bool isError)
        {
            this.eventType = null;
            this.timestamp = timestamp;
            this.type = type;
            this.description = description;
            this.data = data;
            this.isError = isError ? 1 : 0;
        }

        public bool GetErrorAsBoolean() => isError == 1;

        private SystemEventType GetEventType()
        {
            if (values.TryGetValue(type, out var et))
            {
                return et;
            }
            if (Enum.TryParse(type, out et))
            {
                values[type] = et;
                return et;
            }

            return SystemEventType.Dummy;
        }

        public bool Equals(SystemEvent other)
        {
            return timestamp == other.timestamp && type == other.type && description == other.description && data == other.data && isError == other.isError;
        }

        public override bool Equals(object obj)
        {
            return obj is SystemEvent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (type != null ? type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (description != null ? description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (data != null ? data.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isError;
                return hashCode;
            }
        }
    }
}