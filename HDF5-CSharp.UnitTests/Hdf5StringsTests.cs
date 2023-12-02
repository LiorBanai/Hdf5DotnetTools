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
        public void WriteAndReadOneString()
        {
            try
            {
                string[] str = new[] { "test" };

                string filename = Path.Combine(folder, "testOneStringList.H5");


                // Open file and write the strings
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                Hdf5.WriteStrings(fileId, "/test", str);

                // Read the strings and close file
                Assert.IsTrue(fileId > 0);
                IEnumerable<string> strs2 = Hdf5.ReadStrings(fileId, "/test", "", true).result;
                Assert.IsTrue(strs2.Count() == 1);
                foreach (var s in strs2)
                {
                    Assert.IsTrue(str[0] == s);
                }

                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadListOfStrings()
        {
            try
            {
                List<string> strs = new List<string>
                {
                    "t",
                    "tst",
                    "test1",
                    "small test",
                };

                string filename = Path.Combine(folder, "testStringList.H5");


                // Open file and write the strings
                var fileId = Hdf5.CreateFile(filename);
                Assert.IsTrue(fileId > 0);
                Hdf5.WriteStrings(fileId, "/test", strs);

                // Read the strings and close file
                Assert.IsTrue(fileId > 0);
                IEnumerable<string> strs2 = Hdf5.ReadStrings(fileId, "/test", "", true).result;
                Assert.IsTrue(strs.Count() == strs2.Count());
                foreach (var item in strs2.Select((str, i) => new { i, str }))
                {
                    Assert.IsTrue(item.str == strs[item.i]);
                }

                Hdf5.CloseFile(fileId);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadOneAsciiString()
        {
            try
            {
                string test = "This is a test string";
                string filename = Path.Combine(folder, "testOneString.H5");


                var fileId = Hdf5.CreateFile(filename);
                Hdf5.WriteAsciiString(fileId, "/test", test);
                Assert.IsTrue(Hdf5.CloseFile(fileId) == 0);

                fileId = Hdf5.OpenFile(filename);
                string readStr = Hdf5.ReadAsciiString(fileId, "/test");
                Assert.IsTrue(test == readStr);
                Assert.IsTrue(Hdf5.CloseFile(fileId) == 0);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }

        [TestMethod]
        public void WriteAndReadOneUnicodeString()
        {
            try
            {
                string test = "Γαζέες καὶ μυρτιὲς δὲν θὰ βρῶ πιὰ στὸ χρυσαφὶ ξέφωτο";
                string filename = Path.Combine(folder, "testUnicodeString.H5");


                var fileId = Hdf5.CreateFile(filename);
                Hdf5.WriteUnicodeString(fileId, "/test", test);
                Assert.IsTrue(Hdf5.CloseFile(fileId) >= 0);


                fileId = Hdf5.OpenFile(filename);
                string readStr = Hdf5.ReadUnicodeString(fileId, "/test");
                //var readStr = Hdf5.ReadStrings(fileId, "/test");
                Assert.IsTrue(test == readStr);
                Assert.IsTrue(Hdf5.CloseFile(fileId) >= 0);
            }
            catch (Exception ex)
            {
                CreateExceptionAssert(ex);
            }
        }
    }
}