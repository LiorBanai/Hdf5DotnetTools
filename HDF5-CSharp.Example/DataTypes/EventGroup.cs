using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5GroupName("events")]
    public class EventGroup : Hdf5BaseFile, IDisposable
    {
        private Hdf5Events events;
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private List<SystemEvent> SystemEventSamplesData { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private bool record;
        public EventGroup(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "events", logger)
        {
            SystemEventSamplesData = new List<SystemEvent>();

            // SystemEvents = new ChunkedDataset<SystemEvent>("system_events", GroupId);
            // UserTags = new ChunkedDataset<UserTag>("user_tags", GroupId);
            // SystemTaskWriter = Task.Factory.StartNew(() =>
            //{

            //    completed = false;
            //    foreach (SystemEvent data in SystemEventSamplesData.GetConsumingEnumerable())
            //    {
            //        SystemEvent[,] events = new SystemEvent[1, 1];
            //        events[0, 0] = data;
            //        SystemEvents.AppendOrCreateDataset(events);
            //    }
            //    Flush(); //end of data sample. flush data
            //});
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
            catch (Exception)
            {
                //nothing
            }
        }

        public void Enqueue(SystemEventModel systemEvent)
        {
            if (record)
            {
                SystemEvent hdf5SystemEvent = new SystemEvent(systemEvent.TimeStamp, systemEvent.SystemEventType.ToString(), "", systemEvent.EventData, false);
                SystemEventSamplesData.Add(hdf5SystemEvent);
            }
        }

        public Task WaitForDataWritten()
        {
            record = false;
            Logger.LogInformation("Start write of system events");
            events = new Hdf5Events(SystemEventSamplesData);
            var status = Hdf5.WriteObject(GroupId, events);
            Logger.LogInformation("End write of system events with status " + status);
            return Task.CompletedTask;
        }

        public void StopRecording() => record = false;

        public void StartLogging() => record = true;
    }
}