using HDF5CSharp.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace HDF5CSharp.UnitTests
{
    public partial class Hdf5UnitTests
    {
        [TestMethod]
        public void WriteAndReadList()
        {
            string filename = Path.Combine(folder, $"{nameof(WriteAndReadList)}.H5");
            var obj = new TestClassWithLists();
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var status = Hdf5.WriteObject(fileId, obj, "test");
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
                var objWithList = Hdf5.ReadObject<TestClassWithLists>(fileId, "test");
                Assert.IsTrue(obj.Equals(objWithList));
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadJaggedArray()
        {
            string filename = Path.Combine(folder, $"{nameof(WriteAndReadJaggedArray)}.H5");
            var obj = new TestClassWithJaggedArray();
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var status = Hdf5.WriteObject(fileId, obj, "test");
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
                var obj2 = Hdf5.ReadObject<TestClassWithJaggedArray>(fileId, "test");
                Assert.IsTrue(obj.Equals(obj2));
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }
        [TestMethod]
        public void WriteAndReadObjectWithStructs()
        {
            string filename = Path.Combine(folder, "testObjectWithStructArray.H5");


            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var status = Hdf5.WriteObject(fileId, classWithStructs, "test");
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }

            try
            {
                TestClassWithStructs objWithStructs;
                var fileId = Hdf5.OpenFile(filename);
                Assert.IsTrue(fileId > 0);
                objWithStructs = Hdf5.ReadObject<TestClassWithStructs>(fileId, "test");
                CollectionAssert.AreEqual(wDataList, objWithStructs.DataList);
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }
        [TestMethod]
        public void WriteAndReadListOfList()
        {
            string filename = Path.Combine(folder, $"{nameof(WriteAndReadList)}.H5");
            TestClassListOfList data = new TestClassListOfList();
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                Hdf5.WriteObject(fileId, data, "test");
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
                var objWithList = Hdf5.ReadObject<TestClassListOfList>(fileId, "test");
                Hdf5.CloseFile(fileId);
                Assert.IsTrue(objWithList.Data[0].SequenceEqual(data.Data[0]));
                Assert.IsTrue(objWithList.Data[1].SequenceEqual(data.Data[1]));
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }


        public struct SystemEvent
        {
            [Hdf5EntryName("timestamp")] public long timestamp;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)] [Hdf5EntryName("type")] public string type;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)] [Hdf5EntryName("data")] public string data;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)] [Hdf5EntryName("description")] public string description;


            public SystemEvent(long timestamp, string type, string description, string data)
            {
                this.timestamp = timestamp;
                this.type = type;
                this.description = description;
                this.data = data;
            }
        }
        [TestMethod]
        public void WriteAndReadSystemEvent()
        {
            string filename = Path.Combine(folder, "testCompounds.H5");
            List<SystemEvent> se = new List<SystemEvent>();
            Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
            try
            {
                se.Add(new SystemEvent(5, "55", "3300000000000000000000000", "555555555555555555555555555555555"));
                se.Add(new SystemEvent(1, "255", "3d3000000000007777773", "ggggggggggggdf"));

                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var status = Hdf5.WriteCompounds(fileId, "/test", se, attributes);
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
                var cmpList = Hdf5.ReadCompounds<SystemEvent>(fileId, "/test", "", true).ToArray();
                Hdf5.CloseFile(fileId);
                CollectionAssert.AreEqual(se, cmpList);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadStructs()
        {
            string filename = Path.Combine(folder, "testCompoundsWithDifferentDisplayName.H5");
            Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var status = Hdf5.WriteCompounds(fileId, "/test", wData2List, attributes);
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
                var cmpList = Hdf5.ReadCompounds<WData2>(fileId, "/test", "", true).ToArray();
                Hdf5.CloseFile(fileId);
                CollectionAssert.AreEqual(wData2List, cmpList);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void ReadStructs()
        {
            string filename = Path.Combine(folder, "files", "testCompounds_WData2_WData3.H5");
            try
            {
                var fileId = Hdf5.OpenFile(filename);
                Assert.IsTrue(fileId > 0);
                var cmpList = Hdf5.ReadCompounds<WData3>(fileId, "/test", "", true).ToArray();
                Hdf5.CloseFile(fileId);
                // CollectionAssert.AreEqual(wData2List, cmpList);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }
        [TestMethod]
        public void WriteAndReadStructsWithDatetime()
        {
            string filename = Path.Combine(folder, "testCompounds.H5");
            Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var status = Hdf5.WriteCompounds(fileId, "/test", wDataList, attributes);
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
                var cmpList = Hdf5.ReadCompounds<WData>(fileId, "/test", "", true).ToArray();
                Hdf5.CloseFile(fileId);
                CollectionAssert.AreEqual(wDataList, cmpList);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadStructsWithArray()
        {
            string filename = Path.Combine(folder, "testArrayCompounds.H5");
            Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
            try
            {
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var status = Hdf5.WriteCompounds(fileId, "/test", responseList, attributes);
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
                Responses[] cmpList = Hdf5.ReadCompounds<Responses>(fileId, "/test", "", true).ToArray();
                Hdf5.CloseFile(fileId);
                var isSame = responseList.Zip(cmpList, (r, c) =>
                {
                    return r.MCID == c.MCID &&
                    r.PanelIdx == c.PanelIdx &&
                    r.ResponseValues.Zip(c.ResponseValues, (rr, cr) => rr == cr).All(v => v == true);
                });
                Assert.IsTrue(isSame.All(s => s == true));
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }



        [StructLayout(LayoutKind.Sequential)]
        public struct Compound
        {
            public double timestamp;
            public Int32 id;
            [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_R8)]
            public double[] double_array;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string name;

            public Compound(double timestamp, int id, double[] doubleArray, string name)
            {
                this.timestamp = timestamp;
                this.id = id;
                double_array = doubleArray;
                this.name = name;
            }
        }

        // [TestMethod]
        public void WriteCompoundTest()
        {
            string filename = Path.Combine(folder, "WriteCompound.H5");
            try
            {
                var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var id = Int32.MaxValue;
                var double_array = new[] { 10.0, 20, 30 };
                var name = "test";
                var c = new Compound(timestamp, id, double_array, name);
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                var status = Hdf5.WriteCompounds(fileId, "/Sample Data", new List<Compound>() { c }, new Dictionary<string, List<string>>());
                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }
    }
}