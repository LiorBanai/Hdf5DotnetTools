using HDF.PInvoke;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HDF5CSharp.UnitTests
{
    [TestClass]
    public class H5ETest
    {
        [TestMethod]
        public void H5EwalkTest1()
        {
            H5E.auto_t auto_cb = ErrorDelegateMethod;
            Assert.IsTrue(
                H5E.set_auto(H5E.DEFAULT, auto_cb, IntPtr.Zero) >= 0);

            H5E.walk_t walk_cb = WalkDelegateMethod;
            Assert.IsTrue(
                H5E.walk(H5E.DEFAULT, H5E.direction_t.H5E_WALK_DOWNWARD,
                    walk_cb, IntPtr.Zero) >= 0);
        }


        [TestMethod]
        public void H5EwalkTest2()
        {
           // H5E.set_auto(H5E.DEFAULT, ErrorDelegateMethod, IntPtr.Zero);
           Assert.IsTrue(
                H5E.set_auto(H5E.DEFAULT, ErrorDelegateMethod, IntPtr.Zero) >= 0);

            H5E.walk_t walk_cb = WalkDelegateMethod;
            Assert.IsTrue(
                H5E.push(H5E.DEFAULT, "hello.c", "sqrt", 77, H5E.ERR_CLS,
                    H5E.NONE_MAJOR, H5E.NONE_MINOR, "Hello, World!") >= 0);

            Assert.IsTrue(
                H5E.push(H5E.DEFAULT, "hello.c", "sqr", 78, H5E.ERR_CLS,
                    H5E.NONE_MAJOR, H5E.NONE_MINOR, "Hello, World!") >= 0);

            Assert.IsTrue(
                H5E.walk(H5E.DEFAULT, H5E.direction_t.H5E_WALK_DOWNWARD,
                    walk_cb, IntPtr.Zero) >= 0);
        }

        public static int ErrorDelegateMethod(long estack, IntPtr client_data)
        {
            H5E.walk(estack, H5E.direction_t.H5E_WALK_DOWNWARD, WalkDelegateMethod, IntPtr.Zero);
            return 0;
        }

        public static int WalkDelegateMethod(uint n, ref H5E.error_t err_desc, IntPtr client_data)
        {
            // log your error, e.g. logger.LogInformation(err_desc.desc);
            return 0;
        }
    }
}