using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HDF5CSharp.UnitTests
{
#if NET
    public class Net5Primitives
    {
        public Half HalfType { get; set; }
        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }
        public Half fieldHalfType;
        public DateOnly fieldDateOnly;
        public TimeOnly fieldTimeOnly;

        protected bool Equals(Net5Primitives other)
        {
            return fieldHalfType.Equals(other.fieldHalfType) && fieldDateOnly.Equals(other.fieldDateOnly) &&
                   fieldTimeOnly.Equals(other.fieldTimeOnly) && HalfType.Equals(other.HalfType) &&
                   DateOnly.Equals(other.DateOnly) && TimeOnly.Equals(other.TimeOnly);
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

            return Equals((Net5Primitives)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(fieldHalfType, fieldDateOnly, fieldTimeOnly, HalfType, DateOnly, TimeOnly);
        }
    }

    [TestClass]
    public class NET5DataTypeTests
    {
        [TestMethod]
        public void TestNET5Primitives()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            string fn = $"{nameof(TestNET5Primitives)}.h5";
            Net5Primitives testClass = new Net5Primitives
            {
                HalfType = (Half)10.0,
                DateOnly = new DateOnly(2022, 07, 13),
                TimeOnly = new TimeOnly(23, 23),
                fieldHalfType = (Half)10.0,
                fieldDateOnly = new DateOnly(2022, 07, 13),
                fieldTimeOnly = new TimeOnly(23, 23),
            };

            var fileID = Hdf5.CreateFile(fn);
            Hdf5.WriteObject(fileID, testClass, "testObject");
            Hdf5.CloseFile(fileID);
            var readObject = new Net5Primitives();
            fileID = Hdf5.OpenFile(fn);
            readObject = Hdf5.ReadObject(fileID, readObject, "testObject");

            Hdf5.CloseFile(fileID);
            Assert.IsTrue(readObject.Equals(testClass));
        }
    }
#endif
}