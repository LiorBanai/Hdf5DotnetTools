using HDF5CSharp.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace HDF5CSharp.UnitTests
{
    public partial class Hdf5UnitTests
    {
        [TestMethod]
        public void WriteAndReadObjectWithPropertiesTest()
        {
            string filename = Path.Combine(folder, "testObjects.H5");
            try
            {
                testClass.TestInteger = 2;
                testClass.TestDouble = 1.1;
                testClass.TestBoolean = true;
                testClass.TestString = "test string";

                // 31-Oct-2003, 18:00 is  731885.75 in matlab
                testClass.TestTime = new DateTime(2003, 10, 31, 18, 0, 0);

                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);

                Hdf5.WriteObject(fileId, testClass, "objectWithProperties");
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

                TestClass readObject = new TestClass();
                readObject = Hdf5.ReadObject(fileId, readObject, "objectWithProperties");
                Assert.IsTrue(testClass.Equals(readObject));

                readObject = Hdf5.ReadObject<TestClass>(fileId, "objectWithProperties");
                Assert.IsTrue(testClass.Equals(readObject));

                Assert.IsTrue(Hdf5.CloseFile(fileId) >= 0);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadObjectWithPropertiesAndArrayPropertyTest()
        {
            try
            {
                testClassWithArrays.TestInteger = 2;
                testClassWithArrays.TestDouble = 1.1;
                testClassWithArrays.TestBoolean = true;
                testClassWithArrays.TestString = "test string";
                testClassWithArrays.TestDoubles = new[] { 1.1, 1.2, -1.1, -1.2 };
                testClassWithArrays.TestStrings = new[] { "one", "two", "three", "four" };
                testClassWithArrays.testDoublesField = new[] { 1.1, 1.2, -1.1, -1.2 };
                testClassWithArrays.testStringsField = new[] { "one", "two", "three", "four" };
                string filename = Path.Combine(folder, "testArrayObjects.H5");

                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId >= 0);

                Hdf5.WriteObject(fileId, testClassWithArrays, "objectWithTwoArrays");

                TestClassWithArray readObject = new TestClassWithArray
                {
                    TestStrings = new string[0],
                    TestDoubles = null,
                    TestDouble = double.NaN,
                };

                readObject = Hdf5.ReadObject(fileId, readObject, "objectWithTwoArrays");
                Assert.IsTrue(testClassWithArrays.Equals(readObject));

                readObject = Hdf5.ReadObject<TestClassWithArray>(fileId, "objectWithTwoArrays");
                Assert.IsTrue(testClassWithArrays.Equals(readObject));

                Assert.IsTrue(Hdf5.CloseFile(fileId) >= 0);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        public class Coordinate
        {
            [Hdf5EntryName("COORDINATES")] public double[,] COORDINATES { get; set; }
        }

        public class Steps
        {
            [Hdf5EntryName("STEP_0")] public double[,] STEP0 { get; set; }
            [Hdf5EntryName("STEP_1")] public double[,] STEP1 { get; set; }
        }

        //[TestMethod]
        public void ReadObject()
        {
            Hdf5.Settings.LowerCaseNaming = false;
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5Utils.LogWarning = (s) => Errors.Add(s);
            Hdf5Utils.LogError = (s) => Errors.Add(s);
            string filename = @"D:\h5\d.hdf5";
            long fileId = -1;
            try
            {
                fileId = Hdf5.OpenFile(filename, true);
                Assert.IsTrue(fileId > 0);

                //var result = Hdf5.ReadObject<Coordinate>(fileId, "/MODEL_STAGE[1]/MODEL/NODES");
                var step = "/MODEL_STAGE[1]/RESULTS/ON_NODES/DISPLACEMENT/DATA";
                var result2 = Hdf5.ReadObject<Steps>(fileId, step);
            }
            finally
            {
                if (fileId > 0)
                {
                    Hdf5.CloseFile(fileId);
                }
            }
        }

        //[TestMethod]
        public void ReadTable()
        {
            Hdf5.Settings.LowerCaseNaming = false;
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5Utils.LogWarning = (s) => Errors.Add(s);
            Hdf5Utils.LogError = (s) => Errors.Add(s);
            string filename = @"D:\h5\d.hdf5";
            long fileId = -1;
            try
            {
                fileId = Hdf5.OpenFile(filename, true);
                var groupName = "/MODEL_STAGE[1]/RESULTS/ON_NODES/DISPLACEMENT/DATA";
                var groupId = Hdf5.CreateOrOpenGroup(fileId, groupName);
                TabularData<double> tableOfData = Hdf5.Read2DTable<double>(groupId, "STEP_0");
            }
            finally
            {
                if (fileId > 0)
                {
                    Hdf5.CloseFile(fileId);
                }
            }
        }
    }
}