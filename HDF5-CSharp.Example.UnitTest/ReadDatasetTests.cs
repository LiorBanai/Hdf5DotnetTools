//using HDF.PInvoke;
//using HDF5CSharp;
//using HDF5CSharp.Example;
//using HDF5CSharp.Example.DataTypes;
//using Microsoft.Extensions.Logging;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace HDF5_CSharp.Example.UnitTest
//{
//    [TestClass]
//    public class ReadDatasetTests : BaseClass
//    {
//        string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test2.h5");
//        string file = @"D:\Lior\Kama Drive\kama\H5\1_John.h5";
//        private string eventsName = "/root/events/system_events";

//        [TestMethod]
//        public void TestReadPartial()
//        {
//            var fileId = Hdf5.OpenFile(file);
//            Assert.IsTrue(fileId > 0);
//            var se2 = Hdf5.ReadCompounds<SystemEvent2>(fileId, eventsName, "");

//        }
//    }
//}