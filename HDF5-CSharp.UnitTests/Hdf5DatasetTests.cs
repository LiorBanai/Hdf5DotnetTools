using HDF.PInvoke;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HDF5CSharp.UnitTests
{
    public partial class Hdf5UnitTests
    {
        [TestMethod]
        public void WriteAndReadDatetimeDataset()
        {
            string filename = Path.Combine(folder, "testDatetime.H5");
            var times = new DateTime[10, 5];
            var offset = new DateTime(2000, 1, 1, 12, 0, 0);
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 5; j++)
                {
                    times[i, j] = offset.AddDays(i + j * 5);
                }

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                Hdf5.WriteDataset(fileId, "/test", times);

                var timesRead = (DateTime[,])Hdf5.ReadDataset<DateTime>(fileId, "/test").Result;
                CompareDatasets(times, timesRead);

                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadTimespanDataset()
        {
            string filename = Path.Combine(folder, "testTimespan.H5");
            var times = new TimeSpan[10, 5];
            var offset = new TimeSpan(1, 0, 0, 0, 0);
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 5; j++)
                {
                    times[i, j] = offset.Add(new TimeSpan(i + j * 5, 0, 0));
                }

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                Hdf5.WriteDataset(fileId, "/test", times);

                TimeSpan[,] timesRead = (TimeSpan[,])Hdf5.ReadDataset<TimeSpan>(fileId, "/test").Result;
                CompareDatasets(times, timesRead);

                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadDataset()
        {
            string filename = Path.Combine(folder, "testDataset.H5");
            var dset = dsets.First();

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                Hdf5.WriteDataset(fileId, "/test", dset);
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }

            try
            {
                var fileId = Hdf5.OpenFile(filename);
                Assert.IsTrue(fileId > 0);
                double[,] dset2 = (double[,])Hdf5.ReadDataset<double>(fileId, "/test").Result;
                CompareDatasets(dset, dset2);
                bool same = dset == dset2;

                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadPrimitives()
        {
            string filename = Path.Combine(folder, "testPrimitives.H5");
            int intValue = 2;
            double dblValue = 1.1;
            string strValue = "test";
            bool boolValue = true;
            var groupStr = "/test";
            string concatFunc(string x) => string.Concat(groupStr, "/", x);
            Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var groupId = Hdf5.CreateOrOpenGroup(fileId, groupStr);
                Hdf5.WriteOneValue(groupId, concatFunc(nameof(intValue)), intValue, attributes);
                Hdf5.WriteOneValue(groupId, concatFunc(nameof(dblValue)), dblValue, attributes);
                Hdf5.WriteOneValue(groupId, concatFunc(nameof(strValue)), strValue, attributes);
                Hdf5.WriteOneValue(groupId, concatFunc(nameof(boolValue)), boolValue, attributes);
                Hdf5.CloseGroup(groupId);
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }

            try
            {
                var fileId = Hdf5.OpenFile(filename);
                Assert.IsTrue(fileId > 0);
                var groupId = H5G.open(fileId, groupStr);
                int readInt = Hdf5.ReadOneValue<int>(groupId, concatFunc(nameof(intValue)));
                Assert.IsTrue(intValue == readInt);
                double readDbl = Hdf5.ReadOneValue<double>(groupId, concatFunc(nameof(dblValue)));
                Assert.IsTrue(dblValue == readDbl);
                string readStr = Hdf5.ReadOneValue<string>(groupId, concatFunc(nameof(strValue)));
                Assert.IsTrue(strValue == readStr);
                bool readBool = Hdf5.ReadOneValue<bool>(groupId, concatFunc(nameof(boolValue)));
                Assert.IsTrue(boolValue == readBool);
                H5G.close(groupId);
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadAllPrimitives()
        {
            string filename = Path.Combine(folder, "testAllPrimitives.H5");

            //var groupStr = "/test";
            //string concatFunc(string x) => string.Concat(groupStr, "/", x);
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                Hdf5.WriteObject(fileId, allTypesObject, "/test");

                var readObject = Hdf5.ReadObject<AllTypesClass>(fileId, "/test");
                Assert.IsTrue(allTypesObject.PublicInstanceFieldsEqual(readObject));
                Assert.IsTrue(Hdf5.CloseFile(fileId) == 0);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAppendAndReadChunckedDataset()
        {
            string filename = Path.Combine(folder, "testChunks.H5");
            string groupName = "/test";
            string datasetName = "Data";

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var groupId = Hdf5.CreateOrOpenGroup(fileId, groupName);
                Assert.IsTrue(groupId >= 0);

                //var chunkSize = new ulong[] { 5, 5 };
                using (var chunkedDset = new ChunkedDataset<double>(datasetName, groupId, dsets.First()))
                {
                    foreach (var ds in dsets.Skip(1))
                    {
                        chunkedDset.AppendDataset(ds);
                    }
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

                //var groupId = H5G.open(fileId, groupName);
                //var dset = Hdf5.ReadDatasetToArray<double>(groupId, datasetName);
                var dset = Hdf5.ReadDatasetToArray<double>(fileId, string.Concat(groupName, "/", datasetName));

                Assert.IsTrue(dset.Result.Rank == dsets.First().Rank);
                var xSum = dsets.Select(d => d.GetLength(0)).Sum();
                Assert.IsTrue(xSum == dset.Result.GetLength(0));
                var testRange = Enumerable.Range(0, 30).Select(t => (double)t);

                // get every 5th element in the matrix
                var x0Range = dset.Result.Cast<double>().Where((d, i) => i % 5 == 0);
                Assert.IsTrue(testRange.SequenceEqual(x0Range));

                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAppendFlushAndReadChunckedDataset()
        {
            string filename = Path.Combine(folder, "testChunksAndFlush.H5");
            string groupName = "/test";
            string datasetName = "Data";

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var groupId = Hdf5.CreateOrOpenGroup(fileId, groupName);
                Assert.IsTrue(groupId >= 0);

                //var chunkSize = new ulong[] { 5, 5 };
                using (var chunkedDset = new ChunkedDataset<double>(datasetName, groupId, dsets.First()))
                {
                    foreach (var ds in dsets.Skip(1))
                    {
                        chunkedDset.AppendDataset(ds);
                        chunkedDset.Flush();
                        var dsetRead = Hdf5.ReadDatasetToArray<double>(fileId, string.Concat(groupName, "/", datasetName));
                    }
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

                //var groupId = H5G.open(fileId, groupName);
                //var dset = Hdf5.ReadDatasetToArray<double>(groupId, datasetName);
                var dset = Hdf5.ReadDatasetToArray<double>(fileId, string.Concat(groupName, "/", datasetName));

                Assert.IsTrue(dset.Result.Rank == dsets.First().Rank);
                var xSum = dsets.Select(d => d.GetLength(0)).Sum();
                Assert.IsTrue(xSum == dset.Result.GetLength(0));
                var testRange = Enumerable.Range(0, 30).Select(t => (double)t);

                // get every 5th element in the matrix
                var x0Range = dset.Result.Cast<double>().Where((d, i) => i % 5 == 0);
                Assert.IsTrue(testRange.SequenceEqual(x0Range));

                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }
        [TestMethod]
        public void WriteAndOverrideChunkedDataset()
        {
            var data = CreateDataset();
            string filename = Path.Combine(folder, "testOverrideChunks.H5");
            string groupName = "/test";
            string datasetName = "Data";

            //create
            var fileId = Hdf5.CreateFile(filename);
            Assert.IsTrue(fileId > 0);
            var groupId = Hdf5.CreateOrOpenGroup(fileId, groupName);
            Assert.IsTrue(groupId >= 0);
            using (var chunkedDset = new ChunkedDataset<double>(datasetName, groupId))
            {
                chunkedDset.AppendOrCreateDataset(data);
            }

            Hdf5.CloseFile(fileId);

            //read
            fileId = Hdf5.OpenFile(filename);

            var dataRead = Hdf5.ReadDatasetToArray<double>(fileId, string.Concat(groupName, "/", datasetName));
            Hdf5.CloseFile(fileId);
            CompareDatasets(dataRead.Result as double[,], data);
        }

        [TestMethod]
        public void WriteAndReadSubsetOfDataset()
        {
            string filename = Path.Combine(folder, "testSubset.H5");
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var chunkSize = new ulong[] { 5, 5 };
                using (var chunkedDset = new ChunkedDataset<double>("/test", fileId, dsets.First()))
                {
                    foreach (var ds in dsets.Skip(1))
                    {
                        chunkedDset.AppendDataset(ds);
                    }
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
                ulong begIndex = 8;
                ulong endIndex = 21;
                var dset = Hdf5.ReadDataset<double>(fileId, "/test", begIndex, endIndex);
                Hdf5.CloseFile(fileId);

                Assert.IsTrue(dset.Rank == dsets.First().Rank);
                int count = Convert.ToInt32(endIndex - begIndex + 1);
                Assert.IsTrue(count == dset.GetLength(0));

                // Creat a range from number 8 to 21
                var testRange = Enumerable.Range((int)begIndex, count).Select(t => (double)t);

                // Get the first column from row index number 8 (the 9th row) to row index number 21 (22th row) 
                var x0Range = dset.Cast<double>().Where((d, i) => i % 5 == 0);
                Assert.IsTrue(testRange.SequenceEqual(x0Range));
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadChunckedDataset2()
        {
            string filename = Path.Combine(folder, "testChunks2.H5");
            string groupName = "/test";
            string datasetName = "Data";

            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var groupId = Hdf5.CreateOrOpenGroup(fileId, groupName);
                Assert.IsTrue(groupId >= 0);

                //var chunkSize = new ulong[] { 5, 5 };
                using (var chunkedDset = new ChunkedDataset<double>(datasetName, groupId))
                {
                    foreach (var ds in dsets)
                    {
                        chunkedDset.AppendOrCreateDataset(ds);
                    }
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

                //var groupId = H5G.open(fileId, groupName);
                //var dset = Hdf5.ReadDatasetToArray<double>(groupId, datasetName);
                var dset = Hdf5.ReadDatasetToArray<double>(fileId, string.Concat(groupName, "/", datasetName));

                Assert.IsTrue(dset.Result.Rank == dsets.First().Rank);
                var xSum = dsets.Select(d => d.GetLength(0)).Sum();
                Assert.IsTrue(xSum == dset.Result.GetLength(0));
                var testRange = Enumerable.Range(0, 30).Select(t => (double)t);

                // get every 5th element in the matrix
                var x0Range = dset.Result.Cast<double>().Where((d, i) => i % 5 == 0);
                Assert.IsTrue(testRange.SequenceEqual(x0Range));

                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        private long WriteDataset(string filename)
        {
            long tef2 = Hdf5.CreateFile(filename);
            int[] blah = { 1, 2, 4, 5, 0 };
            Hdf5.WriteDatasetFromArray<int>(tef2, "blah", blah);
            Hdf5.CloseFile(tef2);
            var what = "???"; // breakpoint in VS to test h5 file contents independently before next write step
            tef2 = Hdf5.OpenFile(filename);
            blah[4] = 6;
            Hdf5.WriteDatasetFromArray<int>(tef2, "blah", blah); // This command throws several debug errors from PInvoke
            var (success, result) = Hdf5.ReadDataset<int>(tef2, "blah");
            Assert.IsTrue(success);
            Assert.IsTrue(result.Cast<int>().SequenceEqual(blah));

            // loading the hdf5 file shows it only has {1, 2, 4, 5, 0} stored.
            return tef2;
        }
        [TestMethod]
        public void WriteAndUpdateDataset()
        {
            string filename = "writeandupdatedatset.h5";
            long tef2 = WriteDataset(filename);
            Hdf5.CloseFile(tef2);
            File.Delete(filename);
        }

        [TestMethod]
        public void OverrideDataset()
        {
            string filename1 = "overridedataset1.h5";
            long id = WriteDataset(filename1);
            FileInfo fi = new FileInfo(filename1);
            var l1 = fi.Length;
            Hdf5.CloseFile(id);
            File.Delete(filename1);
            string filename = "overridedataset.h5";
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5.Settings.OverrideExistingData = true;
            long tef2 = Hdf5.CreateFile(filename);
            int[] blah = { 1, 2, 4, 5, 0 };
            Hdf5.WriteDatasetFromArray<int>(tef2, "blah", blah);
            Hdf5.CloseFile(tef2);
            var what = "???"; // breakpoint in VS to test h5 file contents independently before next write step
            tef2 = Hdf5.OpenFile(filename);
            for (int i = 0; i < 10; i++)
            {
                blah[4] = i + i;
                Hdf5.WriteDatasetFromArray<int>(tef2, "blah", blah);
            }

            var (success, result) = Hdf5.ReadDataset<int>(tef2, "blah");
            Assert.IsTrue(success);
            Assert.IsTrue(result.Cast<int>().SequenceEqual(blah));
            FileInfo fi2 = new FileInfo(filename);
            var l2 = fi.Length;
            Hdf5.CloseFile(tef2);
            File.Delete(filename);
            Assert.IsTrue(l1 == l2);
        }
        [TestMethod]
        public void OverrideAndIncreaseDataset()
        {
            string filename = "overrideandincreasedataset.h5";
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5.Settings.OverrideExistingData = true;
            long id = Hdf5.CreateFile(filename);
            int[] d1 = { 1, 2, 3, 4, 5 };
            int[] d2 = { 11, 12, 13, 14, 15 };
            Hdf5.WriteDatasetFromArray<int>(id, "d1", d1);
            Hdf5.WriteDatasetFromArray<int>(id, "d2", d2);
            Hdf5.CloseFile(id);
            id = Hdf5.OpenFile(filename);
            var (success, result) = Hdf5.ReadDataset<int>(id, "d1");
            var (success1, result2) = Hdf5.ReadDataset<int>(id, "d2");
            Hdf5.CloseFile(id);

            int[] d3 = { 21, 22, 24, 25, 26, 27, 28, 29, 210 };
            Assert.IsTrue(success);
            Assert.IsTrue(result.Cast<int>().SequenceEqual(d1));
            Assert.IsTrue(success1);
            Assert.IsTrue(result2.Cast<int>().SequenceEqual(d2));

            id = Hdf5.OpenFile(filename);
            Hdf5.WriteDatasetFromArray<int>(id, "d1", d3);
            Hdf5.CloseFile(id);
            id = Hdf5.OpenFile(filename);
            var (success3, result3) = Hdf5.ReadDataset<int>(id, "d1");
            var (success4, result4) = Hdf5.ReadDataset<int>(id, "d2");
            Hdf5.CloseFile(id);
            Assert.IsTrue(success3);
            Assert.IsTrue(result3.Cast<int>().SequenceEqual(d3));
            Assert.IsTrue(success4);
            Assert.IsTrue(result4.Cast<int>().SequenceEqual(d2));
        }
    }
}