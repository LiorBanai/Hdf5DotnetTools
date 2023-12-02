using HDF.PInvoke;
using HDF5CSharp.DataTypes;
using System;

namespace HDF5CSharp
{
    public static partial class Hdf5
    {
        public static Settings Settings { get; set; }


        static Hdf5()
        {
            Settings = new Settings();
        }
    }

    public class Settings
    {
        public DateTimeType DateTimeType { get; set; }
        public bool LowerCaseNaming { get; set; }
        public bool H5InternalErrorLoggingEnabled { get; private set; }
        public bool ThrowOnError { get; private set; }
        public bool ThrowOnNonExistNameWhenReading { get; set; }
        public bool OverrideExistingData { get; set; }
        public float Version { get; set; }
        public bool GlobalLoggingEnabled { get; private set; }
        /// <summary>
        /// Character set to use for text strings.
        /// </summary>
        public CharacterSetType CharacterSetType { get; set; }
        /// <summary>
        /// Type of padding to use in character strings.
        /// </summary>
        public CharacterPaddingType CharacterPaddingType { get; set; }
        public Settings()
        {
            DateTimeType = DateTimeType.Ticks;
            ThrowOnError = true;
            OverrideExistingData = true;
            CharacterPaddingType = CharacterPaddingType.SPACEPAD;
            CharacterSetType = CharacterSetType.UTF8;
            Version = 2.0f;
            GlobalLoggingEnabled = true;
        }

        public Settings(DateTimeType dateTimeType, bool lowerCaseNaming, bool throwOnError, bool overrideExistingData)
        {
            DateTimeType = dateTimeType;
            LowerCaseNaming = lowerCaseNaming;
            ThrowOnError = throwOnError;
            OverrideExistingData = overrideExistingData;
        }


        public Settings(DateTimeType dateTimeType, bool lowerCaseNaming, bool throwOnError, bool overrideExistingData, CharacterSetType characterSetType, CharacterPaddingType characterPaddingType) :this(dateTimeType,lowerCaseNaming, throwOnError, overrideExistingData)
        {
            CharacterPaddingType = characterPaddingType;
            CharacterSetType = characterSetType;
        }
        public bool EnableH5InternalErrorReporting(bool enable)
        {
            H5InternalErrorLoggingEnabled = enable;
            if (enable)
            {
                return H5E.set_auto(H5E.DEFAULT, Hdf5Errors.ErrorDelegateMethod, IntPtr.Zero) >= 0;
            }

            return H5E.set_auto(H5E.DEFAULT, null, IntPtr.Zero) >= 0;
        }

        public void EnableLogging(bool enable)
        {
            GlobalLoggingEnabled = enable;
        }

        public void EnableThrowOnErrors(bool enable)
        {
            ThrowOnError = enable;
        }

        public void EnableThrowOnNonExistNameWhenReading(bool enable)
        {
            ThrowOnNonExistNameWhenReading= enable;
        }
    }

    public enum DateTimeType
    {
        Ticks,
        UnixTimeSeconds,
        UnixTimeMilliseconds,
    }
}