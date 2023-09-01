using HDF.PInvoke;
using HDF5CSharp.DataTypes;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace HDF5CSharp;

public class ChunkedDataset<T> : IDisposable where T : struct
{
    ulong[] _currentDims, _oldDims;
    readonly ulong[] _maxDims = { H5S.UNLIMITED, H5S.UNLIMITED };
    private ulong[] _chunkDims;
    long _status, _spaceId, _datasetId, _propId;
    readonly long _typeId, _datatype;

    public string Datasetname { get; private set; }
    public int Rank { get; private set; }
    public long GroupId { get; private set; }
    /// <summary>
    /// Constructor to create a chuncked dataset object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="groupId"></param>
    public ChunkedDataset(string name, long groupId)
    {
        Datasetname = name;
        GroupId = groupId;
        _datatype = Hdf5.GetDatatype(typeof(T));
        _typeId = H5T.copy(_datatype);
        _chunkDims = null;
    }
    /// <summary>
    /// Constructor to create a chuncked dataset object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="groupId"></param>
    /// <param name="chunkSize"></param>
    public ChunkedDataset(string name, long groupId, ulong[] chunkSize)
    {
        Datasetname = name;
        GroupId = groupId;
        _datatype = Hdf5.GetDatatype(typeof(T));
        _typeId = H5T.copy(_datatype);
        _chunkDims = chunkSize;
    }

    /// <summary>
    /// Constructor to create a chuncked dataset object with an initial dataset. 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="groupId"></param>
    /// <param name="dataset"></param>
    public ChunkedDataset(string name, long groupId, T[,] dataset) : this(name, groupId, new[] { Convert.ToUInt64(dataset.GetLongLength(0)), Convert.ToUInt64(dataset.GetLongLength(1)) })
    {
        FirstDataset(dataset);
    }

    public void FirstDataset(Array dataset)
    {
        if (GroupId <= 0)
        {
            throw new Hdf5Exception("cannot call FirstDataset because group or file couldn't be created");
        }

        if (Hdf5Utils.GetRealName(GroupId, Datasetname, string.Empty).valid)
        {
            throw new Hdf5Exception("cannot call FirstDataset because dataset already exists");
        }

        Rank = dataset.Rank;
        _currentDims = GetDims(dataset);

        /* Create the data space with unlimited dimensions. */
        _spaceId = H5S.create_simple(Rank, _currentDims, _maxDims);

        /* Modify dataset creation properties, i.e. enable chunking  */
        _propId = H5P.create(H5P.DATASET_CREATE);
        _status = H5P.set_chunk(_propId, Rank, _chunkDims);

        /* Create a new dataset within the file using chunk creation properties.  */
        _datasetId = Hdf5Utils.GetDatasetId(GroupId, Hdf5Utils.NormalizedName(Datasetname), _datatype, _spaceId,_propId);

        /* Write data to dataset */
        GCHandle hnd = GCHandle.Alloc(dataset, GCHandleType.Pinned);
        _status = H5D.write(_datasetId, _datatype, H5S.ALL, H5S.ALL, H5P.DEFAULT,
            hnd.AddrOfPinnedObject());
        if (_status < 0)
        {
            Hdf5Utils.LogMessage("Unable to write Dataset", Hdf5LogLevel.Error);
        }

        hnd.Free();
        H5S.close(_spaceId);
        _spaceId = -1;
    }

    public void AppendOrCreateDataset(Array dataset)
    {
        if (_chunkDims == null)
        {
            if (dataset.Rank < 1)
            {
                string msg = "Empty array was passed. Ignoring.";
                Hdf5Utils.LogMessage(msg, Hdf5LogLevel.Error);
                return;
            }

            for (int dimension = 1; dimension <= dataset.Rank; dimension++)
            {
                var size = dataset.GetUpperBound(dimension - 1) + 1;
                if (size == 0)
                {
                    string msg = $"Empty array was passed for dimension {dimension}. Ignoring.";
                    Hdf5Utils.LogMessage(msg, Hdf5LogLevel.Error);
                    return;
                }
            }
            _chunkDims = new[]
                {Convert.ToUInt64(dataset.GetLongLength(0)), Convert.ToUInt64(dataset.GetLongLength(1))};

            Rank = dataset.Rank;
            _currentDims = GetDims(dataset);

            /* Create the data space with unlimited dimensions. */
            _spaceId = H5S.create_simple(Rank, _currentDims, _maxDims);

            /* Modify dataset creation properties, i.e. enable chunking  */
            _propId = H5P.create(H5P.DATASET_CREATE);
            _status = H5P.set_chunk(_propId, Rank, _chunkDims);

            /* Create a new dataset within the file using chunk creation properties.  */
            _datasetId = Hdf5Utils.GetDatasetId(GroupId, Hdf5Utils.NormalizedName(Datasetname), _datatype, _spaceId,_propId);

            /* Write data to dataset */
            GCHandle hnd = GCHandle.Alloc(dataset, GCHandleType.Pinned);
            _status = H5D.write(_datasetId, _datatype, H5S.ALL, H5S.ALL, H5P.DEFAULT,
                hnd.AddrOfPinnedObject());
            hnd.Free();
            H5S.close(_spaceId);
            _spaceId = -1;
        }
        else
        {
            AppendDataset(dataset);
        }
    }
    public void AppendDataset(Array dataset)
    {
        if (!Hdf5Utils.GetRealName(GroupId, Datasetname, string.Empty).valid)
        {
            string msg = "call constructor or FirstDataset first before appending.";
            Hdf5Utils.LogMessage(msg, Hdf5LogLevel.Error);
            if (Hdf5.Settings.ThrowOnError)
            {
                throw new Hdf5Exception(msg);
            }
        }
        _oldDims = _currentDims;
        _currentDims = GetDims(dataset);
        int rank = dataset.Rank;
        ulong[] zeros = Enumerable.Range(0, rank).Select(z => (ulong)0).ToArray();

        /* Extend the dataset. Dataset becomes 10 x 3  */
        var size = new[] { _oldDims[0] + _currentDims[0] }.Concat(_oldDims.Skip(1)).ToArray();

        _status = H5D.set_extent(_datasetId, size);
        ulong[] offset = new[] { _oldDims[0] }.Concat(zeros.Skip(1)).ToArray();

        /* Select a hyperslab in extended portion of dataset  */
        var filespaceId = H5D.get_space(_datasetId);
        _status = H5S.select_hyperslab(filespaceId, H5S.seloper_t.SET, offset, null,
                                      _currentDims, null);

        /* Define memory space */
        var memId = H5S.create_simple(Rank, _currentDims, null);

        /* Write the data to the extended portion of dataset  */
        GCHandle hnd = GCHandle.Alloc(dataset, GCHandleType.Pinned);
        _status = H5D.write(_datasetId, _datatype, memId, filespaceId,
                           H5P.DEFAULT, hnd.AddrOfPinnedObject());
        hnd.Free();

        _currentDims = size;
        H5S.close(memId);
        H5S.close(filespaceId);
    }

    public void Flush()
    {
        try
        {
            H5D.flush(_datasetId);
        }
        catch (Exception e)
        {
            Hdf5Utils.LogMessage($"Unable to flash dataset: {e}", Hdf5LogLevel.Error);
        }

    }
    /// <summary>
    /// Finalizer of object
    /// </summary>
    ~ChunkedDataset()
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
        if (!Hdf5Utils.GetRealName(GroupId, Datasetname, string.Empty).valid)
        {
            Hdf5Utils.LogMessage($"Dataset {Datasetname} does not exist.", Hdf5LogLevel.Warning);
            return;
        }

        if (_datasetId >= 0)
        {
            H5D.close(_datasetId);
        }

        if (_propId >= 0)
        {
            H5P.close(_propId);
        }

        if (_spaceId >= 0)
        {
            H5S.close(_spaceId);
        }

        if (itIsSafeToAlsoFreeManagedObjects)
        {

        }
    }

    private ulong[] GetDims(Array dset)
    {
        return Enumerable.Range(0, dset.Rank).Select(i =>
        { return (ulong)dset.GetLength(i); }).ToArray();
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
