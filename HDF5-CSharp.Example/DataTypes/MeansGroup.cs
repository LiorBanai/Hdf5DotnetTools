using HDF5CSharp.DataTypes;
using HDF5CSharp.Example.DataTypes.HDF5Store.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HDF5CSharp.Example.DataTypes
{
    public class MeansGroup : Hdf5BaseFile, IDisposable
    {
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private ReaderWriterLockSlim LockSlim { get; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private List<MeansFullECGEvent> MeansSamplesData { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private bool record;
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private long index;
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private int BatchSizeInSeconds;
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private int BatchSizeInSamples;
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private PeriodicTimer MeansSystemEventWriter { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private Task MeansSystemEventTaskWriter { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private ChunkedCompound<MeansFullECGEvent> ChunkedMeansSystemEvents { get; set; }
        private CancellationTokenSource cts;

        public MeansGroup(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "means", logger)
        {
            MeansSamplesData = new List<MeansFullECGEvent>();

            cts = new CancellationTokenSource();
            LockSlim = new ReaderWriterLockSlim();
            BatchSizeInSamples = 1;
            BatchSizeInSeconds = 4;
            index = 0;
            ChunkedMeansSystemEvents = new ChunkedCompound<MeansFullECGEvent>("ecg_means_events", GroupId);
            MeansSystemEventWriter = new PeriodicTimer(TimeSpan.FromSeconds(BatchSizeInSeconds));
            var token = cts.Token;
            MeansSystemEventTaskWriter = Task.Run(async () =>
            {
                while (await MeansSystemEventWriter.WaitForNextTickAsync())
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (MeansSamplesData.Count >= BatchSizeInSamples)
                    {
                        AppendSamples();
                    }
                }
            }, token);
        }

        private void AppendSamples()
        {
            try
            {
                LockSlim.EnterWriteLock();
                if (MeansSamplesData.Any())
                {
                    ChunkedMeansSystemEvents.AppendOrCreateCompound(MeansSamplesData);
                    MeansSamplesData.Clear();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error appending means events: {e.Message}");
            }
            finally
            {
                LockSlim.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            try
            {
                if (!Disposed)
                {
                    MeansSystemEventWriter.Dispose();
                    ChunkedMeansSystemEvents?.Dispose();
                    Hdf5.CloseGroup(GroupId);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error closing Means group: {e.Message}");
            }
        }

        public void Enqueue(long timestamp, string data)
        {
            if (record)
            {
                try
                {
                    LockSlim.EnterWriteLock();
                    Interlocked.Increment(ref index);
                    var mse = new MeansFullECGEvent(index, timestamp, data);
                    MeansSamplesData.Add(mse);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Error adding means full event: {e.Message}");
                }
                finally
                {
                    LockSlim.ExitWriteLock();
                }
            }
        }

        public Task WaitForDataWritten()
        {
            cts.Cancel(false);
            record = false;
            AppendSamples();
            return Task.CompletedTask;
        }

        public void StopRecording() => record = false;

        public void StartLogging() => record = true;

        public void EnqueueRange(List<(long Timestamp, string Data)> data)
        {
            if (record)
            {
                try
                {
                    LockSlim.EnterWriteLock();
                    List<MeansFullECGEvent> itms = new List<MeansFullECGEvent>(data.Count);
                    foreach ((long Timestamp, string Data) d in data)
                    {
                        Interlocked.Increment(ref index);
                        var mse = new MeansFullECGEvent(index, d.Timestamp, d.Data);
                        itms.Add(mse);
                    }
                    MeansSamplesData.AddRange(itms);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Error adding means full event: {e.Message}");
                }
                finally
                {
                    LockSlim.ExitWriteLock();
                }
            }
        }
    }
}