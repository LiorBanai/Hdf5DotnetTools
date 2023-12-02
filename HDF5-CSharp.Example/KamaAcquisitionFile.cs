using HDF.PInvoke;
using HDF5CSharp.Example.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HDF5CSharp.Example
{
    public class KamaAcquisitionFile
    {
        private long fileId;
        long groupRoot;
        private long groupEIT;
        //h5 data
        public ProcedureInformation ProcedureInformation { get; set; }
        public SystemInformation SystemInformation { get; set; }
        public EIT EIT { get; set; }
        public ECG ECG { get; set; }
        public InjectionGroup InjectionGroup { get; set; }
        public CalibrationGroup CalibrationGroup { get; set; }
        public Patient PatientInfo { get; set; }
        public SystemEventGroup SystemEvents { get; set; }
        public RPositionGroup RPosition { get; set; }
        public TagsGroup Tags { get; set; }
        private UserEventsGroup UserEventsGroup { get; set; }
        //end h5 data
        private static ILogger Logger { get; set; }

        private int RecordNumber { get; set; }
        private bool RecordingInProgress { get; set; }
        private bool FileClosed { get; set; }
        public string FileName { get; private set; }
        private int EITDefaultChunkSize { get; set; }
        private int ECGDefaultChunkSize { get; set; }
        public MeansGroup MeansData { get; set; }

        static KamaAcquisitionFile()
        {
            Hdf5.Settings.LowerCaseNaming = true;
            Hdf5.Settings.DateTimeType = DateTimeType.UnixTimeMilliseconds;
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            Hdf5Utils.LogError = s => Logger?.LogError($"HDF5 Error: {s}");
            Hdf5Utils.LogWarning = s => Logger?.LogWarning($"HDF5 Warning: {s}");
            Hdf5Utils.LogDebug = s => Logger?.LogDebug($"HDF5 Debug: {s}");
            Hdf5Utils.LogInfo = s => Logger?.LogInformation($"HDF5 Info: {s}");
        }
        public KamaAcquisitionFile(string filename, AcquisitionInterface acquisitionInterface, ILogger logger,
            int eitDefaultChunkSize = 24000, int ecgDefaultChunkSize = 10)
        {
            FileName = filename;
            Logger = logger;
            EITDefaultChunkSize = eitDefaultChunkSize;
            ECGDefaultChunkSize = ecgDefaultChunkSize;
            RecordNumber = 1;
            H5E.set_auto(H5E.DEFAULT, null, IntPtr.Zero);
            fileId = Hdf5.CreateFile(filename);
            groupRoot = fileId; 
            MeansData = new MeansGroup(fileId, groupRoot, logger);

            groupEIT = Hdf5.CreateOrOpenGroup(groupRoot, "eit");
            ProcedureInformation = new ProcedureInformation(fileId, groupRoot, logger)
            {
                ProcedureDirectory = Path.GetDirectoryName(filename),
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now,
            };

            SystemInformation = new SystemInformation(fileId, groupRoot, logger);
            SystemInformation.SystemType = acquisitionInterface.ToString();
            //  InjectionGroup = new InjectionGroup(fileId, groupRoot);
            CalibrationGroup = new CalibrationGroup(fileId, groupRoot, logger);
            SystemEvents = new SystemEventGroup(fileId, groupRoot, logger);
            RPosition = new RPositionGroup(fileId, groupRoot, logger);
            Tags = new TagsGroup(fileId, groupRoot, logger);
            UserEventsGroup = new UserEventsGroup(fileId, groupRoot, logger);
        }

        #region public

        public void AppendElectrodeSample(ElectrodeFrame sample)
        {
            EIT.Enqueue(sample);
        }

        public void AppendEcgSample(ECGFrame e)
        {
            ECG.Enqueue(e);
        }
        public void StopRecording()
        {
            EIT?.CompleteAdding();
            ECG?.CompleteAdding();
            SystemEvents?.StopRecording();
            RPosition?.StopRecording();
            Tags?.StopRecording();
            MeansData?.StopRecording();
            RecordingInProgress = false;
        }

        public Task StartLogging(AcquisitionProtocolParameters acquisitionProtocol)
        {
            EIT = new EIT(RecordNumber++, EITDefaultChunkSize, acquisitionProtocol.AsJson(), fileId, groupEIT, Logger);
            //var acquisitionInformation = new AcquisitionInformation(acquisitionProtocol, fileId, groupEIT, Logger);
            //acquisitionInformation.FlushDataAndCloseObject();
            ECG = new ECG(fileId, groupRoot, ECGDefaultChunkSize, (int)acquisitionProtocol.ScanDescription.EcgParams.SampleRate, Logger);
            SystemEvents.StartLogging();
            RPosition.StartLogging();
            Tags.StartLogging();
            MeansData.StartLogging();

            return Task.CompletedTask;
        }

        private async Task CloseHandles()
        {
            ProcedureInformation.FlushDataAndCloseObject();
            SystemInformation.FlushDataAndCloseObject();
            CalibrationGroup.FlushDataAndCloseObject();
            //wait for writing all data before resetting
            if (EIT != null)
            {
                await EIT.WaitForDataWritten();
                EIT.Dispose();
            }

            if (ECG != null)
            {
                await ECG.WaitForDataWritten();
                ECG.Dispose();
            }
            if (SystemEvents != null)
            {
                await SystemEvents.WaitForDataWritten();
                SystemEvents.Dispose();
            }

            if (RPosition != null)
            {
                await RPosition.WaitForDataWritten();
                RPosition.Dispose();
            }
            if (Tags != null)
            {
                await Tags.WaitForDataWritten();
                Tags.Dispose();
            }
            if (UserEventsGroup != null)
            {
                await UserEventsGroup.WaitForDataWritten();
                UserEventsGroup.Dispose();
            }
            if (MeansData != null)
            {
                await MeansData.WaitForDataWritten();
                MeansData.Dispose();
            }
            await Task.CompletedTask;
        }

        #endregion

        public async Task<(bool, string)> StopProcedure()
        {
            if (!File.Exists(FileName))
            {
                string msg = $"File {FileName} does not exist";
                Logger?.LogWarning(msg);
                return (false, msg);
            }
            if (FileClosed)
            {
                return GeneralUtils.CheckFileSize(FileName);
            }

            FileClosed = true;
            Logger?.LogInformation("Stop Procedure called");
            ProcedureInformation.EndDateTime = DateTime.Now;
            if (RecordingInProgress)
            {
                StopRecording();
            }

            await CloseHandles();
            Hdf5.Flush(groupRoot, H5F.scope_t.GLOBAL);
            Hdf5.CloseGroup(groupEIT);
            long result = Hdf5.CloseFile(fileId);
            if (result >= 0)
            {
                Logger?.LogInformation("Stop Procedure H5 File closed");
            }
            else
            {
                Logger?.LogError("Cannot close H5 File: " + result);
            }
            return GeneralUtils.CheckFileSize(FileName);
        }

        public void SavePatientInfo(PatientInfo p, DateTime examDate)
        {
            PatientInfo = new Patient(fileId, groupRoot, Logger)
            {
                FirstName = p.PatientFirstName ?? "Unknown",
                LastName = p.PatientFamilyName ?? "Unknown",
                Age = p.PatientAge,
                Id = p.PatientId ?? "Unknown",
                Gender = p.PatientGender ?? "Unknown",
                Type = p.Type ?? "Unknown",
                ExamDate = examDate,
                Height = p.PatientHeight,
                Weight = p.PatientWeight,
            };
            PatientInfo.FlushDataAndCloseObject();
        }

        public void UpdateSystemInformation(string systemId, string[] boardIds)
        {
            SystemInformation.SystemId = systemId;
            SystemInformation.BoardIds = boardIds;
        }

        public void SetProcedureInformation(ProcedureInfo procedureInfo)
        {
            ProcedureInformation.ProcedureType = procedureInfo.Procedure ?? string.Empty;
            ProcedureInformation.GeolocationData = procedureInfo.GeolocationData ?? string.Empty;
            ProcedureInformation.ProcedureInstitute = procedureInfo.Institute ?? string.Empty;
            ProcedureInformation.guid = procedureInfo.Guid.ToString();
            ProcedureInformation.LocalTimeZone = procedureInfo.LocalTimeZone ?? string.Empty;
            ProcedureInformation.UtcOffset = procedureInfo.UtcOffset;
            ProcedureInformation.IsDaylightSaving = procedureInfo.IsDaylightSaving;
            ProcedureInformation.StartDateTime = procedureInfo.StartTimeUtc;
            ProcedureInformation.Reviewer = procedureInfo.Reviewer;
        }

        public void AppendEcgCycleDescriptionSample(ECGCycleDescription e) => ECG.AppendEcgCycleDescriptionSample(e);

        public void AppendSystemEvent(SystemEventModel systemEvent) => SystemEvents.Enqueue(systemEvent);


        public void AddCalibrationsData(CalibrationsSystemInformation calibrationsSystemInformation)
        {
            CalibrationGroup.AddCalibrationsData(calibrationsSystemInformation);
            CalibrationGroup.FlushDataAndCloseObject();
        }

        public void AppendTag(string tag)
        {
            Tags.Enqueue(tag);
            Logger?.LogInformation("logTag: " + tag);
        }

        public void AppendUserEvent(UserEventRecord record) => UserEventsGroup.Enqueue(record);

        public void SaveSystemAssemblies(List<AssemblyInformationRecord> assembliesInformationRecord)
        {
            SystemInformation.Assemblies = assembliesInformationRecord.ToArray();
        }

        public void AppendRPosition(RPositionsMessagePack rPositions) => RPosition.Enqueue(rPositions);
        public void AppendMean(long timestamp, string data) => MeansData.Enqueue(timestamp, data);
        public void AppendMeans(List<(long timestamp, string data)> data) => MeansData.EnqueueRange(data);
    }
}