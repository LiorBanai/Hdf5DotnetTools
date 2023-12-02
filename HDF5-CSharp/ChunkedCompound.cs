using HDF.PInvoke;
using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace HDF5CSharp
{
    public class ChunkedCompound<T> : IDisposable where T : struct
    {
        private ulong[] currentDims;
        private readonly ulong[] maxDims = { H5S.UNLIMITED, H5S.UNLIMITED };
        private long status;
        private long spaceId;
        private long datasetId;
        private long propId;
        private readonly long typeId;
        private readonly long datatype;
        public string GroupName { get; private set; }
        public int Rank { get; private set; }
        public long GroupId { get; private set; }

        /// <summary>
        /// Constructor to create a chuncked Compound object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupId"></param>
        public ChunkedCompound(string name, long groupId)
        {
            GroupName = name;
            GroupId = groupId;
            datatype = Hdf5.CreateType(typeof(T));
            typeId = H5T.copy(datatype);
        }

        /// <summary>
        /// Constructor to create a chuncked Compound object with an initial Compound. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupId"></param>
        /// <param name="items"></param>
        public ChunkedCompound(string name, long groupId, IEnumerable<T> items) : this(name, groupId)
        {
            FirstCompound(items);
        }

        private void FirstCompound(IEnumerable<T> items)
        {
            if (GroupId <= 0)
            {
                throw new Hdf5Exception("cannot call FirstDataset because group or file couldn't be created");
            }

            if (Hdf5Utils.GetRealName(GroupId, GroupName, string.Empty).Valid)
            {
                throw new Hdf5Exception("cannot call FirstDataset because dataset already exists");
            }

            if (items == null || !items.Any())
            {
                throw new Hdf5Exception("empty items");
            }
            Type type = typeof(T);
            var count = (ulong)items.LongCount();

            var typeId = Hdf5.CreateType(type);
            var log10 = (int)Math.Log10(count);
            ulong pow = (ulong)Math.Pow(10, log10);
            ulong c_s = Math.Min(1000, pow);
            ulong[] chunk_size = { c_s };
            ulong[] dims = { count };
            long dcpl = 0;

            dcpl = Hdf5.CreateProperty(chunk_size);

            // Create dataspace.  Setting maximum size to NULL sets the maximum
            // size to be the current size.
            spaceId = H5S.create_simple(dims.Length, dims, maxDims);

            // Create the dataset and write the compound data to it.
            datasetId = Hdf5Utils.GetDatasetId(GroupId, Hdf5Utils.NormalizedName(GroupName), typeId, spaceId, dcpl);

            var ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            foreach (var strct in items)
            {
                writer.Write(Hdf5.GetBytes(strct));
            }

            var bytes = ms.ToArray();

            GCHandle hnd = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var statusId = H5D.write(datasetId, typeId, spaceId, H5S.ALL,
                H5P.DEFAULT, hnd.AddrOfPinnedObject());
            currentDims = new[] { (ulong)items.LongCount(), (ulong)1 };
            if (statusId < 0)
            {
                Hdf5Utils.LogMessage("Error creating compound", Hdf5LogLevel.Error);
            }
            /*
             * Close and release resources.
             */

            hnd.Free();
            H5S.close(spaceId);
            spaceId = -1;
        }

        public void AppendOrCreateCompound(IEnumerable<T> items)
        {
            if (items == null || !items.Any())
            {
                Hdf5Utils.LogMessage($"Empty list in {nameof(AppendOrCreateCompound)}", Hdf5LogLevel.Warning);
                return;
            }
            if (currentDims == null)
            {
                if (items.LongCount() < 1)
                {
                    string msg = "Empty array was passed. Ignoring.";
                    Hdf5Utils.LogMessage(msg, Hdf5LogLevel.Error);
                    return;
                }
                FirstCompound(items);
            }
            else
            {
                AppendCompound(items);
            }
        }
        public void AppendCompound(IEnumerable<T> list)
        {
            if (list == null || !list.Any())
            {
                Hdf5Utils.LogMessage($"Empty list in {nameof(AppendCompound)}", Hdf5LogLevel.Warning);
                return;
            }
            if (!Hdf5Utils.GetRealName(GroupId, GroupName, string.Empty).Valid)
            {
                string msg = $"call constructor or {nameof(FirstCompound)} first before appending.";
                Hdf5Utils.LogMessage(msg, Hdf5LogLevel.Error);
                if (Hdf5.Settings.ThrowOnError)
                {
                    throw new Hdf5Exception(msg);
                }
            }

            var _oldDims = this.currentDims.ToArray();
            var _ListDims = new ulong[] { (ulong)list.LongCount(), 1 };
            ulong[] zeros = Enumerable.Range(0, 2).Select(z => (ulong)0).ToArray();

            /* Extend the dataset. Dataset becomes 10 x 3  */
            var size = new ulong[] { _oldDims[0] + _ListDims[0], 1 };

            var _status = H5D.set_extent(datasetId, size);
            ulong[] offset = new[] { _oldDims[0] }.Concat(zeros.Skip(1)).ToArray();

            /* Select a hyperslab in extended portion of dataset  */
            var filespaceId = H5D.get_space(datasetId);
            if (filespaceId < 0)
            {
                string msg = $"error creating file space.";
                Hdf5Utils.LogMessage(msg, Hdf5LogLevel.Error);
                if (Hdf5.Settings.ThrowOnError)
                {
                    throw new Hdf5Exception(msg);
                }
            }
            _status = H5S.select_hyperslab(filespaceId, H5S.seloper_t.SET, offset, null, _ListDims, null);
            if (_status < 0)
            {
                string msg = $"error creating hyperslab.";
                Hdf5Utils.LogMessage(msg, Hdf5LogLevel.Error);
                if (Hdf5.Settings.ThrowOnError)
                {
                    throw new Hdf5Exception(msg);
                }
            }
            /* Define memory space */
            var memId = H5S.create_simple(2, _ListDims, null);
            var ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            foreach (var strct in list)
            {
                writer.Write(Hdf5.GetBytes(strct));
            }
            var bytes = ms.ToArray();
            currentDims = size;
            /* Write the data to the extended portion of dataset  */
            GCHandle hnd = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            _status = H5D.write(datasetId, datatype, memId, filespaceId,
                H5P.DEFAULT, hnd.AddrOfPinnedObject());

            hnd.Free();
            H5S.close(memId);
            H5S.close(filespaceId);
        }

        public void Flush()
        {
            try
            {
                H5D.flush(datasetId);
            }
            catch (Exception e)
            {
                Hdf5Utils.LogMessage($"Unable to flash {nameof(ChunkedCompound<T>)}: {e}", Hdf5LogLevel.Error);
            }
        }

        /// <summary>
        /// Finalizer of object
        /// </summary>
        ~ChunkedCompound()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose function as suggested in the stackoverflow discussion below
        /// See: http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface/538238#538238
        /// </summary>
        /// <param name="itIsSafeToAlsoFreeManagedObjects"></param>
        protected virtual void Dispose(bool itIsSafeToAlsoFreeManagedObjects)
        {
            if (!Hdf5Utils.GetRealName(GroupId, GroupName, string.Empty).Valid)
            {
                Hdf5Utils.LogMessage($"Dataset {GroupName} does not exist.", Hdf5LogLevel.Warning);
            }

            if (datasetId >= 0)
            {
                H5D.close(datasetId);
            }

            if (propId >= 0)
            {
                H5P.close(propId);
            }

            if (spaceId >= 0)
            {
                H5S.close(spaceId);
            }

            if (itIsSafeToAlsoFreeManagedObjects)
            {
            }
        }

        private ulong[] GetDims(Array dset)
        {
            return Enumerable.Range(0, dset.Rank).Select(i => (ulong)dset.GetLength(i)).ToArray();
        }

        /// <summary>
        /// Dispose function as suggested in the stackoverflow discussion below
        /// See: http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface/538238#538238
        /// </summary>
        public void Dispose()
        {
            Dispose(true); //I am calling you from Dispose, it's safe
            GC.SuppressFinalize(this); //Hey, GC: don't bother calling finalize later
        }
    }
}