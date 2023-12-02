using HDF5CSharp.DataTypes;
using HDF5CSharp.Example.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HDF5CSharp.UnitTests.Core
{
    public class Hdf5SystemEvents
    {
        [Hdf5EntryName("system_events")] public SystemEvent[] Events { get; set; }
        public Hdf5SystemEvents(List<SystemEvent> eventsData)
        {
            Events = eventsData.ToArray();
        }

        public Hdf5SystemEvents()
        {
        }
    }
    public class SystemEventGroup : Hdf5BaseFile, IDisposable
    {
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private ReaderWriterLockSlim LockSlim { get; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private List<SystemEvent> SystemEventSamplesData { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private bool record;
        public SystemEventGroup(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "events", logger)
        {
            SystemEventSamplesData = new List<SystemEvent>();
            LockSlim = new ReaderWriterLockSlim();
        }


        public void Dispose()
        {
            try
            {
                if (!Disposed)
                {
                    Hdf5.CloseGroup(GroupId);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error closing System Event group: {e.Message}");
            }
        }

        public void Enqueue(SystemEventModel systemEvent)
        {
            if (record)
            {
                SystemEvent hdf5SystemEvent = new SystemEvent(systemEvent.TimeStamp, systemEvent.SystemEventType.ToString(), "", systemEvent.EventData, false);
                LockSlim.EnterWriteLock();
                SystemEventSamplesData.Add(hdf5SystemEvent);
                LockSlim.ExitWriteLock();
            }
        }


        public Task WaitForDataWritten()
        {
            try
            {
                record = false;
                LockSlim.EnterWriteLock();
                if (!SystemEventSamplesData.Any())
                {
                    Logger.LogWarning("No system events to write to H5 file");
                    return Task.CompletedTask;
                }

                var dataToWrite = SystemEventSamplesData;
                SystemEventSamplesData = new List<SystemEvent>();
                Logger.LogInformation("Start write of system events");
                Hdf5SystemEvents events = new Hdf5SystemEvents(dataToWrite);
                var status = Hdf5.WriteObject(GroupRoot, events, Constants.EventGroupName);
                Logger.LogInformation("End write of system events with status " + status);
                return Task.CompletedTask;
            }
            finally
            {
                LockSlim.ExitWriteLock();
            }
        }

        public void StopRecording() => record = false;

        public void StartLogging() => record = true;
    }
}