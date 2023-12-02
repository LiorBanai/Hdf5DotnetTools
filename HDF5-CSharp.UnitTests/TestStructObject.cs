using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HDF5CSharp.UnitTests
{
    public struct StructData
    {
        public int serial_no;
        public string location;
        public double temperature;
        public double pressure;

        public bool Equals(StructData other)
        {
            return serial_no == other.serial_no && location == other.location &&
                   temperature.Equals(other.temperature) && pressure.Equals(other.pressure);
        }

        public override bool Equals(object obj)
        {
            return obj is StructData other && Equals(other);
        }

        public override int GetHashCode()
        {
#if NET
            return HashCode.Combine(serial_no, location, temperature, pressure);
#endif
            return 0;
        }
    }

    public class TestClassWithStructMembers
    {
        public StructData StructData { get; set; }
        public StructData structDataField;

        protected bool Equals(TestClassWithStructMembers other)
        {
            return structDataField.Equals(other.structDataField) && StructData.Equals(other.StructData);
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

            return Equals((TestClassWithStructMembers)obj);
        }

        public override int GetHashCode()
        {
#if NET
            return HashCode.Combine(structDataField, StructData);
#endif
            return 0;
        }
    }

    [TestClass]
    public class TestStructObject
    {
        [TestMethod]
        public void TestStructMembers()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            string fn = $"{nameof(TestStructMembers)}.h5";
            var fileID = Hdf5.CreateFile(fn);
            var testClass = new TestClassWithStructMembers
            {
                structDataField = new StructData()
                { location = "loc", pressure = 10, serial_no = 50, temperature = 50.4 },
                StructData = new StructData()
                { location = "loc_prop", pressure = 20, serial_no = 60, temperature = 950.4 },
            };
            Hdf5.WriteObject(fileID, testClass, "testObject");
            Hdf5.CloseFile(fileID);
            var readObject = new TestClassWithStructMembers();
            fileID = Hdf5.OpenFile(fn);
            readObject = Hdf5.ReadObject(fileID, readObject, "testObject");
            Hdf5.CloseFile(fileID);
            Assert.IsTrue(readObject.Equals(testClass));
        }
    }
}