namespace HDF5CSharp.DataTypes
{
    /// <summary>
    /// Character set to use for text strings.
    /// </summary>
    public enum CharacterSetType
    {
        /// <summary>
        /// US ASCII [value = 0].
        /// </summary>
        ASCII = 0,

        /// <summary>
        /// UTF-8 Unicode encoding [value = 1].
        /// </summary>
        UTF8 = 1,
    }

    /// <summary>
    /// Type of padding to use in character strings.
    /// </summary>
    public enum CharacterPaddingType
    {
        /// <summary>
        /// null terminate like in C
        /// </summary>
        NULLTERM = 0,

        /// <summary>
        /// pad with nulls
        /// </summary>
        NULLPAD = 1,

        /// <summary>
        /// pad with spaces like in Fortran
        /// </summary>
        SPACEPAD = 2,
    }

    public enum Hdf5Save
    {
        Save,
        DoNotSave,
    }
    public enum Hdf5ReadWrite
    {
        DoNothing,
        ReadOnly,
        WriteOnly,
        ReadWrite,
    }
    public enum Hdf5MandatoryReadElement
    {
        CanSkipRead,
        MandatoryForRead,
    }
    public enum Hdf5LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
    }
}