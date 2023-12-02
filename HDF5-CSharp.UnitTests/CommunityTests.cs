using Bogus;
using HDF.PInvoke;
using HDF5CSharp.UnitTests.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HDF5CSharp.UnitTests
{
    internal class HDF5DataClass
    {
        public double Location { get; set; }
        public byte[] Image { get; set; }

        protected bool Equals(HDF5DataClass other)
        {
            return Location.Equals(other.Location) && Image.SequenceEqual(other.Image);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((HDF5DataClass)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Location.GetHashCode() * 397) ^ (Image != null ? Image.GetHashCode() : 0);
            }
        }
    }

    public class Data
    {
        public float surfaceCurrentSpeed;
        public float surfaceCurrentDirection;
    }

    public class container
    {
        public Data[] values;
    }
    [TestClass]
    public class CommunityTests : Hdf5BaseUnitTests
    {
        //[TestMethod]
        public void TestTable()
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            string filename = Path.Combine(folder, "files", "table.H5");


            var fileId = Hdf5.OpenFile(filename);
            Assert.IsTrue(fileId > 0);
            var cmpList = Hdf5.Read2DTable<Data>(fileId, "/SurfaceCurrent/SurfaceCurrent.01/Group_009/values");
            Hdf5.CloseFile(fileId);
            // CollectionAssert.AreEqual(wData2List, cmpList);
        }

        [TestMethod]
        public void TestMemory()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            string fn = $"{nameof(TestMemory)}.h5";
            var data = TestCreateFile(fn).ToList();
            for (int i = 0; i < 100; i++)
            {
                TestReadAndCompare(fn, data);
            }
        }

        private void TestReadAndCompare(string fn, List<HDF5DataClass> original)
        {
            /*
            PerformanceCounter PC = new PerformanceCounter();
            PC.CategoryName = "Process";
            PC.CounterName = "Working Set - Private";
            PC.InstanceName = Process.GetCurrentProcess().ProcessName;
            */
            List<HDF5DataClass> fromH5File = TestReadFile(fn);
            //Console.WriteLine($"After Read {i}: {Convert.ToInt32(PC.NextValue()) / 1024}");
            Assert.IsTrue(original.SequenceEqual(fromH5File));
            // PC.Close();
            // PC.Dispose();
        }

        private IEnumerable<HDF5DataClass> GenerateDate()
        {
            Randomizer.Seed = new Random(8675309);
            var random = new Bogus.Randomizer();
            for (int i = 0; i < 10; i++)
            {
                yield return new HDF5DataClass
                {
                    Location = i * 10,
                    Image = random.Bytes(10048576),
                };
            }
        }

        private IEnumerable<HDF5DataClass> TestCreateFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var fileID = Hdf5.CreateFile(filename);
            List<HDF5DataClass> original = GenerateDate().ToList();
            for (int i = 0; i < original.Count; i++)
            {
                Hdf5.WriteObject(fileID, original[i], $"testObject{i}");
            }

            Hdf5.CloseFile(fileID);
            return original;
        }

        private List<HDF5DataClass> TestReadFile(string filename)
        {
            var fileID = Hdf5.OpenFile(filename);
            List<HDF5DataClass> read = new List<HDF5DataClass>();
            int i = 0;
            bool readOK = true;
            do
            {
                var dataClass = Hdf5.ReadObject<HDF5DataClass>(fileID, $"testObject{i++}");
                if (dataClass != null)
                {
                    read.Add(dataClass);
                }
                else
                {
                    readOK = false;
                }
            } while (readOK);

            Hdf5.CloseFile(fileID);
            return read;
        }

        [TestMethod]
        public void TestAttributesCreation()
        {
            string filename = $"{nameof(TestAttributesCreation)}.h5";
            long fileId = Hdf5.CreateFile(filename);

            long groupFId = Hdf5.CreateOrOpenGroup(fileId, "GROUP_F");

            var featureCodeDs = new Hdf5Dataset();
            featureCodeDs.WriteStrings(groupFId, "featureCode", new[] { "WaterLevel" });

            var waterLevelFeatures = new List<WaterLevelFeature>();
            waterLevelFeatures.Add(new WaterLevelFeature(
                code: "waterLevelHeight",
                name: "Water level height",
                uomName: "metres",
                fillValue: "-9999.0",
                dataType: "H5T_FLOAT",
                lower: "-99.99",
                upper: "99.99",
                closure: "closedInterval"));

            waterLevelFeatures.Add(new WaterLevelFeature(
                code: "waterLevelTrend",
                name: "Water level trend",
                uomName: "",
                fillValue: "0",
                dataType: "H5T_ENUM",
                lower: "",
                upper: "",
                closure: ""));

            waterLevelFeatures.Add(new WaterLevelFeature(
                code: "waterLevelTime",
                name: "Water level time",
                uomName: "DateTime",
                fillValue: "",
                dataType: "H5T_STRING",
                lower: "19000101T000000Z",
                upper: "21500101T000000Z",
                closure: "closedInterval"));

            Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
            attributes.Add("chunking", new List<string>() { "0,0" });

            Hdf5.WriteCompounds(groupFId, "WaterLevel", waterLevelFeatures, attributes);
            Hdf5.CloseGroup(groupFId);

            long waterLevelGroupId = Hdf5.CreateOrOpenGroup(fileId, "Waterlevel");
            Hdf5.WriteAttribute(waterLevelGroupId, "commonPointRule", (byte)4);
            Hdf5.WriteAttribute(waterLevelGroupId, "dataCodingFormat", (byte)1);
            Hdf5.WriteAttribute(waterLevelGroupId, "dimension", (Int16)2);
            Hdf5.WriteAttribute(waterLevelGroupId, "horizontalPositionUncertainty", (int)-1);
            Hdf5.WriteAttribute(waterLevelGroupId, "maxDatasetHeight", (float)2.898);
            Hdf5.WriteAttribute(waterLevelGroupId, "methodWaterLevelProduct", "pred, obsv, hcst, or fcst");
            Hdf5.WriteAttribute(waterLevelGroupId, "minDatasetHeight", (float)0.039);
            Hdf5.WriteAttribute(waterLevelGroupId, "numInstances", (Int16)1);
            Hdf5.WriteAttribute(waterLevelGroupId, "timeUncertainty", (float)-1.0);
            Hdf5.WriteAttribute(waterLevelGroupId, "verticalUncertainty", (float)-1.0);

            long wlGroup01 = Hdf5.CreateOrOpenGroup(waterLevelGroupId, "WaterLevel.01");
            Hdf5.WriteAttribute(wlGroup01, "dateTimeOfFirstRecord", "20190703T000000Z");
            Hdf5.WriteAttribute(wlGroup01, "dateTimeOfLastRecord", "20190704T000000Z");
            Hdf5.WriteAttribute(wlGroup01, "eastBoundLongitude", (double)3.5);
            Hdf5.WriteAttribute(wlGroup01, "northBoundLatitude", (double)53.2);
            Hdf5.WriteAttribute(wlGroup01, "numGRP", (Int16)4);
            Hdf5.WriteAttribute(wlGroup01, "numberOfStations", (Int16)4);
            Hdf5.WriteAttribute(wlGroup01, "southBoundLatitude", (double)50.2);
            Hdf5.WriteAttribute(wlGroup01, "typeOfWaterLevelData", (byte)2);
            Hdf5.WriteAttribute(wlGroup01, "westBoundLongitude", (double)1.1);

            long group001 = Hdf5.CreateOrOpenGroup(wlGroup01, "Group_001");
            var waterLevelItems = new List<WaterLevelItem>();
            waterLevelItems.Add(new WaterLevelItem(height: "1.325", trend: "0"));
            waterLevelItems.Add(new WaterLevelItem(height: "1.324", trend: "0"));
            waterLevelItems.Add(new WaterLevelItem(height: "1.238", trend: "0"));
            waterLevelItems.Add(new WaterLevelItem(height: "1.825", trend: "0"));

            Hdf5.WriteCompounds(group001, "values", waterLevelItems, attributes);
            Hdf5.WriteAttribute(group001, "timePoint", "20190703T000000Z");

            Hdf5.CloseGroup(group001);

            Hdf5.CloseGroup(wlGroup01);
            Hdf5.CloseGroup(waterLevelGroupId);
            Hdf5.CloseFile(fileId);
            File.Delete(filename);
        }

        [TestMethod]
        public void TestAttributesAsciiStringCreation()
        {
            string filename = $"{nameof(TestAttributesAsciiStringCreation)}.h5";
            long fileId = Hdf5.CreateFile(filename);

            long groupFId = Hdf5.CreateOrOpenGroup(fileId, "Ascii");

            var featureCodeDs = new Hdf5AttributeRW();
            featureCodeDs.WriteAsciiStringAttributes(groupFId, "Ascii", new[] { "WaterLevel" });

            Hdf5.CloseGroup(groupFId);
            Hdf5.CloseFile(fileId);
            File.Delete(filename);
        }
        [TestMethod]
        public void TestAttributesIntCreation()
        {
            string filename = $"{nameof(TestAttributesIntCreation)}.h5";
            long fileId = Hdf5.CreateFile(filename);

            long groupFId = Hdf5.CreateOrOpenGroup(fileId, "GROUP_F");

            var featureCodeDs = new Hdf5Dataset();
            featureCodeDs.WriteNumericAttributes(groupFId, "Int32Attribute", new int[] { 1,2,3 });
            featureCodeDs.WriteNumericAttributes(groupFId, "Int16Attribute", new short[] { 1, 2, 3 });
            featureCodeDs.WriteNumericAttributes(groupFId, "DoubleAttribute", new Double[] { 1.1f, 2.2f, 3.3f });
            featureCodeDs.WriteNumericAttributes(groupFId, "longAttribute", new long[] { 10, 20, 30 });

            Hdf5.CloseGroup(groupFId);
            Hdf5.CloseFile(fileId);
            File.Delete(filename);
        }
        [TestMethod]
        public void FileNotCloseAfterCreateGroupRecursivelyTest()
        {
            string fileName = $"{nameof(FileNotCloseAfterCreateGroupRecursivelyTest)}.h5";
            var fid = Hdf5.CreateFile(fileName);
            var lastGroup = Hdf5.CreateGroupRecursively(fid, "/1/2/3/4");
            Hdf5.CloseGroup(lastGroup);
            Hdf5.CloseFile(fid);
            File.Delete(fileName);
        }
        [TestMethod]
        public void FileNotCloseAfterCreateGroupRecursivelyCloseAllTest()
        {
            string fileName = $"{nameof(FileNotCloseAfterCreateGroupRecursivelyCloseAllTest)}.h5";
            var fid = Hdf5.CreateFile(fileName);
            var lastGroup = Hdf5.CreateGroupRecursively(fid, "/1/2/3/4", true, true);
            Hdf5.CloseFile(fid);
            File.Delete(fileName);
        }
    }
}