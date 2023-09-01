using HDF5CSharp.DataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HDF5CSharp;

public partial class Hdf5
{


    public static object WriteObject(long groupId, object writeValue, string groupName = null)
    {
        if (writeValue == null)
        {
            throw new ArgumentNullException(nameof(writeValue));
        }
        Type tyObject = writeValue.GetType();
        Attribute attribute = Attribute.GetCustomAttributes(tyObject).SingleOrDefault(a => a is Hdf5GroupName);
        if (string.IsNullOrEmpty(groupName) && attribute != null)
        {
            groupName = ((Hdf5GroupName)attribute).Name;
        }

        bool skip = SkipSaveProcessing(Attribute.GetCustomAttributes(tyObject));
        if (skip)
        {
            return writeValue;
        }

        bool createGroupName = !string.IsNullOrWhiteSpace(groupName);
        if (createGroupName)
        {
            groupId = CreateOrOpenGroup(groupId, groupName);
        }
        WriteProperties(tyObject, writeValue, groupId);
        WriteFields(tyObject, writeValue, groupId);
        WriteHdf5Attributes(tyObject, groupId, groupName);
        if (createGroupName)
        {
            if (CloseGroup(groupId) != 0)
            {
                Hdf5Utils.LogMessage($"Error closing group: {groupName}({groupId})",Hdf5LogLevel.Error);
            }
        }

        return (writeValue);
    }

    private static void WriteHdf5Attributes(Type type, long groupId, string name)
    {
        foreach (Attribute attr in Attribute.GetCustomAttributes(type))
        {
            if (attr is Hdf5Attribute h5At)
            {
                WriteAttribute(groupId, name, h5At.Name);
            }
            if (attr is Hdf5Attributes h5Ats)
            {
                WriteAttributes<string>(groupId, name, h5Ats.Names);
            }
        }
        foreach (var attribute in Attributes(type))
        {
            WriteAttributes<string>(groupId, attribute.Key, attribute.Value.ToArray());
        }

    }

    private static void WriteFields(Type tyObject, object writeValue, long groupId)
    {
        FieldInfo[] miMembers = tyObject.GetFields(BindingFlags.DeclaredOnly | /*BindingFlags.NonPublic |*/ BindingFlags.Instance | BindingFlags.Public);

        foreach (FieldInfo info in miMembers)
        {
            if (SkipSaveProcessing(Attribute.GetCustomAttributes(info)))
            {
                continue;
            }

            object infoVal = info.GetValue(writeValue);
            if (infoVal == null)
            {
                continue;
            }

            Dictionary<string, List<string>> attributes = Attributes(info);
            string name = info.Name;
            foreach (Attribute attr in Attribute.GetCustomAttributes(info))
            {
                if (attr is Hdf5EntryNameAttribute hdf5EntryNameAttribute)
                {
                    name = hdf5EntryNameAttribute.Name;
                }
            }
            WriteField(infoVal, attributes, groupId, name);
        }
    }

    private static void WriteProperties(Type tyObject, object writeValue, long groupId)
    {
        PropertyInfo[] miMembers = tyObject.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        foreach (PropertyInfo info in miMembers)
        {
            if (SkipSaveProcessing(Attribute.GetCustomAttributes(info)))
            {
                continue;
            }

            object infoVal = info.GetValue(writeValue, null);
            if (infoVal == null)
            {
                continue;
            }

            Dictionary<string, List<string>> attributes = Attributes(info);
            string name = info.Name;
            foreach (Attribute attr in Attribute.GetCustomAttributes(info))
            {
                if (attr is Hdf5EntryNameAttribute hdf5EntryNameAttribute)
                {
                    name = hdf5EntryNameAttribute.Name;

                }
            }
            WriteField(infoVal, attributes, groupId, name);
        }
    }



    private static void WriteField(object infoVal, Dictionary<string, List<string>> attributes, long groupId, string name)
    {
        Type ty = infoVal.GetType();
        TypeCode code = Type.GetTypeCode(ty);
        foreach (var attribute in Attributes(ty))
        {
            if (!attributes.ContainsKey(attribute.Key))
            {
                attributes.Add(attribute.Key, attribute.Value);
            }
        }

        if (ty.IsArray)
        {
            var elType = ty.GetElementType();
            TypeCode elCode = Type.GetTypeCode(elType);
            if (elCode != TypeCode.Object || ty == typeof(TimeSpan[]))
            {
                dsetRW.WriteArray(groupId, name, (Array)infoVal, attributes);
            }
            else
            {
                CallByReflection<(int, long)>(nameof(WriteCompounds), elType, new[] { groupId, name, infoVal, attributes });
            }
        }
        else if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elType = Hdf5Utils.GetEnumerableType(ty);

            TypeCode elCode = Type.GetTypeCode(elType);
          

            if (elCode != TypeCode.Object || ty == typeof(List<TimeSpan>))
            {
                IList infoAsList = (IList)infoVal;
                Array arr = Array.CreateInstance(elType, infoAsList.Count);
                for (var i = 0; i < infoAsList.Count; i++)
                {
                    arr.SetValue(infoAsList[i], i);
                }
                dsetRW.WriteArray(groupId, name, arr, attributes);
            }
            else
            {
                CallByReflection<(int, long)>(nameof(WriteCompounds), elType, new[] { groupId, name, infoVal, attributes });
            }

        }
#if NET
        else if (ty == typeof(Half) || ty == typeof(TimeOnly) || ty == typeof(DateOnly))
        {
            (int success, long CreatedgroupId) =// WriteOneValue(groupId, name, infoVal);

                CallByReflection<(int, long)>(nameof(WriteOneValue), ty, new[] { groupId, name, infoVal, attributes });

        }
#endif
        else if (primitiveTypes.Contains(code) || ty == typeof(TimeSpan))
        {
            (int success, long CreatedgroupId) =// WriteOneValue(groupId, name, infoVal);

                CallByReflection<(int, long)>(nameof(WriteOneValue), ty, new[] { groupId, name, infoVal, attributes });
            //todo: fix it
            //add its attributes if there are: 
            //foreach (Attribute attr in Attribute.GetCustomAttributes(filedInfo))
            //{
            //    if (attr is Hdf5Attribute)
            //    {
            //        var h5at = attr as Hdf5Attribute;
            //        WriteStringAttribute(groupId, name, h5at.Name, name);
            //    }

            //    if (attr is Hdf5Attributes)
            //    {
            //        var h5ats = attr as Hdf5Attributes;
            //        WriteAttributes<string>(groupId, name, h5ats.Names, attr.);
            //    }
            //}
        }
        else
        {
            WriteObject(groupId, infoVal, name);
        }
    }

    static T CallByReflection<T>(string name, Type typeArg, object[] values)
    {
        // Just for simplicity, assume it's public etc
        MethodInfo method = typeof(Hdf5).GetMethod(name);
        MethodInfo generic = method.MakeGenericMethod(typeArg);
        return (T)generic.Invoke(null, values);

    }

}
