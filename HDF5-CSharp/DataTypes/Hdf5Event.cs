using System;
using System.Runtime.InteropServices;

namespace HDF5CSharp.DataTypes
{
    /// <summary>
    /// 
    /// </summary>
    [Hdf5GroupName("Event")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Hdf5Event
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Event;

        /// <summary>
        /// Time property. Datetimes can't be saved so the TimeTicks field gets saved
        /// </summary>
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)]
        public DateTime Time
        {
            get => new DateTime(TimeTicks);
            set => TimeTicks = Hdf5Conversions.FromDatetime(value, Hdf5.Settings.DateTimeType);
        }

        public long TimeTicks;

        /// <summary>
        /// Duration property. Timespans can't be saved so the DurationTicks field gets saved
        /// </summary>
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)]
        public TimeSpan Duration
        {
            get => new TimeSpan(DurationTicks);
            set => DurationTicks = value.Ticks;
        }

        public long DurationTicks;
    }
}