using HDF5CSharp.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HDF5CSharp.UnitTests
{
    [TestClass]
    public class FilesUnitTests : Hdf5BaseUnitTests
    {
        private static List<string> Errors { get; set; }
        private static string folder;

        public FilesUnitTests()
        {
            Errors = new List<string>();
            folder = AppDomain.CurrentDomain.BaseDirectory;
        }
        [TestMethod]
        public void TestReadStructure()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5Utils.LogWarning = (s) => Errors.Add(s);
            Hdf5Utils.LogError = (s) => Errors.Add(s);
            string fileName = @"C:\Users\liorb\Downloads\hdf5_test.h5";
            if (File.Exists(fileName))
            {
                var tree = Hdf5.ReadTreeFileStructure(fileName);
                var flat = Hdf5.ReadFlatFileStructure(fileName);
                File.Delete(fileName);
                if (Errors.Any())
                {
                    foreach (string error in Errors)
                    {
                        Console.WriteLine(error);
                    }
                }
                Assert.IsFalse(File.Exists(fileName));
                Assert.IsTrue(tree != null);
                Assert.IsTrue(flat != null);
            }
        }

        [TestMethod]
        public void TestInnerPathNotExist()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(false);
            Hdf5Utils.LogError = (s) => Errors.Add(s);
            string fileName = Path.Combine(folder, "files", "testFile.H5");
            var fileId = Hdf5.OpenFile(fileName, true);
            var result = Hdf5Utils.ItemExists(fileId, "/A/B/C",Hdf5ElementType.Dataset);
            Assert.IsFalse(result);
            var id = Hdf5.OpenDatasetIfExists(fileId, "/A/B/C","");
            Assert.IsTrue(id==-1);
        }


        //[TestMethod]
        public void TestLoops()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5Utils.LogError = (s) => Errors.Add(s);
            string fileName = Path.Combine(folder, "files", "loop.H5");
            long fileId = -1;
            bool readOK = true;
            Dictionary<string, TabularData<double>> data = new Dictionary<string, TabularData<double>>();
            fileId = Hdf5.OpenFile(fileName, true);
            var groupId = Hdf5.CreateOrOpenGroup(fileId, "/MODEL_STAGE[1]/RESULTS/ON_NODES/DISPLACEMENT/DATA/");
            int step = 0;
            do
            {
                string name = $"STEP_{step++}";
                TabularData<double> disp = Hdf5.Read2DTable<double>(groupId, name);
                if (disp.Data != null)
                {
                    data.Add(name, disp);
                }
                else
                {
                    readOK = false;
                }
            } while (readOK);

            Hdf5.CloseGroup(groupId);
            Assert.IsTrue(data.Count == 10,$"Actual: {data.Count}");
            Hdf5.CloseFile(fileId);
            File.Delete(fileName);
        }
    }
}