using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HDF5CSharp.UnitTests
{
    [Hdf5Attributes(new[] { "some info", "more info" })]
    internal class AttributeSimpleClass : IEquatable<AttributeSimpleClass>
    {
        public class InnerClass : IEquatable<InnerClass>
        {
            public string noAttributeName = "empty;";

            [Hdf5("some money")]
            public decimal money = 100.12M;

            public bool Equals(InnerClass other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return noAttributeName == other.noAttributeName && money == other.money;
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

                return Equals((InnerClass)obj);
            }

            public override int GetHashCode()
            {
#if NET
                return HashCode.Combine(noAttributeName, money);
#else
                return 0;
#endif
            }
        }
        [Hdf5("birthdate")]
        public DateTime datetime;
        public double noAttribute = 10.0;
        public string StringProperty { get; private set; }
        public InnerClass inner;

        public AttributeSimpleClass()
        {
            datetime = new DateTime(1969, 12, 01, 12, 00, 00, DateTimeKind.Local);
            StringProperty = "stringValue";
            inner = new InnerClass();
        }

        public void SetStringProperty(string value) => StringProperty = value;
        public bool Equals(AttributeSimpleClass other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return datetime.Equals(other.datetime) && noAttribute.Equals(other.noAttribute) &&
                   Equals(inner, other.inner) && StringProperty == other.StringProperty;
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

            return Equals((AttributeSimpleClass)obj);
        }

        public override int GetHashCode()
        {
#if NET
            return HashCode.Combine(datetime, noAttribute, inner, StringProperty);
#else
            return 0;
#endif
        }
    }
    [Hdf5Attributes(new[] { "some info", "more info" })]
    [Hdf5GroupName("test_object")]
    internal class SaveReadAttributeClass
    {
        [Hdf5EntryName("test_int_read_only")]
        [Hdf5ReadWrite(Hdf5ReadWrite.ReadOnly)] public int TestIntReadOnly { get; set; }

        [Hdf5EntryName("test_int_save_only")]
        [Hdf5ReadWrite(Hdf5ReadWrite.WriteOnly)] public int TestIntDoNotRead { get; set; }

        [Hdf5EntryName("test_int_read_write")]
        [Hdf5ReadWrite(Hdf5ReadWrite.ReadWrite)] public int TestIntReadWrite { get; set; }

        [Hdf5EntryName("test_int_save_only")]
        [Hdf5ReadWrite(Hdf5ReadWrite.ReadOnly)] public int TestIntReadOnlyOfOtherProperty { get; set; }

        [Hdf5EntryName("test_int_no_attribute")] public int TestIntNoAttribute { get; set; }

        public SaveReadAttributeClass()
        {
        }
        public SaveReadAttributeClass(int value)
        {
            TestIntReadOnly = TestIntDoNotRead = TestIntNoAttribute = TestIntReadWrite = value;
        }

        protected bool Equals(SaveReadAttributeClass other)
        {
            return TestIntReadOnly == other.TestIntReadOnly && TestIntDoNotRead == other.TestIntDoNotRead && TestIntReadWrite == other.TestIntReadWrite && TestIntNoAttribute == other.TestIntNoAttribute;
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

            return Equals((SaveReadAttributeClass)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TestIntReadOnly;
                hashCode = (hashCode * 397) ^ TestIntDoNotRead;
                hashCode = (hashCode * 397) ^ TestIntReadWrite;
                hashCode = (hashCode * 397) ^ TestIntNoAttribute;
                return hashCode;
            }
        }
    }

    [Hdf5Attributes(new[] { "some info", "more info" })]
    internal class AttributeClass
    {
        [Hdf5KeyValuesAttributes("Key", new[] { "NestedInfo some info", "NestedInfo more info" })]
        public class NestedInfo
        {
            public int noAttribute = 10;

            [Hdf5("some money")]
            public decimal money = 100.12M;
        }

        [Hdf5("birthdate")]
        public DateTime aDatetime = new DateTime(1969, 12, 01, 12, 00, 00, DateTimeKind.Local);

        public double noAttribute = 10.0;

        public NestedInfo nested = new NestedInfo();
    }

    internal class AllTypesClass
    {
        public bool aBool = true;
        public byte aByte = 10;
        public char aChar = 'a';
        public DateTime aDatetime = new DateTime(1969, 12, 01, 12, 00, 00, DateTimeKind.Local);
        public decimal aDecimal = new decimal(2.344);
        public double aDouble = 2.1;
        public short aInt16 = 10;
        public int aInt32 = 100;
        public long aInt64 = 1000;
        public sbyte aSByte = 10;
        public float aSingle = 100;
        public ushort aUInt16 = 10;
        public uint aUInt32 = 100;
        public ulong aUInt64 = 1000;
        public string aString = "test";
        public TimeSpan aTimeSpan = TimeSpan.FromHours(1);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WData
    {
        public int serial_no;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string location;
        public double temperature;
        public double pressure;

        public DateTime Time
        {
            get => new DateTime(timeTicks);
            set => timeTicks = value.Ticks;
        }

        public long timeTicks;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WData2
    {
        public int serial_no;
        [Hdf5EntryName("location1")]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string location;
        public double temperature;
        public double pressure;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string label;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WData3
    {
        public int serial_no;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string location;
        public double temperature;
        public double pressure;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string label;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Responses
    {
        public long MCID;
        public int PanelIdx;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public short[] ResponseValues;
    }

    public class TestClass : IEquatable<TestClass>
    {
        public int TestInteger { get; set; }
        public double TestDouble { get; set; }
        public bool TestBoolean { get; set; }
        public string TestString { get; set; }
        [Hdf5EntryName("Test_time")]
        public DateTime TestTime { get; set; }

        public bool Equals(TestClass other)
        {
            return other.TestInteger == TestInteger &&
        other.TestDouble == TestDouble &&
        other.TestBoolean == TestBoolean &&
                    other.TestString == TestString &&
            other.TestTime == TestTime;
        }
    }
    public class TestClassWithArray : TestClass
    {
        public double[] testDoublesField;
        public string[] testStringsField;
        public double[] TestDoubles { get; set; }
        public string[] TestStrings { get; set; }

        public bool Equals(TestClassWithArray other)
        {
            return base.Equals(other) &&
                   other.TestDoubles.SequenceEqual(TestDoubles) &&
                   other.testDoublesField.SequenceEqual(testDoublesField) &&
                   other.testStringsField.SequenceEqual(testStringsField);
        }
    }
    internal class TestClassWithStructs
    {
        public TestClassWithStructs()
        {
        }
        public WData[] DataList { get; set; }
    }
    [Serializable]
    public class TestClassWithArrayOfFloats : IEquatable<TestClassWithArrayOfFloats>
    {
        public float[] Floats { get; set; }

        public TestClassWithArrayOfFloats()
        {
        }
        public TestClassWithArrayOfFloats(float seed)
        {
            Floats = new[] { 1f + seed, 2 + seed, 3f + seed, 4f + seed };
        }

        public bool Equals(TestClassWithArrayOfFloats other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Floats.SequenceEqual(other.Floats);
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

            return Equals((TestClassWithArrayOfFloats)obj);
        }

        public override int GetHashCode()
        {
            return (Floats != null ? Floats.GetHashCode() : 0);
        }
    }
    public struct TestStructWithArrayOfFloats : IEquatable<TestStructWithArrayOfFloats>
    {
        public float[] Floats { get; set; }
        public TestStructWithArrayOfFloats(float seed)
        {
            Floats = new[] { 1f + seed, 2 + seed, 3f + seed, 4f + seed };
        }

        public bool Equals(TestStructWithArrayOfFloats other)
        {
            return Floats.SequenceEqual(other.Floats);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((TestClassWithArrayOfFloats)obj);
        }

        public override int GetHashCode()
        {
            return (Floats != null ? Floats.GetHashCode() : 0);
        }
    }

    internal class TestClassWithJaggedArray : IEquatable<TestClassWithJaggedArray>
    {
        public List<int[][]> Data { get; set; }
        public List<int[][]> dataField;
        public TestClassWithJaggedArray()
        {
            Data = new List<int[][]>()
            {
                new[] { new[] { 1, 2, 3, 4, 5}},
                new[] { new[] { 11, 12, 13, 14, 15}},
            };
            dataField = new List<int[][]>()
            {
                new[] { new[] { 1, 2, 3, 4, 5}},
                new[] { new[] { 11, 12, 13, 14, 15}},
            };
        }

        public bool Equals(TestClassWithJaggedArray other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Data[0][0].SequenceEqual(other.Data[0][0]) &&
                   Data[1][0].SequenceEqual(other.Data[1][0]) &&
                   dataField[0][0].SequenceEqual(other.dataField[0][0]) &&
                   dataField[1][0].SequenceEqual(other.dataField[1][0]);
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

            return Equals((TestClassWithJaggedArray)obj);
        }

        public override int GetHashCode()
        {
#if NET
            return HashCode.Combine(dataField, Data);
#else
            return 0;
#endif
        }
    }

    internal class TestClassListOfList
    {
        public List<int[]> Data { get; set; }
        public List<int[]> dataField;

        public TestClassListOfList()
        {
            Data = new List<int[]>(2) { new[] { 1 }, new[] { 1, 2 } };
            dataField = new List<int[]>(21) { new[] { 2 }, new[] { 4, 5 } };
        }
    }
    internal class TestClassWithLists : IEquatable<TestClassWithLists>
    {
        public DateTime time;
        public List<int> numbers;
        public List<DateTime> times;

        public DateTime TimeProperty { get; set; }
        public List<int> NumbersProperty { get; set; }
        public List<DateTime> TimesProperty { get; set; }
        public List<TestClassWithArrayOfFloats> floats;
        public List<TestClassWithArrayOfFloats> FloatsProperties { get; set; }
        public TestClassWithLists()
        {
            time = DateTime.Now;
            times = new List<DateTime>
                { DateTime.Now.AddSeconds(10), DateTime.Now.AddSeconds(20), DateTime.Now.AddSeconds(30)};
            TimesProperty = new List<DateTime>
                { DateTime.Now.AddSeconds(10), DateTime.Now.AddSeconds(20), DateTime.Now.AddSeconds(30)};
            numbers = new List<int> { 1, 2, 3 };
            TimeProperty = DateTime.Now;
            NumbersProperty = new List<int> { 4, 5, 6 };
            floats = new List<TestClassWithArrayOfFloats> { new TestClassWithArrayOfFloats(1f), new TestClassWithArrayOfFloats(2) };

            FloatsProperties = new List<TestClassWithArrayOfFloats> { new TestClassWithArrayOfFloats(3f), new TestClassWithArrayOfFloats(4) };
        }

        public bool Equals(TestClassWithLists other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return numbers.SequenceEqual(other.numbers) && time.Equals(other.time) &&
                   NumbersProperty.SequenceEqual(other.NumbersProperty) && TimeProperty.Equals(other.TimeProperty) &&
                   times.SequenceEqual(other.times) && TimesProperty.SequenceEqual(other.TimesProperty);
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

            return Equals((TestClassWithLists)obj);
        }

        public override int GetHashCode() => (numbers != null ? numbers.GetHashCode() : 0);
    }
}