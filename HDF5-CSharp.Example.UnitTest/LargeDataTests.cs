using HDF5CSharp.Example;
using HDF5CSharp.Example.DataTypes;
using HDF5CSharp.Example.DataTypes.HDF5Store.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HDF5_CSharp.Example.UnitTest
{
    [TestClass]
    public class Hdf5UnitTests
    {
        //[TestMethod]
        public async Task TestFullFileWriteReadWithManyMeansResults()
        {
            string meanContent = File.ReadAllText("SingleMeanResult.txt", Encoding.UTF8);

            // 1- we will load the h5 file and create a new one base on this one 
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test3.h5");

            Console.WriteLine(filename);

            var kama = new KamaAcquisitionFile(filename, AcquisitionInterface.Simulator, null);
            ProcedureInfo info = new ProcedureInfo
            {
                ExamDate = DateTime.Now,
                Procedure = "test",
                Patient = new PatientInfo()
                {
                    PatientFamilyName = "PArker",
                    PatientFirstName = "Peter",
                    PatientAge = 26,
                },
            };

            kama.SavePatientInfo(info.Patient, info.ExamDate);
            kama.UpdateSystemInformation("32423423", new[] { "11", "12" });
            kama.SetProcedureInformation(info);

            string data = File.ReadAllText("CalibrationInfoTest.json");
            var parameters = AcquisitionProtocolParameters.FromJson(data);
            await kama.StartLogging(parameters);

            for (int i = 0; i < 25000; i++)
            {
                string txt = meanContent.Replace("ZZZZZZZZZZZZZZ", i.ToString());
                SystemEventModel se = new SystemEventModel(SystemEventType.ECGBodyLeadConnected, i, txt);
                kama.AppendSystemEvent(se);
            }

            kama.StopRecording();
            await kama.StopProcedure();

            using (KamaAcquisitionReadOnlyFile readFile = new KamaAcquisitionReadOnlyFile(filename))
            {
                readFile.ReadSystemInformation();
                readFile.ReadProcedureInformation();
                readFile.ReadPatientInformation();
                var readEvents = readFile.ReadSystemEvents();
                Assert.IsTrue(readEvents.Count == 25000);
                for (int i = 0; i < 25000; i++)
                {
                    Assert.IsTrue(readEvents[i].timestamp == i);

                    // Assert.IsTrue(events[i].data == i);
                }
            }
            File.Delete(filename);
        }
    }
}