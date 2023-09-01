using System;

namespace HDF5CSharp.DataTypes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class Hdf5GroupName : Attribute
{

    public Hdf5GroupName(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }
}

public sealed class Hdf5KeyValuesAttributes : Attribute
{
    public string Key { get; set; }
    public string[] Values { get; private set; }
    public Hdf5KeyValuesAttributes(string key, string[] values)
    {
        Values = values;
        Key = key;
    }

}
public sealed class Hdf5Attributes : Attribute
{

    public Hdf5Attributes(string[] names)
    {
        Names = names;
    }

    public string[] Names { get; private set; }
}

public sealed class Hdf5Attribute : Attribute
{

    public Hdf5Attribute(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }
}

[AttributeUsage(AttributeTargets.All)]
public sealed class Hdf5MandatoryReadElementAttribute : Attribute
{
    public Hdf5MandatoryReadElement MandatoryRead { get; }
    public Hdf5MandatoryReadElementAttribute(Hdf5MandatoryReadElement readKind)
    {
        MandatoryRead = readKind;
    }

}

[AttributeUsage(AttributeTargets.All)]
public sealed class Hdf5ReadWriteAttribute : Attribute
{
    public Hdf5ReadWrite ReadKind { get; }
    public Hdf5ReadWriteAttribute(Hdf5ReadWrite readKind)
    {
        ReadKind = readKind;
    }

}
[Obsolete("Recommended Attribute is Hdf5ReadWriteAttribute")]
[AttributeUsage(AttributeTargets.All)]
public sealed class Hdf5SaveAttribute : Attribute
{
    public Hdf5Save SaveKind { get; }
    public Hdf5SaveAttribute(Hdf5Save saveKind)
    {
        SaveKind = saveKind;
    }

}

[AttributeUsage(AttributeTargets.All)]
public sealed class Hdf5EntryNameAttribute : Attribute
{
    public string Name { get; }
    public Hdf5EntryNameAttribute(string name)
    {
        Name = name;
    }

}
