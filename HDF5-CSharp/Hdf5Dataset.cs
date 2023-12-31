using HDF.PInvoke;
using HDF5CSharp.DataTypes;
using HDF5CSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HDF5CSharp
{
    public class Hdf5Dataset : IHdf5ReaderWriter
    {
        public (bool Success, Array Result) ReadToArray<T>(long groupId, string name, string alternativeName, bool mandatory)
        {
            return Hdf5.ReadDatasetToArray<T>(groupId, name, alternativeName, mandatory);
        }

        public (int Success, long CreatedgroupId) WriteFromArray<T>(long groupId, string name, Array dset)
        {
            return Hdf5.WriteDatasetFromArray<T>(groupId, name, dset);
        }
        public (int Success, long CreatedgroupId) WriteStrings(long groupId, string name, IEnumerable<string> collection, string datasetName = null)
        {
            return Hdf5.WriteStrings(groupId, name, collection.ToArray());
        }
        public (int Success, long CreatedgroupId) WriteNumericAttributes<T>(long groupId, string name, IEnumerable<T> collection, string datasetName = null) where T : struct
        {
            return Hdf5.WriteIntegerAttributes(groupId, name, collection.ToArray());
        }

        public Array ReadStructs<T>(long groupId, string name, string alternativeName, bool mandatory) where T : struct
        {
            return Hdf5.ReadCompounds<T>(groupId, name, alternativeName, mandatory).ToArray();
        }

        public (bool Success, IEnumerable<string>) ReadStrings(long groupId, string name, string alternativeName, bool mandatory)
        {
            return Hdf5.ReadStrings(groupId, name, alternativeName, mandatory);
        }
    }
    public static partial class Hdf5
    {
        private static readonly Hdf5ReaderWriter dsetRW = new Hdf5ReaderWriter(new Hdf5Dataset());

        public static long OpenDatasetIfExists(long fileOrGroupId, string name, string alternativeName)
        {
            if (Hdf5Utils.ItemExists(fileOrGroupId, name, Hdf5ElementType.Dataset))
            {
                return H5D.open(fileOrGroupId, name);
            }
            if (Hdf5Utils.ItemExists(fileOrGroupId, alternativeName, Hdf5ElementType.Dataset))
            {
                return H5D.open(fileOrGroupId, alternativeName);
            }

            return -1;
        }

        public static long OpenAttributeIfExists(long fileOrGroupId, string name, string alternativeName)
        {
            if (Hdf5Utils.ItemExists(fileOrGroupId, name, Hdf5ElementType.Attribute))
            {
                return H5A.open(fileOrGroupId, name);
            }
            if (Hdf5Utils.ItemExists(fileOrGroupId, alternativeName, Hdf5ElementType.Attribute))
            {
                return H5A.open(fileOrGroupId, alternativeName);
            }

            return -1;
        }
        [Obsolete("Use ItemExists")]
        public static bool DatasetExists(long groupId, string datasetName) => Hdf5Utils.ItemExists(groupId, datasetName, Hdf5ElementType.Dataset);

        /// <summary>
        /// Reads an n-dimensional dataset.
        /// </summary>
        /// <typeparam name="T">Generic parameter strings or primitive type</typeparam>
        /// <param name="groupId">id of the group. Can also be a file Id</param>
        /// <param name="name">name of the dataset</param>
        /// <param name="alternativeName">Alternative name</param>
        /// <returns>The n-dimensional dataset</returns>
        public static (bool Success, Array Result) ReadDatasetToArray<T>(long groupId, string name, string alternativeName = "", bool mandatory = false) //where T : struct
        {
            var (valid, datasetName) = Hdf5Utils.GetRealName(groupId, name, alternativeName);
            if (!valid)
            {
                string error = $"Error reading {groupId}. Name:{name}. AlternativeName:{alternativeName}";
                Hdf5Utils.LogMessage(error, Hdf5LogLevel.Warning);
                if (mandatory || Settings.ThrowOnNonExistNameWhenReading)
                {
                    Hdf5Utils.LogMessage(error, Hdf5LogLevel.Error);
                    throw new Hdf5Exception(error);
                }

                return (false, Array.Empty<T>());
            }
            var datasetId = H5D.open(groupId, datasetName);
            var datatype = GetDatatype(typeof(T));
            var spaceId = H5D.get_space(datasetId);
            int rank = H5S.get_simple_extent_ndims(spaceId);
            long count = H5S.get_simple_extent_npoints(spaceId);
            Type type = typeof(T);
            Array result = Array.Empty<T>();
            if (rank >= 0 && count >= 0)
            {
                int rankChunk;
                ulong[] maxDims = new ulong[rank];
                ulong[] dims = new ulong[rank];
                ulong[] chunkDims = new ulong[rank];
                long simpleId = H5S.get_simple_extent_dims(spaceId, dims, maxDims);
                long[] lengths = dims.Select(d => Convert.ToInt64(d)).ToArray();
                result = Array.CreateInstance(type, lengths);

                //var typeId = H5D.get_type(datasetId);
                //var mem_type = H5T.copy(datatype);
                if (datatype == H5T.C_S1)
                {
                    H5T.set_size(datatype, new IntPtr(2));
                }

                var propId = H5D.get_create_plist(datasetId);

                if (H5D.layout_t.CHUNKED == H5P.get_layout(propId))
                {
                    rankChunk = H5P.get_chunk(propId, rank, chunkDims);
                }

                var memId = H5S.create_simple(rank, dims, maxDims);
                var hnd = GCHandle.Alloc(result, GCHandleType.Pinned);
                H5D.read(datasetId, datatype, memId, spaceId,
                    H5P.DEFAULT, hnd.AddrOfPinnedObject());

                H5S.close(memId);
                H5P.close(propId);
                hnd.Free();
            }
            H5D.close(datasetId);
            H5S.close(spaceId);
            return (true, result);
        }

        /// <summary>
        /// Reads part of a two dimensional dataset.
        /// </summary>
        /// <typeparam name="T">Generic parameter strings or primitive type</typeparam>
        /// <param name="groupId">id of the group. Can also be a file Id</param>
        /// <param name="name">name of the dataset</param>
        /// <param name="beginIndex">The index of the first row to be read</param>
        /// <param name="endIndex">The index of the last row to be read</param>
        /// <returns>The two dimensional dataset</returns>
        public static T[,] ReadDataset<T>(long groupId, string name, ulong beginIndex, ulong endIndex) //where T : struct
        {
            ulong[] start = { 0, 0 }, stride = null, count = { 0, 0 },
                block = null, offsetOut = { 0, 0 };
            var datatype = GetDatatype(typeof(T));

            var datasetId = H5D.open(groupId, Hdf5Utils.NormalizedName(name));
            var spaceId = H5D.get_space(datasetId);
            int rank = H5S.get_simple_extent_ndims(spaceId);
            ulong[] maxDims = new ulong[rank];
            ulong[] dims = new ulong[rank];
            ulong[] chunkDims = new ulong[rank];
            var memId_n = H5S.get_simple_extent_dims(spaceId, dims, maxDims);

            start[0] = beginIndex;
            start[1] = 0;
            count[0] = endIndex - beginIndex + 1;
            count[1] = dims[1];

            var status = H5S.select_hyperslab(spaceId, H5S.seloper_t.SET, start, stride, count, block);

            // Define the memory dataspace.
            T[,] dset = new T[count[0], count[1]];
            var memId = H5S.create_simple(rank, count, null);

            // Define memory hyperslab. 
            status = H5S.select_hyperslab(memId, H5S.seloper_t.SET, offsetOut, null,
                         count, null);

            // Read data from hyperslab in the file into the hyperslab in 
            // memory and display.             
            GCHandle hnd = GCHandle.Alloc(dset, GCHandleType.Pinned);
            H5D.read(datasetId, datatype, memId, spaceId,
                H5P.DEFAULT, hnd.AddrOfPinnedObject());
            hnd.Free();
            H5D.close(datasetId);
            H5S.close(spaceId);
            H5S.close(memId);
            return dset;
        }

        /// <summary>
        /// Reads part of a two dimensional dataset.
        /// </summary>
        /// <typeparam name="T">Generic parameter strings or primitive type</typeparam>
        /// <param name="groupId">id of the group. Can also be a file Id</param>
        /// <param name="name">name of the dataset</param>
        /// <param name="beginIndex">The index of the first row to be read</param>
        /// <param name="endIndex">The index of the last row to be read</param>
        /// <returns>The two dimensional dataset</returns>
        public static T[] ReadRowsFromDataset<T>(long groupId, string name, ulong beginIndex, ulong endIndex)
        {
            ulong[] start = { 0, 0 }, stride = null, count = { 0 },
            block = null, offsetOut = { 0, 0 };
            Type type = typeof(T);
            var typeId = CreateType(type);
            string normalizedName = Hdf5Utils.NormalizedName(name);
            var datasetId = H5D.open(groupId, normalizedName);
            var spaceId = H5D.get_space(datasetId);
            int rank = H5S.get_simple_extent_ndims(spaceId);
            ulong[] maxDims = new ulong[rank];
            ulong[] dims = new ulong[rank];
            ulong[] chunkDims = new ulong[rank];
            var memId_n = H5S.get_simple_extent_dims(spaceId, dims, maxDims);

            start[0] = beginIndex;
            start[1] = 0;
            count[0] = endIndex - beginIndex + 1;

            var status = H5S.select_hyperslab(spaceId, H5S.seloper_t.SET, start, stride, count, block);
            var memId = H5S.create_simple(rank, count, null);

            // Define memory hyperslab.
            status = H5S.select_hyperslab(memId, H5S.seloper_t.SET, offsetOut, null,
            count, null);

            // Define the memory dataspace.
            int msgSize = Marshal.SizeOf(type) * (int)count[0];
            IntPtr ptr = Marshal.AllocHGlobal(msgSize);

            // Read data from hyperslab in the file into the hyperslab in
            // memory and display.
            H5D.read(datasetId, typeId, memId, spaceId,
            H5P.DEFAULT, ptr);
            T[] msg = new T[count[0]];
            for (int i = 0; i < (int)count[0]; i++)
            {
                IntPtr ins = new IntPtr(ptr.ToInt64() + i * Marshal.SizeOf(type));
                msg[i] = Marshal.PtrToStructure<T>(ins);
            }
            H5D.close(datasetId);
            H5S.close(spaceId);
            H5S.close(memId);
            Marshal.FreeHGlobal(ptr);
            return msg;
        }

        /// <summary>
        /// Reads a dataset or string array with one value in it
        /// </summary>
        /// <typeparam name="T">Generic parameter strings or primitive type</typeparam>
        /// <param name="groupId">id of the group. Can also be a file Id</param>
        /// <param name="name">name of the dataset</param>
        /// <param name="alternativeName"></param>
        /// <param name="mandatory"></param>
        /// <returns>One value or string</returns>
        public static T ReadOneValue<T>(long groupId, string name, string alternativeName = "", bool mandatory = false)
        {
            var dset = dsetRW.ReadArray<T>(groupId, name, alternativeName, mandatory);
            int[] first = new int[dset.Result.Rank].Select(f => 0).ToArray();
            T result = (T)dset.Result.GetValue(first);
            return result;
        }

        public static (bool Success, Array Result) ReadDataset<T>(long groupId, string name, string alternativeName = "", bool mandatory = false)
        {
            return dsetRW.ReadArray<T>(groupId, name, alternativeName, mandatory);
        }

        /// <summary>
        /// Writes one value to a hdf5 file
        /// </summary>
        /// <typeparam name="T">Generic parameter strings or primitive type</typeparam>
        /// <param name="groupId">id of the group. Can also be a file Id</param>
        /// <param name="name">name of the dataset</param>
        /// <param name="dset">The dataset</param>
        /// <returns>status of the write method</returns>
        public static (int Success, long CreatedgroupId) WriteOneValue<T>(long groupId, string name, T dset, Dictionary<string, List<string>> attributes)
        {
            if (typeof(T) == typeof(string))

            //WriteStrings(groupId, name, new string[] { dset.ToString() });
            {
                return dsetRW.WriteArray(groupId, name, new T[1] { dset }, attributes);
            }

            Array oneVal = new T[1, 1] { { dset } };
            return dsetRW.WriteArray(groupId, name, oneVal, attributes);
        }

        public static void WriteDataset(long groupId, string name, Array collection)
        {
            dsetRW.WriteArray(groupId, name, collection, new Dictionary<string, List<string>>());
        }

        public static (int Success, long CreatedgroupId) WriteDatasetFromArray<T>(long groupId, string name, Array dset) //where T : struct
        {
            int rank = dset.Rank;
            ulong[] dims = Enumerable.Range(0, rank).Select(i => { return (ulong)dset.GetLength(i); }).ToArray();

            ulong[] maxDims = null;
            var spaceId = H5S.create_simple(rank, dims, maxDims);
            var datatype = GetDatatype(typeof(T));
            var typeId = H5T.copy(datatype);
            if (datatype == H5T.C_S1)
            {
                H5T.set_size(datatype, new IntPtr(2));
            }

            string normalizedName = Hdf5Utils.NormalizedName(name);
            var datasetId = Hdf5Utils.GetDatasetId(groupId, normalizedName, datatype, spaceId, H5P.DEFAULT);
            if (datasetId == -1L)
            {
                return (-1, -1L);
            }

            GCHandle hnd = GCHandle.Alloc(dset, GCHandleType.Pinned);
            var result = H5D.write(datasetId, datatype, H5S.ALL, H5S.ALL, H5P.DEFAULT,
                hnd.AddrOfPinnedObject());
            hnd.Free();
            H5D.close(datasetId);
            H5S.close(spaceId);
            H5T.close(typeId);
            return (result, datasetId);
        }

        /// <summary>
        /// Appends a dataset to a hdf5 file. If called the first time a dataset is created
        /// </summary>
        /// <typeparam name="T">Generic parameter only primitive types are allowed</typeparam>
        /// <param name="groupId">id of the group. Can also be a file Id</param>
        /// <param name="name">name of the dataset</param>
        /// <param name="dset">The dataset</param>
        /// <returns>status of the write method</returns>
        public static long AppendDataset<T>(long groupId, string name, Array dset, ulong chunkX = 200) where T : struct
        {
            var rank = dset.Rank;
            ulong[] dimsExtend = Enumerable.Range(0, rank).Select(i =>
            { return (ulong)dset.GetLength(i); }).ToArray();
            ulong[] maxDimsExtend = null;
            ulong[] dimsChunk = new[] { chunkX }.Concat(dimsExtend.Skip(1)).ToArray();
            ulong[] zeros = Enumerable.Range(0, rank).Select(z => (ulong)0).ToArray();
            long status, spaceId, datasetId;

            // name = ToHdf5Name(name);
            var datatype = GetDatatype(typeof(T));
            var typeId = H5T.copy(datatype);
            var datasetExists = H5L.exists(groupId, Hdf5Utils.NormalizedName(name)) > 0;

            /* Create a new dataset within the file using chunk 
               creation properties.  */
            if (!datasetExists)
            {
                spaceId = H5S.create_simple(dset.Rank, dimsExtend, maxDimsExtend);
                datasetId = Hdf5Utils.GetDatasetId(groupId, Hdf5Utils.NormalizedName(name), typeId, spaceId, H5P.DEFAULT);

                var propId = H5P.create(H5P.DATASET_CREATE);
                status = H5P.set_chunk(propId, rank, dimsChunk);

                /* Write data to dataset */
                GCHandle hnd = GCHandle.Alloc(dset, GCHandleType.Pinned);
                status = H5D.write(datasetId, datatype, H5S.ALL, H5S.ALL, H5P.DEFAULT,
                    hnd.AddrOfPinnedObject());
                hnd.Free();
                H5P.close(propId);
            }
            else
            {
                datasetId = H5D.open(groupId, Hdf5Utils.NormalizedName(name));
                spaceId = H5D.get_space(datasetId);
                var rank_old = H5S.get_simple_extent_ndims(spaceId);
                ulong[] maxDims = new ulong[rank_old];
                ulong[] dims = new ulong[rank_old];
                var memId1 = H5S.get_simple_extent_dims(spaceId, dims, maxDims);

                ulong[] oldChunk = null;
                int chunkDims = 0;
                var propId = H5P.create(H5P.DATASET_ACCESS);
                status = H5P.get_chunk(propId, chunkDims, oldChunk);

                /* Extend the dataset. */
                var size = new[] { dims[0] + dimsExtend[0] }.Concat(dims.Skip(1)).ToArray();
                status = H5D.set_extent(datasetId, size);

                /* Select a hyperslab in extended portion of dataset  */
                var filespaceId = H5D.get_space(datasetId);
                var offset = new[] { dims[0] }.Concat(zeros.Skip(1)).ToArray();
                status = H5S.select_hyperslab(filespaceId, H5S.seloper_t.SET, offset, null,
                                              dimsExtend, null);

                /* Define memory space */
                var memId2 = H5S.create_simple(rank, dimsExtend, null);

                /* Write the data to the extended portion of dataset  */
                GCHandle hnd = GCHandle.Alloc(dset, GCHandleType.Pinned);
                status = H5D.write(datasetId, datatype, memId2, spaceId,
                                   H5P.DEFAULT, hnd.AddrOfPinnedObject());
                hnd.Free();
                H5S.close(memId1);
                H5S.close(memId2);
                H5P.close(propId);
                H5D.close(filespaceId);
            }

            //todo: close?
            H5T.close(datatype);
            H5D.close(datasetId);
            H5S.close(spaceId);
            return status;
        }
    }
}