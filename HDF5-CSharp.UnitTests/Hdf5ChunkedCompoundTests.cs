using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace HDF5CSharp.UnitTests
{
    public partial class Hdf5UnitTests
    {
        [TestMethod]
        public void CreateAndAppendChunkedCompoundTest()
        {
            string filename = Path.Combine(folder, $"{nameof(CreateAndAppendChunkedCompoundTest)}.H5");
            string groupName = "/test";
            string datasetName = "Data";

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var groupId = Hdf5.CreateOrOpenGroup(fileId, groupName);
                Assert.IsTrue(groupId >= 0);
                //var chunkSize = new ulong[] { 5, 5 };
                using (var chunkedDset = new ChunkedCompound<WData>(datasetName, groupId, wDataList.Take(2)))
                {
                    chunkedDset.AppendCompound(wDataList.Skip(2));
                }
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }

            try
            {
                var fileId = Hdf5.OpenFile(filename);
                var dset = Hdf5.ReadCompounds<WData>(fileId, string.Concat(groupName, "/", datasetName),"", true).ToList();

                Assert.IsTrue(dset.LongCount() == wDataList.LongLength);
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void AppendOrCreateCompoundTest()
        {
            string filename = Path.Combine(folder, $"{nameof(AppendOrCreateCompoundTest)}.H5");
            string groupName = "/test";
            string datasetName = "Data";

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var groupId = Hdf5.CreateOrOpenGroup(fileId, groupName);
                Assert.IsTrue(groupId >= 0);
                //var chunkSize = new ulong[] { 5, 5 };
                using (var chunkedDset = new ChunkedCompound<WData>(datasetName, groupId))
                {
                    chunkedDset.AppendOrCreateCompound(wDataList);
                }
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }

            try
            {
                var fileId = Hdf5.OpenFile(filename);
                var dset = Hdf5.ReadCompounds<WData>(fileId, string.Concat(groupName, "/", datasetName), "", true);
                Assert.IsTrue(dset.LongCount() == wDataList.LongLength);
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }
    }
}