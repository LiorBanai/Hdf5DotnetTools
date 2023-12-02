using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HDF5CSharp.UnitTests
{
    public abstract class Hdf5BaseUnitTests
    {
        protected static int ErrorCountExpected;
        private static List<string> Errors { get; set; }
        private static List<string> Warning { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Errors = new List<string>();
            Warning = new List<string>();
            EnableErrors();
        }
        [TestCleanup]
        public void Cleanup()
        {
            Assert.IsTrue(Errors.Count == ErrorCountExpected, "Error exists");
            ErrorCountExpected = 0;
            Errors.Clear();
        }

        private static void EnableErrors()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5Utils.LogWarning = (s) => Warning.Add(s);
            Hdf5Utils.LogError = (s) => Errors.Add(s);
        }
    }

    [TestClass]
    public partial class Hdf5UnitTests
    {
        static private TestClass testClass;
        static private TestClassWithArray testClassWithArrays;
        static private List<double[,]> dsets;
        static private WData[] wDataList;
        static private WData2[] wData2List;
        static private Responses[] responseList;
        static private AllTypesClass allTypesObject;
        static private TestClassWithStructs classWithStructs;
        protected static int ErrorCountExpected;
        private static List<string> Errors { get; set; }
        static private string folder;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            Hdf5.Settings.LowerCaseNaming = true;

            //folder = System.IO.Path.GetTempPath();
            folder = AppDomain.CurrentDomain.BaseDirectory;
            dsets = new List<double[,]>
            {
                CreateDataset(),
                CreateDataset(10),
                CreateDataset(20),
            };

            wDataList = new WData[4]
            {
                new WData() { serial_no = 1153, location = "Exterior (static)", temperature = 53.23, pressure = 24.57, Time=new DateTime(2000, 1, 1) },
                new WData() { serial_no = 1184, location = "Intake",  temperature = 55.12, pressure = 22.95, Time=new DateTime(2000, 1, 2) },
                new WData() { serial_no = 1027, location = "Intake manifold", temperature = 103.55, pressure = 31.23, Time=new DateTime(2000, 1, 3) },
                new WData() { serial_no = 1313, location = "Exhaust manifold", temperature = 1252.89, pressure = 84.11, Time=new DateTime(2000, 1, 4) },
            };

            wData2List = new WData2[4]
            {
                new WData2() { serial_no = 1153, location = "Exterior (static)", label="V", temperature = 53.23, pressure = 24.57 },
                new WData2() { serial_no = 1184, location = "Intake", label="uV", temperature = 55.12, pressure = 22.95 },
                new WData2() { serial_no = 1027, location = "Intake manifold", label="V", temperature = 103.55, pressure = 31.23 },
                new WData2() { serial_no = 1313, location = "Exhaust manifold", label="mV", temperature = 1252.89, pressure = 84.11 },
            };
            responseList = new Responses[4]
            {
                new Responses() { MCID=1, PanelIdx=5, ResponseValues=new short[4] { 1, 2, 3, 4} },
                new Responses() { MCID=2, PanelIdx=6, ResponseValues=new short[4] { 5, 6, 7, 8}},
                new Responses() { MCID=3, PanelIdx=7, ResponseValues=new short[4] { 1, 2, 3, 4}},
                new Responses() { MCID=4, PanelIdx=8, ResponseValues=new short[4] { 5, 6, 7, 8}},
            };

            classWithStructs = new TestClassWithStructs { DataList = wDataList };
            testClass = new TestClass();
            testClassWithArrays = new TestClassWithArray();
            allTypesObject = new AllTypesClass();

            var files = Directory.GetFiles(folder, "*.H5");
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Errors = new List<string>();
            EnableErrors();
        }
        [TestCleanup]
        public void Cleanup()
        {
            foreach (var e in Errors)
            {
                Console.WriteLine(e);
            }

            Assert.IsTrue(Errors.Count == ErrorCountExpected, "Error exists");
            ErrorCountExpected = 0;
            Errors.Clear();
        }

        private static void EnableErrors()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5Utils.LogWarning = (s) => Errors.Add(s);
            Hdf5Utils.LogError = (s) => Errors.Add(s);
        }

        /// <summary>
        /// create a matrix and fill it with numbers
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>the matrix </returns>
        private static double[,] CreateDataset(int offset = 0)
        {
            var dset = new double[10, 5];
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 5; j++)
                {
                    double x = i + j * 5 + offset;
                    dset[i, j] = (j == 0) ? x : x / 10;
                }
            return dset;
        }

        private static void CompareDatasets<T>(T[,] dset, T[,] dset2)
        {
            Assert.IsTrue(dset.Rank == dset2.Rank);
            Assert.IsTrue(
                Enumerable.Range(0, dset.Rank).All(dimension =>
                dset.GetLength(dimension) == dset2.GetLength(dimension)));
            Assert.IsTrue(dset.Cast<T>().SequenceEqual(dset2.Cast<T>()));
        }

        private void CreateExceptionAssert(Exception ex)
        {
            Console.WriteLine(ex.ToString());
            var failStr = "Unexpected exception of type {0} caught: {1}";
            failStr = string.Format(failStr, ex.GetType(), ex.Message);
            Assert.Fail(failStr);
        }
    }
}