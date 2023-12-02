using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HDF5CSharp.Example.DataTypes
{
    public class Hdf5RPositionEvents
    {
        [Hdf5EntryName("rposition_events")] public RPositionEvent[] Events { get; set; }
        public Hdf5RPositionEvents(List<RPositionEvent> eventsData)
        {
            Events = eventsData.ToArray();
        }

        public Hdf5RPositionEvents()
        {
        }
    }
    public class RPositionGroup : Hdf5BaseFile, IDisposable
    {
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private ReaderWriterLockSlim LockSlim { get; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private List<RPositionEvent> RPositionSamplesData { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private bool record;

        public RPositionGroup(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "events", logger)
        {
            RPositionSamplesData = new List<RPositionEvent>();
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
                Logger.LogError(e, $"Error closing RPosition group: {e.Message}");
            }
        }

        public void Enqueue(RPositionsMessagePack rPosition)
        {
            if (record)
            {
                var positions = rPosition.NavigationData.SelectMany(r =>
                    r.Points.Select(p => new RPositionEvent(rPosition.Timestamp, r.Name, p.x, p.y, p.z, r.Trajectory.x, r.Trajectory.y, r.Trajectory.z))).ToList();
                try
                {
                    LockSlim.EnterWriteLock();
                    RPositionSamplesData.AddRange(positions);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Error adding RPosition: {e.Message}");
                }
                finally
                {
                    LockSlim.ExitWriteLock();
                }
            }
        }

        public Task WaitForDataWritten()
        {
            try
            {
                record = false;
                LockSlim.EnterWriteLock();
                if (!RPositionSamplesData.Any())
                {
                    Logger?.LogWarning("No R Position events to write to H5 file");
                    return Task.CompletedTask;
                }
                var dataToWrite = RPositionSamplesData;
                RPositionSamplesData = new List<RPositionEvent>();
                Logger?.LogInformation("Start write of RPosition events");
                Hdf5RPositionEvents events = new Hdf5RPositionEvents(dataToWrite);
                var status = Hdf5.WriteObject(GroupRoot, events, Constants.EventGroupName);
                Logger?.LogInformation("End write of RPosition with status " + status);
            }
            catch (Exception e)
            {
                Logger?.LogError(e, $"Error writing RPosition Events: {e.Message}");
            }
            finally
            {
                LockSlim.ExitWriteLock();
            }
            return Task.CompletedTask;
        }

        public void StopRecording() => record = false;

        public void StartLogging() => record = true;
    }
}