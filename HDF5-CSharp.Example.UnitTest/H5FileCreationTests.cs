using HDF5CSharp;
using HDF5CSharp.Example;
using HDF5CSharp.Example.DataTypes;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HDF5_CSharp.Example.UnitTest
{
    public class BaseClass
    {
        protected ILogger Logger { get; }
        protected KamaAcquisitionFile kama { get; set; }
        protected string AcquisitionScanProtocolPath { get; set; }
        private List<string> Errors { get; set; }
        protected BaseClass()
        {
            Errors = new List<string>();
            AcquisitionScanProtocolPath = "AcquisitionScanProtocol.json";
            var loggerFactory = LoggerFactory.Create(builder =>
            {
            });
            Logger = loggerFactory.CreateLogger<BaseClass>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Errors = new List<string>();
            Hdf5.Settings.EnableLogging(true);
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5Utils.LogError = (s) => Errors.Add(s);
        }
        [TestCleanup]
        public void TestCleanup()
        {
            Assert.IsTrue(Errors.Count == 0, $"Errors were found: {String.Join(",",Errors)}");
            Errors.Clear();
        }
    }

    [TestClass]
    public class H5FileCreationTests : BaseClass
    {
        private string calibrationPath = "CalibrationInfoTest.json";

        [TestMethod]
        public async Task TestFullFileWriteRead()
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test2.h5");
            Console.WriteLine(filename);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            kama = new KamaAcquisitionFile(filename, AcquisitionInterface.Simulator, Logger);
            ProcedureInfo info = new ProcedureInfo
            {
                ExamDate = DateTime.Now,
                Procedure = "test",
                Patient = new PatientInfo()
                {
                    PatientFamilyName = "PArker",
                    PatientFirstName = "Peter",
                    PatientAge = 26
                },
            };

            kama.SavePatientInfo(info.Patient, info.ExamDate);
            kama.UpdateSystemInformation("32423423", new[] { "11", "12" });
            kama.SetProcedureInformation(info);
            string data = File.ReadAllText(AcquisitionScanProtocolPath);
            AcquisitionProtocolParameters parameters = AcquisitionProtocolParameters.FromJson(data);
            await kama.StartLogging(parameters);
            var ecgTask = WriteECGData(kama, 2, 1);
            var eitTask = WriteEITData(parameters, kama, 5, 1);
            var seTask = WriteSystemEvents(kama);
            await Task.WhenAll(ecgTask, eitTask, seTask);
            var ecgTestData = await ecgTask;
            var eitsTestData = await eitTask;

            kama.StopRecording();
            await kama.StopProcedure();


            using (KamaAcquisitionReadOnlyFile readFile = new KamaAcquisitionReadOnlyFile(filename))
            {
                readFile.ReadSystemInformation();
                readFile.ReadProcedureInformation();
                readFile.ReadPatientInformation();
                Assert.IsTrue(readFile.PatientInformation.Equals(kama.PatientInfo));
                Assert.IsTrue(readFile.ProcedureInformation.Equals(kama.ProcedureInformation));
                Assert.IsTrue(readFile.SystemInformation.Equals(kama.SystemInformation));

                readFile.ReadECGData();

                readFile.ReadEITData();
                Assert.IsTrue(readFile.EITs.SequenceEqual(eitsTestData));
                readFile.ReadSystemEvents();
                Assert.IsTrue(readFile.Events.Count == 100);
            }

            File.Delete(filename);
        }

        [TestMethod]
        public async Task TestCalibrationsData()
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestCalibrationsData.h5");
            Console.WriteLine(filename);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            kama = new KamaAcquisitionFile(filename, AcquisitionInterface.Simulator, Logger);
            ProcedureInfo info = new ProcedureInfo
            {
                ExamDate = DateTime.Now,
                Procedure = "test",
                Patient = new PatientInfo()
                {
                    PatientFamilyName = "PArker",
                    PatientFirstName = "Peter",
                    PatientAge = 26
                },
            };

            kama.SavePatientInfo(info.Patient, info.ExamDate);
            kama.UpdateSystemInformation("32423423", new[] { "11", "12" });
            string data = File.ReadAllText(calibrationPath);
            CalibrationsSystemInformation calib = CalibrationsSystemInformation.FromJson(data);
            kama.AddCalibrationsData(calib);
            await kama.StopProcedure();
            File.Delete(filename);
        }

        [TestMethod]
        public async Task TestsSystemInformation()
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestsSystemInformation.h5");
            Console.WriteLine(filename);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            kama = new KamaAcquisitionFile(filename, AcquisitionInterface.Simulator, Logger);
            ProcedureInfo info = new ProcedureInfo
            {
                ExamDate = DateTime.Now,
                Procedure = "test",
                Patient = new PatientInfo()
                {
                    PatientFamilyName = "PArker",
                    PatientFirstName = "Peter",
                    PatientAge = 26
                },
            };
            kama.SavePatientInfo(info.Patient, info.ExamDate);
            kama.UpdateSystemInformation("32423423", new[] { "11", "12" });
            await kama.StopProcedure();
            File.Delete(filename);
        }

        private void CreatePatientDetails(KamaAcquisitionFile file)
        {
            ProcedureInfo info = new ProcedureInfo
            {
                ExamDate = DateTime.Now,
                Procedure = "test",
                Patient = new PatientInfo()
                {
                    PatientFamilyName = "PArker",
                    PatientFirstName = "Peter",
                    PatientAge = 26
                },
            };

            file.SavePatientInfo(info.Patient, info.ExamDate);
        }

        private void UpdateSystemInformation(KamaAcquisitionFile file)
        {
            kama.UpdateSystemInformation("32423423", new[] { "11", "12" });
        }

        private async Task<List<ECGFrame>> WriteECGData(KamaAcquisitionFile file, int loop,
            int sleepsBetweenWritesMilliseconds)
        {
            var result = await Task.Factory.StartNew(() =>
            {
                int count = 10;
                List<ECGFrame> ecgFrames = new List<ECGFrame>();
                for (int index = 0; index < loop; index++)
                {
                    var unFilteredData = new List<List<float>>
                       {
                           Enumerable.Range(0, count).Select(i => 0.0f + index * 100 + i).ToList(),
                           Enumerable.Range(0, count).Select(i => 50.0f + index * 100 + i).ToList(),
                       };

                    var filteredData = new List<List<float>>
                       {
                           Enumerable.Range(0, count).Select(i => 1000.0f + index * 100 + i).ToList(),
                           Enumerable.Range(0, count).Select(i => 1050.0f + index * 100 + i).ToList(),
                       };

                    var timestamps = Enumerable.Range(0, count).Select(i => DateTimeOffset.Now.ToUnixTimeMilliseconds()).ToList();

                    ECGFrame frame = new ECGFrame(filteredData, unFilteredData, timestamps[0], (ulong)index);

                    file.AppendEcgSample(frame);
                    ecgFrames.Add(frame);
                    Thread.Sleep(sleepsBetweenWritesMilliseconds);
                }

                return ecgFrames;
            });
            return result;
        }

        private async Task<List<EITEntry>> WriteEITData(AcquisitionProtocolParameters parameters,
            KamaAcquisitionFile file, int loop, int sleepsBetweenWritesMilliseconds)
        {
            var result = await Task.Factory.StartNew(() =>
            {
                Random random = new Random();
                List<ElectrodeFrame> samples = new List<ElectrodeFrame>();
                for (int index = 0; index < loop; index++)
                {
                    ElectrodeFrame frame = new ElectrodeFrame();
                    frame.ComplexCurrentMatrix = Enumerable.Range(0, 2441)
                        .Select(i => ((float)random.NextDouble(), (float)random.NextDouble())).ToArray();
                    frame.ComplexVoltageMatrix = Enumerable.Range(0, 2441)
                        .Select(i => ((float)random.NextDouble(), (float)random.NextDouble())).ToArray();
                    frame.PacketId = (ulong)index;
                    var timestamp = DateTimeOffset.Now;
                    frame.timestamp = timestamp.ToUnixTimeMilliseconds();
                    ulong saturationMask = (ulong)random.Next();
                    saturationMask = (saturationMask << 32);
                    saturationMask = saturationMask | (uint)random.Next();
                    frame.SaturationMask = saturationMask;
                    Thread.Sleep(sleepsBetweenWritesMilliseconds);
                    file.AppendElectrodeSample(frame);
                    samples.Add(frame);
                }

                EITEntry entry = new EITEntry
                {
                    Configuration = parameters.AsJson(),
                    StartDateTime = DateTimeOffset.FromUnixTimeMilliseconds(samples.First().timestamp).DateTime,
                    EndDateTime = DateTimeOffset.FromUnixTimeMilliseconds(samples.Last().timestamp).DateTime,
                    VoltagesReal = new float[samples.Count, samples[0].ComplexVoltageMatrix.Length],
                    VoltagesIm = new float[samples.Count, samples[0].ComplexVoltageMatrix.Length],
                    CurrentsIm = new float[samples.Count, samples[0].ComplexVoltageMatrix.Length],
                    CurrentsReal = new float[samples.Count, samples[0].ComplexVoltageMatrix.Length],
                    Saturation = new ulong[samples.Count, 1],
                    Timestamps = new long[samples.Count, 1],
                };

                for (var i = 0; i < samples.Count; i++)
                {
                    ElectrodeFrame electrodeFrame = samples[i];

                    for (int j = 0; j < electrodeFrame.ComplexVoltageMatrix.Length; j++)
                    {
                        entry.VoltagesReal[i, j] = electrodeFrame.ComplexVoltageMatrix[j].Re;
                        entry.VoltagesIm[i, j] = electrodeFrame.ComplexVoltageMatrix[j].Im;
                    }

                    for (int j = 0; j < electrodeFrame.ComplexCurrentMatrix.Length; j++)
                    {
                        entry.CurrentsReal[i, j] = electrodeFrame.ComplexCurrentMatrix[j].Re;
                        entry.CurrentsIm[i, j] = electrodeFrame.ComplexCurrentMatrix[j].Im;
                    }

                    entry.Saturation[i, 0] = electrodeFrame.SaturationMask;
                    entry.Timestamps[i, 0] = electrodeFrame.timestamp;
                }

                return entry;
            });
            return new List<EITEntry>
            {
                result,
            };
        }

        private Task WriteSystemEvents(KamaAcquisitionFile file)
        {
            Array values = Enum.GetValues(typeof(SystemEventType));
            Random random = new Random();

            return Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    SystemEventType set = (SystemEventType)values.GetValue(random.Next(values.Length));
                    SystemEventModel se = new SystemEventModel(set, DateTimeOffset.Now.ToUnixTimeMilliseconds(), "Empty");
                    file.AppendSystemEvent(se);
                }
            });
        }

        //[TestMethod]
        public void TestRead()
        {
            //string filename = @"D:\Data\9_pig.h5";
            string filename = @"c:\kalpa\test.h5";
            //string filename = @"d:\data\test2400.h5";
            var fileId = Hdf5.OpenFile(filename);
            Stopwatch st = Stopwatch.StartNew();
            var ds = Hdf5.ReadDatasetToArray<float>(fileId, "/eit/d1/voltages.im");
            st.Stop();
            Console.WriteLine(ds.result.Length);
            Console.WriteLine("read time im: " + st.ElapsedMilliseconds);
            st.Restart();
            ds = Hdf5.ReadDatasetToArray<float>(fileId, "/eit/d1/voltages.re");
            st.Stop();
            Console.WriteLine(ds.result.Length);
            Console.WriteLine("read time re: " + st.ElapsedMilliseconds);
            Hdf5.CloseFile(fileId);
        }

        public async Task TestEventsWrite()
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testEvents.h5");
            Console.WriteLine(filename);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            kama = new KamaAcquisitionFile(filename, AcquisitionInterface.Simulator, Logger);
            ProcedureInfo info = new ProcedureInfo
            {
                ExamDate = DateTime.Now,
                Procedure = "test",
                Patient = new PatientInfo()
                {
                    PatientFamilyName = "PArker",
                    PatientFirstName = "Peter",
                    PatientAge = 26
                },
            };

            kama.SavePatientInfo(info.Patient, info.ExamDate);
            kama.UpdateSystemInformation("32423423", new[] { "11", "12" });
            string data = File.ReadAllText(AcquisitionScanProtocolPath);
            AcquisitionProtocolParameters parameters = AcquisitionProtocolParameters.FromJson(data);
            await kama.StartLogging(parameters);



            kama.StopRecording();
            await kama.StopProcedure();
            File.Delete(filename);
        }


        [TestMethod]
        public void WriteAndReadUserSystemEvents()
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WriteAndReadUserSystemEvents.H5");

            var userEventsData = new List<UserEventRecord>()
            {
                new UserEventRecord("research1", "button1", "none",DateTimeOffset.Now.ToUnixTimeMilliseconds()),
                new UserEventRecord("research1", "button2", "none", DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10),
            };
            var fileId = Hdf5.CreateFile(filename);
            Hdf5UserEvents hdf5UserEvents = new Hdf5UserEvents(userEventsData);
            Assert.IsTrue(fileId > 0);
            var status = Hdf5.WriteObject(fileId, hdf5UserEvents, "test");
            Hdf5.CloseFile(fileId);

            fileId = Hdf5.OpenFile(filename);
            Assert.IsTrue(fileId > 0);
            var objWithStructs = Hdf5.ReadObject<Hdf5UserEvents>(fileId, "test");
            CollectionAssert.AreEqual(hdf5UserEvents.Events, objWithStructs.Events);
            Hdf5.CloseFile(fileId);
        }
    }
}