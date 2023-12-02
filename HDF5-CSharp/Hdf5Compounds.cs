using HDF.PInvoke;
using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace HDF5CSharp
{
    public static partial class Hdf5
    {
        // information: https://www.hdfgroup.org/ftp/HDF5/examples/examples-by-api/hdf5-examples/1_8/C/H5T/h5ex_t_cmpd.c
        //or: https://www.hdfgroup.org/HDF5/doc/UG/HDF5_Users_Guide-Responsive%20HTML5/index.html#t=HDF5_Users_Guide%2FDatatypes%2FHDF5_Datatypes.htm%3Frhtocid%3Dtoc6.5%23TOC_6_8_Complex_Combinationsbc-22

        /// <summary>
        /// The attributes are applied to the created dataset of the parameter name (not to the top level group long parameter) 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId">the root group id to create the compound</param>
        /// <param name="name">the name of the compound</param>
        /// <param name="list">the values to write</param>
        /// <param name="attributes">the attributes to create at the compound name (not at the group id)</param>
        /// <returns></returns>
        public static (int success, long CreatedgroupId) WriteCompounds<T>(long groupId, string name, IEnumerable<T> list, Dictionary<string, List<string>> attributes) //where T : struct
        {
            Type type = typeof(T);
            if (type.IsValueType)
            {
                var ms = new MemoryStream();
                byte[] bytes = null;
                try
                {
                    BinaryWriter writer = new BinaryWriter(ms);
                    foreach (var strct in list)
                    {
                        writer.Write(GetBytes(strct));
                    }
                    bytes = ms.ToArray();
                }
                catch (Exception e)
                {
                    Hdf5Utils.LogMessage(e.Message, Hdf5LogLevel.Error);
                    return WriteLargeCompounds<T>(groupId, name, list.ToList(), attributes);
                }

                var cnt = list.Count();
                var typeId = CreateType(type);

                var log10 = (int)Math.Log10(cnt);
                ulong pow = (ulong)Math.Pow(10, log10);
                ulong c_s = Math.Min(1000, pow);
                ulong[] chunk_size = { c_s };

                ulong[] dims = { (ulong)cnt };

                long dcpl = 0;
                if (!list.Any() || log10 == 0)
                {
                }
                else
                {
                    dcpl = CreateProperty(chunk_size);
                }

                // Create dataspace.  Setting maximum size to NULL sets the maximum
                // size to be the current size.
                var spaceId = H5S.create_simple(dims.Length, dims, null);


                // Create the dataset if it doesn't exist + remove and create otherwise
                var datasetId = Hdf5Utils.GetDatasetId(groupId, Hdf5Utils.NormalizedName(name), typeId, spaceId, H5P.DEFAULT);

                GCHandle hnd = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var statusId = H5D.write(datasetId, typeId, spaceId, H5S.ALL,
                    H5P.DEFAULT, hnd.AddrOfPinnedObject());


                if (attributes != null)
                {
                    foreach (KeyValuePair<string, List<string>> kvp in attributes)
                    {
                        WriteStringAttributes(datasetId,kvp.Key, kvp.Value);
                    }
                }
                hnd.Free();
                /*
                 * Close and release resources.
                 */
                H5D.close(datasetId);
                H5S.close(spaceId);
                H5T.close(typeId);
                H5P.close(dcpl);

                return (statusId, datasetId);
            }
            else
            {
                byte[] rawdata = ObjectToByteArray(list);
                return WriteCompounds(groupId, name, rawdata, attributes);
            }
        }
        public static (int success, long CreatedgroupId) WriteLargeCompounds<T>(long groupId, string name, List<T> list, Dictionary<string, List<string>> attributes) //where T : struct
        {
            int current = 0;
            int count = list.Count / 10;
            Type type = typeof(T);
            if (type.IsValueType)
            {
                var typeId = CreateType(type);
                var log10 = (int)Math.Log10(count);
                ulong pow = (ulong)Math.Pow(10, log10);
                ulong c_s = Math.Min(1000, pow);
                ulong[] chunk_size = { c_s };

                ulong[] dims = { (ulong)count };
                ulong[] maxDims = { (ulong)list.Count };
                long dcpl = 0;
                if (!list.Any() || log10 == 0)
                {
                }
                else
                {
                    dcpl = CreateProperty(chunk_size);
                }

                // Create dataspace.  Setting maximum size to NULL sets the maximum
                // size to be the current size.
                var spaceId = H5S.create_simple(dims.Length, dims, maxDims);

                // Create the dataset and write the compound data to it.

                var datasetId = Hdf5Utils.GetDatasetId(groupId, Hdf5Utils.NormalizedName(name), typeId, spaceId, H5P.DEFAULT);

                var ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                var chunks = list.Chunk(count).ToList();
                foreach (var strct in chunks.First())
                {
                    writer.Write(GetBytes(strct));
                }

                var bytes = ms.ToArray();

                GCHandle hnd = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var statusId = H5D.write(datasetId, typeId, spaceId, H5S.ALL,
                    H5P.DEFAULT, hnd.AddrOfPinnedObject());

                hnd.Free();
                /*
                 * Close and release resources.
                 */

                foreach (var cu in chunks.Skip(1))
                {
                    current += cu.Count();
                    AppendCompound(cu, current, datasetId);
                }
                foreach (KeyValuePair<string, List<string>> keyValuePair in attributes)
                {
                    WriteStringAttributes(datasetId, keyValuePair.Key, keyValuePair.Value);
                }
                H5D.close(datasetId);
                H5S.close(spaceId);
                H5T.close(typeId);
                H5P.close(dcpl);
              
                return (statusId, datasetId);
            }
            else
            {
                byte[] rawdata = ObjectToByteArray(list);
                return WriteCompounds(groupId, name, rawdata, attributes);
            }
        }
        public static void AppendCompound<T>(IEnumerable<T> list, int oldCount, long datasetId)
        {
            if (list == null || !list.Any())
            {
                Hdf5Utils.LogWarning($"Empty list in {nameof(AppendCompound)}");
                return;
            }
            var _datatype = Hdf5.CreateType(typeof(T));
            var _oldDims = new ulong[] { (ulong)oldCount, 1 };
            var _currentDims = new ulong[] { (ulong)list.Count(), 1 };
            ulong[] zeros = Enumerable.Range(0, 2).Select(z => (ulong)0).ToArray();

            /* Extend the dataset. Dataset becomes 10 x 3  */
            var size = new ulong[] { (ulong)(_oldDims[0] + _currentDims[0]), 1 };

            var _status = H5D.set_extent(datasetId, size);
            ulong[] offset = new[] { _oldDims[0] }.Concat(zeros.Skip(1)).ToArray();

            /* Select a hyperslab in extended portion of dataset  */
            var filespaceId = H5D.get_space(datasetId);
            _status = H5S.select_hyperslab(filespaceId, H5S.seloper_t.SET, offset, null, _currentDims, null);

            /* Define memory space */
            var memId = H5S.create_simple(2, _currentDims, null);
            var ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            foreach (var strct in list)
            {
                writer.Write(GetBytes(strct));
            }
            var bytes = ms.ToArray();

            /* Write the data to the extended portion of dataset  */
            GCHandle hnd = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            _status = H5D.write(datasetId, _datatype, memId, filespaceId,
                H5P.DEFAULT, hnd.AddrOfPinnedObject());
            hnd.Free();
            H5S.close(memId);
            H5S.close(filespaceId);
        }
        public static byte[] ObjectToByteArray<T>(T obj)
        {
            if (obj == null)
            {
                return null;
            }
#pragma warning disable SYSLIB0011

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
#pragma warning restore SYSLIB0011

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        private static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
#pragma warning disable SYSLIB0011
            object obj = binForm.Deserialize(memStream);
            return obj;
#pragma warning restore SYSLIB0011

        }
        public static long CreateProperty(ulong[] chunk_size)
        {
            var dcpl = H5P.create(H5P.DATASET_CREATE);
            H5P.set_layout(dcpl, H5D.layout_t.CHUNKED);
            H5P.set_chunk(dcpl, chunk_size.Length, chunk_size);
            H5P.set_deflate(dcpl, 6);
            return dcpl;
        }

        public static long CreateType(Type t)
        {
            var size = Marshal.SizeOf(t);
            var float_size = Marshal.SizeOf(typeof(float));
            var int_size = Marshal.SizeOf(typeof(int));
            var typeId = H5T.create(H5T.class_t.COMPOUND, new IntPtr(size));
            if (t == typeof(byte))
            {
                H5T.insert(typeId, t.Name, IntPtr.Zero, GetDatatype(t));
                return typeId;
            }
            var compoundInfo = GetCompoundInfo(t);
            foreach (var cmp in compoundInfo)
            {
                //Console.WriteLine(string.Format("{0}  {1}", cmp.name, cmp.datatype));
                // Lines below don't produce an error message but hdfview can't read compounds properly
                //var typeLong = GetDatatype(cmp.type);
                //H5T.insert(typeId, cmp.name, Marshal.OffsetOf(t, cmp.name), typeLong);
                H5T.insert(typeId, cmp.displayName, Marshal.OffsetOf(t, cmp.name), cmp.datatype);
            }
            return typeId;
        }
        private static IEnumerable<T> ChangeStrings<T>(IEnumerable<T> array, FieldInfo[] fields) where T : struct
        {
            foreach (var info in fields)
            {
                if (info.FieldType == typeof(string))
                {
                    var attr = info.GetCustomAttributes(typeof(MarshalAsAttribute), false);
                    MarshalAsAttribute maa = (MarshalAsAttribute)attr[0];
                    object value = info.GetValue(array);
                }
            }

            return array;
        }


        ///
        private static int CalcCompoundSize(Type type, bool useIEEE, ref long id)
        {
            // Create the compound datatype for the file.  Because the standard
            // types we are using for the file may have different sizes than
            // the corresponding native types
            var compoundInfo = GetCompoundInfo(type, useIEEE);
            var curCompound = compoundInfo.Last();
            var compoundSize = curCompound.offset + curCompound.size;
            //Create the compound datatype for memory.
            id = H5T.create(H5T.class_t.COMPOUND, new IntPtr(compoundSize));
            foreach (var cmp in compoundInfo)
            {
                H5T.insert(id, cmp.name, new IntPtr(cmp.offset), cmp.datatype);
            }

            return compoundSize;
        }

        public static IEnumerable<OffsetInfo> GetCompoundInfo(Type type, bool ieee = false)
        {
            //Type t = typeof(T);
            //var strtype = H5T.copy(H5T.C_S1);
            //int strsize = (int)H5T.get_size(strtype);
            int curSize = 0;
            List<OffsetInfo> offsets = new List<OffsetInfo>();
            foreach (var x in type.GetFields())
            {
                var fldType = x.FieldType;
                var marshallAsAttribute = x.GetCustomAttribute<MarshalAsAttribute>();

                long datatype;
                int size;
                if (fldType.IsArray)
                {
                    var dataType = ieee
                        ? GetDatatypeIEEE(fldType.GetElementType())
                        : GetDatatype(fldType.GetElementType());
                    size = marshallAsAttribute.SizeConst > 0 ? marshallAsAttribute.SizeConst : Marshal.SizeOf(dataType);
                    var rank = (uint)fldType.GetArrayRank();
                    var arrayRank = fldType.GetArrayRank();
                    var dimensions = Enumerable.Range(0, arrayRank)
                        .Select(i => (ulong)size)
                        .ToArray();
                    datatype = H5T.array_create(dataType, rank, dimensions);
                }
                else
                {
                    size = marshallAsAttribute?.SizeConst > 0 ? marshallAsAttribute.SizeConst : Marshal.SizeOf(fldType);
                    datatype = ieee ? GetDatatypeIEEE(fldType) : GetDatatype(fldType);
                }

                string displayName = Attribute.GetCustomAttribute(x, typeof(Hdf5EntryNameAttribute)) is Hdf5EntryNameAttribute
                        nameAttribute
                        ? nameAttribute.Name
                        : x.Name;
                OffsetInfo oi = new OffsetInfo
                {
                    name = x.Name,
                    displayName = displayName,
                    type = fldType,
                    datatype = datatype,
                    size = fldType == typeof(string) ? StringLength(x) : !fldType.IsArray ? Marshal.SizeOf(fldType) : size * Marshal.SizeOf(fldType.GetElementType()),
                    offset = 0 + curSize,
                };
                if (oi.datatype == H5T.C_S1)
                {
                    var strtype = H5T.copy(H5T.C_S1);
                    H5T.set_size(strtype, new IntPtr(oi.size));
                    oi.datatype = strtype;
                    //  H5T.close(strtype);
                }
                if (oi.datatype == H5T.STD_I64BE)
                {
                    oi.size = oi.size * 2;
                }

                curSize = curSize + oi.size;

                offsets.Add(oi);
            }
            /* poging om ook properties te bewaren.
             * foreach (var x in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
            {
                bool saveProperty = false;
                bool isNotPublic = x.PropertyType.Attributes != TypeAttributes.Public;
                foreach (Attribute attr in Attribute.GetCustomAttributes(x))
                {
                    var legAttr = attr as Hdf5SaveAttribute;
                    var kind = legAttr?.SaveKind;
                    bool saveAndPrivateProp = isNotPublic && kind == Hdf5Save.Save;
                    bool doNotSaveProp = (kind == Hdf5Save.DoNotSave) ;
                    if (saveAndPrivateProp && !doNotSaveProp)
                    {
                        saveProperty = true;
                        continue;
                    }

                }
                if (!saveProperty)
                    continue;
                var propType = x.PropertyType;
                OffsetInfo oi = new OffsetInfo()
                {
                    name = x.Name,
                    type = propType,
                    datatype = ieee ? GetDatatypeIEEE(propType) : GetDatatype(propType),
                    size = propType == typeof(string) ? stringLength(x) : Marshal.SizeOf(propType),
                    offset = 0 + curSize
                };
                if (oi.datatype == H5T.C_S1)
                {
                    strtype = H5T.copy(H5T.C_S1);
                    H5T.set_size(strtype, new IntPtr(oi.size));
                    oi.datatype = strtype;
                }
                if (oi.datatype == H5T.STD_I64BE)
                    oi.size = oi.size * 2;
                curSize = curSize + oi.size;

                offsets.Add(oi);
            }*/

            return offsets;
        }

        private static int StringLength(MemberInfo fld)
        {
            var attr = fld.GetCustomAttributes(typeof(MarshalAsAttribute), false);
            MarshalAsAttribute maa = (MarshalAsAttribute)attr[0];
            var constSize = maa.SizeConst;
            return constSize;
        }

        public static ulong MaxMemoryAllocationOnRead { get; set; } = 1000000000;
        public static IEnumerable<T> ReadCompounds<T>(long groupId, string name, string alternativeName, bool mandatory)
        {
            Type type = typeof(T);
            if (type.IsValueType)
            {
                long typeId = 0;
                // open dataset
                name = Hdf5Utils.NormalizedName(name);
                alternativeName = Hdf5Utils.NormalizedName(alternativeName);
                var nameToUse = Hdf5Utils.ItemExists(groupId, name, Hdf5ElementType.Dataset) ? name : alternativeName;
                if (!Hdf5Utils.ItemExists(groupId, nameToUse, Hdf5ElementType.Dataset))
                {
                    string error = $"Item {nameToUse} does not exist.";
                    Hdf5Utils.LogMessage(error, Hdf5LogLevel.Warning);
                    if (mandatory || Settings.ThrowOnNonExistNameWhenReading)
                    {
                        Hdf5Utils.LogMessage(error, Hdf5LogLevel.Error);
                        throw new Hdf5Exception(error);
                    }
                    return Enumerable.Empty<T>();
                }
                var datasetId = H5D.open(groupId, nameToUse);

                typeId = CreateType(type);
                var compoundSize = Marshal.SizeOf(type);

                /*
                 * Get dataspace and allocate memory for read buffer.
                 */
                var spaceId = H5D.get_space(datasetId);
                int rank = H5S.get_simple_extent_ndims(spaceId);
                ulong[] dims = new ulong[rank];
                var ndims = H5S.get_simple_extent_dims(spaceId, dims, null);
                IEnumerable<T> strcts;


                ulong rows = dims[0];
                ulong datasetStorageSize = (ulong)rows * (ulong)compoundSize;
                // if more than 100 MB
                if (datasetStorageSize < Hdf5.MaxMemoryAllocationOnRead)
                {
                    byte[] bytes = new byte[(int)rows * compoundSize];
                    // Read the data.
                    GCHandle hnd = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    IntPtr hndAddr = hnd.AddrOfPinnedObject();
                    H5D.read(datasetId, typeId, spaceId, H5S.ALL, H5P.DEFAULT, hndAddr);
                    int counter = 0;
                    strcts = Enumerable.Range(1, (int)rows).Select(i =>
                    {
                        byte[] select = new byte[compoundSize];
                        Array.Copy(bytes, counter, select, 0, compoundSize);
                        T s = fromBytes<T>(select);
                        counter = counter + compoundSize;
                        return s;
                    });

                    /*
                    * Close and release resources.
                     */
                    H5D.vlen_reclaim(typeId, spaceId, H5P.DEFAULT, hndAddr);
                    hnd.Free();
                }
                else // data is overflow and need to read the data in chunks 
                {
                    ulong batch = Hdf5.MaxMemoryAllocationOnRead / (ulong)compoundSize;

                    ulong startindex = 0;
                    // read chunk of 100 line
                    var strcts2 = new List<T>();
                    while (startindex < (ulong)rows)
                    {
                        ulong endIndex = startindex + batch - 1 > rows ? rows - 1 : startindex + batch - 1;
                        T[] res = Hdf5.ReadRowsFromDataset<T>(groupId, name, startindex, endIndex);
                        strcts2.AddRange(res);
                        startindex += batch;
                    }

                    strcts = strcts2;
                }



                H5D.close(datasetId);
                H5S.close(spaceId);
                H5T.close(typeId);

                return strcts;
            }

            var result = ReadCompounds<byte>(groupId, name, alternativeName,mandatory);
            return (IEnumerable<T>)ByteArrayToObject(result.ToArray());
        }
    }
}