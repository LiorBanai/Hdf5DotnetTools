using HDF5CSharp.Example.DataTypes;
using HDF5CSharp.Example.DataTypes.HDF5Store.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HDF5CSharp.Example
{
    public class KamaAcquisitionReadOnlyFile : IDisposable
    {
        public string FileName { get; }
        public ProcedureInformation ProcedureInformation { get; set; }
        public SystemInformation SystemInformation { get; set; }
        public Patient PatientInformation { get; set; }
        public ECGData ECG { get; set; }
        public List<EITEntry> EITs { get; set; }
        public List<SystemEvent> Events { get; set; }
        public List<MeansFullECGEvent> Means { get; set; }
        public bool HasEIT => EITs.Any();
        private long fileId;
        private string rootName = "/";
        private string rootNameOld = "/root";
        private string system_informationName = "/system_information";
        private string procedure_informationName = "/procedure_information";
        private string patient_informationName = "/patient";
        private string ecgName = "/ecg";
        private string eitName = "/eit";
        private string eventsName = "/events/system_events";
        private string meansName = "/means/ecg_means_events";
        private bool fileClosed;

        public KamaAcquisitionReadOnlyFile(string filename)
        {
            FileName = filename;
            ProcedureInformation = new ProcedureInformation();
            SystemInformation = new SystemInformation();
            PatientInformation = new Patient();
            ECG = new ECGData();
            EITs = new List<EITEntry>();
            Events = new List<SystemEvent>();
            Means = new List<MeansFullECGEvent>();
            Hdf5.Settings.LowerCaseNaming = true;
            Hdf5.Settings.DateTimeType = DateTimeType.UnixTimeMilliseconds;
            fileId = Hdf5.OpenFile(filename);
        }

        public void ReadSystemInformation()
        {
            string groupName = rootName + system_informationName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                SystemInformation = Hdf5.ReadObject<SystemInformation>(fileId, groupName);
                return;
            }
            groupName = rootNameOld + system_informationName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                SystemInformation = Hdf5.ReadObject<SystemInformation>(fileId, groupName);
            }
        }
        public void ReadProcedureInformation()
        {
            string groupName = rootName + procedure_informationName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                ProcedureInformation = Hdf5.ReadObject<ProcedureInformation>(fileId, groupName);
                return;
            }
            groupName = rootNameOld + procedure_informationName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                ProcedureInformation = Hdf5.ReadObject<ProcedureInformation>(fileId, groupName);
            }
        }

        public void ReadPatientInformation()
        {
            string groupName = rootName + patient_informationName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                PatientInformation = Hdf5.ReadObject<Patient>(fileId, groupName);
                return;
            }
            groupName = rootNameOld + patient_informationName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                PatientInformation = Hdf5.ReadObject<Patient>(fileId, groupName);
            }
        }

        public void ReadECGData()
        {
            string groupName = rootName + ecgName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                ECG = Hdf5.ReadObject<ECGData>(fileId, groupName);
                return;
            }
            groupName = rootNameOld + ecgName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                ECG = Hdf5.ReadObject<ECGData>(fileId, groupName);
            }
        }
        public void ReadEITData()
        {
            int index = 1;
            string rootGroup = rootName + eitName;
            if (!Hdf5.GroupExists(fileId, rootGroup))
            {
                rootGroup = rootNameOld + eitName;
            }

            while (Hdf5.GroupExists(fileId, rootGroup + "/d" + index))
            {
                var entry = Hdf5.ReadObject<EITEntry>(fileId, rootGroup + "/d" + index);
                EITs.Add(entry);
                index++;
            }
        }
        public List<MeansFullECGEvent> ReadMeansEvents()
        {
            if (Means.Any())
            {
                return Means;
            }
            if (fileClosed)
            {
                fileId = Hdf5.OpenFile(FileName);
                fileClosed = false;
            }
            string groupName = rootName + meansName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                try
                {
                    var e = Hdf5.ReadCompounds<MeansFullECGEvent>(fileId, groupName, "", false);
                    Means.AddRange(e);
                    return Means;
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error reading means events: " + exception);
                }
            }

            return new List<MeansFullECGEvent>(0);
        }

        public List<SystemEvent> ReadSystemEvents()
        {
            if (Events.Any())
            {
                return Events;
            }
            if (fileClosed)
            {
                fileId = Hdf5.OpenFile(FileName);
                fileClosed = false;
            }
            string groupName = rootName + eventsName;
            if (Hdf5.GroupExists(fileId, groupName))
            {
                try
                {
                    var e = Hdf5.ReadCompounds<SystemEvent>(fileId, groupName, "", false);
                    Events.AddRange(e);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error reading events: " + exception);
                }
            }
            return Events;
        }

        public void Dispose()
        {
            if (!fileClosed)
            {
                Hdf5.CloseFile(fileId);
                fileClosed = true;
            }
        }
    }
}