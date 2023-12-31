#if NET
#nullable enable
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDF5CSharp.UnitTests
{
    public class TestInnerClassWithArrayWithNulls
    {
        public bool TestBooleanNonNull { get; set; }
        public bool TestBooleanNull { get; set; }
        public bool? TestBooleanNullableNonNull { get; set; }
        public bool? TestBooleanNullableNull { get; set; }
        public bool TestFieldBooleanNonNull;
        public bool TestFieldBooleanNull;
        public bool? TestFieldBooleanNullableNonNull;
        public bool? TestFieldBooleanNullableNull;

        protected bool Equals(TestInnerClassWithArrayWithNulls other)
        {
            return TestFieldBooleanNonNull == other.TestFieldBooleanNonNull &&
                   TestFieldBooleanNull == other.TestFieldBooleanNull &&
                   TestFieldBooleanNullableNonNull == other.TestFieldBooleanNullableNonNull &&
                   TestFieldBooleanNullableNull == other.TestFieldBooleanNullableNull &&
                   TestBooleanNonNull == other.TestBooleanNonNull && TestBooleanNull == other.TestBooleanNull &&
                   TestBooleanNullableNonNull == other.TestBooleanNullableNonNull &&
                   TestBooleanNullableNull == other.TestBooleanNullableNull;
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

            return Equals((TestInnerClassWithArrayWithNulls)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TestFieldBooleanNonNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestFieldBooleanNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestFieldBooleanNullableNonNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestFieldBooleanNullableNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestBooleanNonNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestBooleanNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestBooleanNullableNonNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestBooleanNullableNull.GetHashCode();
                return hashCode;
            }
        }
    }

    public class TestClassWithArrayWithNulls
    {
        public bool TestBooleanNonNull { get; set; }
        public bool TestBooleanNull { get; set; }
        public bool? TestBooleanNullableNonNull { get; set; }
        public bool? TestBooleanNullableNull { get; set; }
        public bool TestFieldBooleanNonNull;
        public bool TestFieldBooleanNull;
        public bool? TestFieldBooleanNullableNonNull;
        public bool? TestFieldBooleanNullableNull;

        public TestInnerClassWithArrayWithNulls InnerClassWithArrayWithNullsNonNull { get; set; }
        public TestInnerClassWithArrayWithNulls innerFieldClassWithArrayWithNullsNonNull;
#if NET
        public TestInnerClassWithArrayWithNulls? InnerClassWithArrayWithNull { get; set; }
        public TestInnerClassWithArrayWithNulls? innerFieldClassWithArrayWithNulls;

#endif

        public TestClassWithArrayWithNulls()
        {
            InnerClassWithArrayWithNullsNonNull = new TestInnerClassWithArrayWithNulls();
            innerFieldClassWithArrayWithNullsNonNull = new TestInnerClassWithArrayWithNulls();
        }
        protected bool Equals(TestClassWithArrayWithNulls other)
        {
            return TestFieldBooleanNonNull == other.TestFieldBooleanNonNull &&
                   TestFieldBooleanNull == other.TestFieldBooleanNull &&
                   TestFieldBooleanNullableNonNull == other.TestFieldBooleanNullableNonNull &&
                   TestFieldBooleanNullableNull == other.TestFieldBooleanNullableNull &&
                   TestBooleanNonNull == other.TestBooleanNonNull && TestBooleanNull == other.TestBooleanNull &&
                   TestBooleanNullableNonNull == other.TestBooleanNullableNonNull &&
                   TestBooleanNullableNull == other.TestBooleanNullableNull &&
                   InnerClassWithArrayWithNullsNonNull.Equals(other.InnerClassWithArrayWithNullsNonNull) &&
                   innerFieldClassWithArrayWithNullsNonNull.Equals(other.innerFieldClassWithArrayWithNullsNonNull);
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

            return Equals((TestClassWithArrayWithNulls)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TestFieldBooleanNonNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestFieldBooleanNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestFieldBooleanNullableNonNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestFieldBooleanNullableNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestBooleanNonNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestBooleanNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestBooleanNullableNonNull.GetHashCode();
                hashCode = (hashCode * 397) ^ TestBooleanNullableNull.GetHashCode();
                return hashCode;
            }
        }
    }
    [TestClass]
    public class NullableTests : Hdf5BaseUnitTests
    {
        [TestMethod]
        public void TestNullableClass()
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);

            string fn = $"{nameof(TestNullableClass)}.h5";
            TestClassWithArrayWithNulls testClass = new TestClassWithArrayWithNulls
            {
                TestBooleanNonNull = true,
                TestBooleanNull = false,
                TestBooleanNullableNonNull = true,
                TestBooleanNullableNull = null,
                TestFieldBooleanNonNull = true,
                TestFieldBooleanNull = false,
                TestFieldBooleanNullableNonNull = true,
                TestFieldBooleanNullableNull = null,
            };

            testClass.InnerClassWithArrayWithNullsNonNull = new TestInnerClassWithArrayWithNulls
            {
                TestBooleanNonNull = true,
                TestBooleanNull = false,
                TestBooleanNullableNonNull = true,
                TestBooleanNullableNull = null,
                TestFieldBooleanNonNull = true,
                TestFieldBooleanNull = false,
                TestFieldBooleanNullableNonNull = true,
                TestFieldBooleanNullableNull = null,
            };
            testClass.innerFieldClassWithArrayWithNullsNonNull = new TestInnerClassWithArrayWithNulls
            {
                TestBooleanNonNull = true,
                TestBooleanNull = false,
                TestBooleanNullableNonNull = true,
                TestBooleanNullableNull = null,
                TestFieldBooleanNonNull = true,
                TestFieldBooleanNull = false,
                TestFieldBooleanNullableNonNull = true,
                TestFieldBooleanNullableNull = null,
            };

#if NET
            testClass.InnerClassWithArrayWithNull = new TestInnerClassWithArrayWithNulls
            {
                TestBooleanNonNull = true,
                TestBooleanNull = false,
                TestBooleanNullableNonNull = true,
                TestBooleanNullableNull = null,
                TestFieldBooleanNonNull = true,
                TestFieldBooleanNull = false,
                TestFieldBooleanNullableNonNull = true,
                TestFieldBooleanNullableNull = null,
            };
            testClass.innerFieldClassWithArrayWithNulls = new TestInnerClassWithArrayWithNulls
            {
                TestBooleanNonNull = true,
                TestBooleanNull = false,
                TestBooleanNullableNonNull = true,
                TestBooleanNullableNull = null,
                TestFieldBooleanNonNull = true,
                TestFieldBooleanNull = false,
                TestFieldBooleanNullableNonNull = true,
                TestFieldBooleanNullableNull = null,
            };
#endif
            var fileID = Hdf5.CreateFile(fn);
            Hdf5.WriteObject(fileID, testClass, "testObject");
            Hdf5.CloseFile(fileID);
            var readObject = new TestClassWithArrayWithNulls();
#if NET
            // readObject.InnerClassWithArrayWithNull = new TestInnerClassWithArrayWithNulls();
            // readObject.innerFieldClassWithArrayWithNulls = new TestInnerClassWithArrayWithNulls();
#endif
            fileID = Hdf5.OpenFile(fn);
            readObject = Hdf5.ReadObject(fileID, readObject, "testObject");

            Hdf5.CloseFile(fileID);
            Assert.IsTrue(readObject.TestBooleanNonNull == testClass.TestBooleanNonNull);
            Assert.IsTrue(readObject.TestBooleanNull == testClass.TestBooleanNull);
            Assert.IsTrue(readObject.TestBooleanNullableNonNull == testClass.TestBooleanNullableNonNull);
            Assert.IsTrue(readObject.TestBooleanNullableNull == null && testClass.TestBooleanNullableNull == null);

            Assert.IsTrue(readObject.TestFieldBooleanNonNull == testClass.TestFieldBooleanNonNull);
            Assert.IsTrue(readObject.TestFieldBooleanNull == testClass.TestFieldBooleanNull);
            Assert.IsTrue(readObject.TestFieldBooleanNullableNonNull == testClass.TestFieldBooleanNullableNonNull);
            Assert.IsTrue(readObject.TestFieldBooleanNullableNull == null &&
                          testClass.TestFieldBooleanNullableNull == null);

            Assert.IsTrue(readObject.Equals(testClass));
            Assert.IsTrue(readObject.InnerClassWithArrayWithNullsNonNull.Equals(testClass.InnerClassWithArrayWithNullsNonNull));
            Assert.IsTrue(readObject.innerFieldClassWithArrayWithNullsNonNull.Equals(testClass.innerFieldClassWithArrayWithNullsNonNull));

#if NET
            Assert.IsTrue(readObject.InnerClassWithArrayWithNull.Equals(testClass.InnerClassWithArrayWithNull));
            Assert.IsTrue(readObject.innerFieldClassWithArrayWithNulls.Equals(testClass.innerFieldClassWithArrayWithNulls));

#endif
        }
    }
}