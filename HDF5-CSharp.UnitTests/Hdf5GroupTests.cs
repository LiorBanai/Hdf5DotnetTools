using HDF.PInvoke;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace HDF5CSharp.UnitTests
{
    public partial class Hdf5UnitTests
    {
        [TestMethod]
        public void WriteAndReadGroupsWithDataset()
        {
            string filename = Path.Combine(folder, "testGroups.H5");

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var dset = dsets.First();

                var groupId = H5G.create(fileId, Hdf5Utils.NormalizedName("/A")); ///B/C/D/E/F/G/H
                Hdf5.WriteDataset(groupId, Hdf5Utils.NormalizedName("test"), dset);
                var subGroupId = Hdf5.CreateOrOpenGroup(groupId, Hdf5Utils.NormalizedName("C"));
                var subGroupId2 = Hdf5.CreateOrOpenGroup(groupId, Hdf5Utils.NormalizedName("/D")); // will be saved at the root location 
                dset = dsets.Skip(1).First();
                Hdf5.WriteDataset(subGroupId, Hdf5Utils.NormalizedName("test2"), dset);
                Hdf5.CloseGroup(subGroupId);
                Hdf5.CloseGroup(subGroupId2);
                Hdf5.CloseGroup(groupId);
                groupId = H5G.create(fileId, Hdf5Utils.NormalizedName("/A/B")); ///B/C/D/E/F/G/H
                dset = dsets.Skip(1).First();
                Hdf5.WriteDataset(groupId, Hdf5Utils.NormalizedName("test"), dset);
                Hdf5.CloseGroup(groupId);

                groupId = Hdf5.CreateGroupRecursively(fileId, Hdf5Utils.NormalizedName("A/B/C/D/E/F/I"));
                Hdf5.CloseGroup(groupId);
                Hdf5.CloseFile(fileId);

                fileId = Hdf5.OpenFile(filename);
                Assert.IsTrue(fileId > 0);
                fileId = Hdf5.OpenFile(filename);

                groupId = H5G.open(fileId, Hdf5Utils.NormalizedName("/A/B"));
                double[,] dset2 = (double[,])Hdf5.ReadDataset<double>(groupId, Hdf5Utils.NormalizedName("test")).Result;
                CompareDatasets(dset, dset2);
                Assert.IsTrue(Hdf5.CloseGroup(groupId) >= 0);
                groupId = H5G.open(fileId, Hdf5Utils.NormalizedName("/A/C"));
                dset2 = (double[,])Hdf5.ReadDataset<double>(groupId, Hdf5Utils.NormalizedName("test2")).Result;
                CompareDatasets(dset, dset2);
                Assert.IsTrue(Hdf5.CloseGroup(groupId) >= 0);
                bool same = dset == dset2;
                dset = dsets.First();
                dset2 = (double[,])Hdf5.ReadDataset<double>(fileId, Hdf5Utils.NormalizedName("/A/test")).Result;
                CompareDatasets(dset, dset2);
                Assert.IsTrue(Hdf5Utils.ItemExists(fileId, Hdf5Utils.NormalizedName("A/B/C/D/E/F/I"), DataTypes.Hdf5ElementType.Dataset));

                Assert.IsTrue(Hdf5.CloseFile(fileId) == 0);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }
    }
}