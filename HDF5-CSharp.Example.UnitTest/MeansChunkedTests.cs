using HDF5CSharp.DataTypes;
using HDF5CSharp.Example;
using HDF5CSharp.Example.DataTypes;
using HDF5CSharp.Example.DataTypes.HDF5Store.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HDF5_CSharp.Example.UnitTest
{
    [TestClass]
    public class MeansChunkedTests : BaseClass
    {
        [TestMethod]
        public async Task MeansChunkedTest()
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{nameof(MeansChunkedTest)}.h5");
            Console.WriteLine(filename);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            Kama = new KamaAcquisitionFile(filename, AcquisitionInterface.Simulator, Logger);
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

            Kama.SavePatientInfo(info.Patient, info.ExamDate);
            Kama.UpdateSystemInformation("32423423", new[] { "11", "12" });
            Kama.SetProcedureInformation(info);
            string data = File.ReadAllText(AcquisitionScanProtocolPath);
            AcquisitionProtocolParameters parameters = AcquisitionProtocolParameters.FromJson(data);
            await Kama.StartLogging(parameters);
            var meansData = await GenerateMeans();
            Kama.StopRecording();
            await Kama.StopProcedure();

            using (KamaAcquisitionReadOnlyFile readFile = new KamaAcquisitionReadOnlyFile(filename))
            {
                readFile.ReadSystemInformation();
                readFile.ReadProcedureInformation();
                readFile.ReadPatientInformation();
                Assert.IsTrue(readFile.PatientInformation.Equals(Kama.PatientInfo));
                Assert.IsTrue(readFile.ProcedureInformation.Equals(Kama.ProcedureInformation));
                Assert.IsTrue(readFile.SystemInformation.Equals(Kama.SystemInformation));

                var means = readFile.ReadMeansEvents();
                CheckMeans(meansData, means);
            }

            File.Delete(filename);
        }

        private void CheckMeans(List<(long Timestamp, string Data)> meansData, List<MeansFullECGEvent> means)
        {
            Assert.IsTrue(meansData.Count == means.Count);
            for (var i = 0; i < means.Count; i++)
            {
                MeansFullECGEvent mean = means[i];
                Assert.IsTrue(meansData[i].Timestamp == mean.timestamp);
                Assert.IsTrue(meansData[i].Data == mean.data);
                Assert.IsTrue(mean.index == i + 1);
            }
        }

        private async Task<List<(long, string)>> GenerateMeans()
        {
            var data = Enumerable.Range(0, 1000)
                .Select(i => (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), i.ToString())).ToList();

            var d1 = data.Take(10);
            foreach ((long Timestamp, string Data) d in d1)
            {
                Kama.AppendMean(d.Timestamp, d.Data);
            }

            await Task.Delay(5000);
            var d2 = data.Skip(10).Take(10).ToList();
            Kama.AppendMeans(d2);
            await Task.Delay(5000);
            var d3 = data.Skip(20).ToList();
            Kama.AppendMeans(d3);
            return data;
        }
    }
}