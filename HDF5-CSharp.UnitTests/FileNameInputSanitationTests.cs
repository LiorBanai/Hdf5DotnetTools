using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HDF5CSharp.UnitTests
{
    [TestClass]
    public class FileNameInputSanitationTests
    {

        [TestInitialize]
        public void Init()
        {

        }

        private static readonly Regex _illegalCharacterValidator = new("[æøåöäïë€]+", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        [TestMethod]
        public void TestValidFileName()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + "_validfilename.h5"); 
            if(_illegalCharacterValidator.IsMatch(path))
            {
                Assert.Inconclusive("Cannot conclude this as Path.GetTempPath() returns path with forbidden characters in it."); // TheBaronOfDubstep: I have an ø in my username, therefore.. 
            }
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                var s = fileInfo.Create();
                s.Flush();
                s.Close();
            }
            try
            {
                var id = Hdf5.OpenFile(path);
                Assert.AreNotEqual(-1, id);
            }
            catch (Exception ex)
            {
                Assert.IsNotInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.IsNotInstanceOfType(ex, typeof(FileNotFoundException));
                Assert.IsNotInstanceOfType(ex, typeof(ArgumentNullException));
                Assert.IsNotInstanceOfType(ex, typeof(ArgumentOutOfRangeException));
            }
            finally
            {
                fileInfo.Delete();
            }
        }

        [TestMethod]
        public void TestInvalidFileNameWithShortpathFix()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + "_vælidfïlënæïm.h5");
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                var s = fileInfo.Create();
                s.Flush();
                s.Close();
            }
            try
            {
                var id = Hdf5.OpenFile(path, attemptShortPath: true);
                Assert.AreNotEqual(-1, id);
            }
            catch (Exception ex)
            {
                Assert.IsNotInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.IsNotInstanceOfType(ex, typeof(FileNotFoundException));
                Assert.IsNotInstanceOfType(ex, typeof(ArgumentNullException));
                Assert.IsNotInstanceOfType(ex, typeof(ArgumentOutOfRangeException));
            }
            finally
            {
                fileInfo.Delete();
            }
        }

        [TestMethod]
        public void TestInvalidFileNameWithoutShortpathFix()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + "_vælidfïlënæïm.h5");
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                var s = fileInfo.Create();
                s.Flush();
                s.Close();
            }
            long id = 0L;
            try
            {
                id = Hdf5.OpenFile(path);
                Assert.AreNotEqual(-1, id);
                Hdf5.CloseFile(id);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentOutOfRangeException));
                Assert.IsNotInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.IsNotInstanceOfType(ex, typeof(FileNotFoundException));
                Assert.IsNotInstanceOfType(ex, typeof(ArgumentNullException));
            }
            finally
            {
                fileInfo.Delete();
            }
        }

        [TestMethod]
        public void TestInvalidFileNameWithNonExistantFile()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + "_vælidfïlënæïm.h5");
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists) fileInfo.Delete();
            try
            {
                var id = Hdf5.OpenFile(path, attemptShortPath: true);
                Assert.AreNotEqual(-1, id);
            }
            catch (Exception ex)
            {
                Assert.IsNotInstanceOfType(ex, typeof(ArgumentOutOfRangeException));
                Assert.IsNotInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.IsInstanceOfType(ex, typeof(FileNotFoundException));
                Assert.IsNotInstanceOfType(ex, typeof(ArgumentNullException));
            }
            finally
            {
                fileInfo.Delete();
            }
        }
    }
}
