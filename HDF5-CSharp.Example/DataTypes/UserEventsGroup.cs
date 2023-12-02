using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDF5CSharp.Example.DataTypes
{
    public class Hdf5UserEvents
    {
        [Hdf5EntryName("user_events")] public UserEventRecord[] Events { get; set; }
        public Hdf5UserEvents(List<UserEventRecord> eventsData)
        {
            Events = eventsData.ToArray();
        }

        public Hdf5UserEvents()
        {
        }
    }

    public class UserEventsGroup : Hdf5BaseFile, IDisposable
    {
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private List<UserEventRecord> userEventsData;

        public UserEventsGroup(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, Constants.EventGroupName, logger)
        {
            userEventsData = new List<UserEventRecord>();
        }

        public void Enqueue(UserEventRecord userEventRecord)
        {
            userEventsData.Add(userEventRecord);
        }

        public Task WaitForDataWritten()
        {
            if (!userEventsData.Any())
            {
                Logger?.LogWarning("No user event to write to H5 file");
                return Task.CompletedTask;
            }
            var dataToWrite = userEventsData;
            userEventsData = new List<UserEventRecord>();
            Hdf5UserEvents eventsToH5 = new Hdf5UserEvents(dataToWrite);
            Logger?.LogInformation("Start write of User Events");
            var status = Hdf5.WriteObject(GroupRoot, eventsToH5, Constants.EventGroupName);
            Logger?.LogInformation("End write of User Events with status " + status);
            return Task.CompletedTask;
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
    }
}