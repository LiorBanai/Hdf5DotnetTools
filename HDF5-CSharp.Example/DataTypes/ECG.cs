using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5GroupName("ecg")]
    public class ECG : Hdf5BaseFile, IDisposable
    {
        [Hdf5EntryName("start_datetime")] public long? StartDateTime { get; set; }
        [Hdf5EntryName("end_datetime")] public long EndDateTime { get; set; }
        [Hdf5EntryName("sampling_rate")] public int SamplingRate { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] public Dictionary<string, string> Parameters { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] public Dictionary<string, string> Header { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private ChunkedDataset<double> UnFiltered { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private ChunkedDataset<double> Filtered { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private ChunkedDataset<long> Timestamps { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private ChunkedDataset<ulong> PacketIds { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private BlockingCollectionQueue<ECGFrame> EcgSamplesData { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private Task EcgTaskWriter { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private int ChunkSize;
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private bool completed;
        public ECG(long fileId, long groupRoot, int chunkSize, int sampleRate, ILogger logger) : base(fileId, groupRoot, "ecg", logger)
        {
            ChunkSize = chunkSize;
            var pool = ArrayPool<ECGFrame>.Shared;
            SamplingRate = sampleRate;
            if (sampleRate == 0)
            {
                logger?.LogCritical("No sample rate was supplied");
            }
            EcgSamplesData = new BlockingCollectionQueue<ECGFrame>();
            Parameters = new Dictionary<string, string>();
            Header = new Dictionary<string, string>();
            UnFiltered = new ChunkedDataset<double>("Signals_unfiltered", GroupId);
            Filtered = new ChunkedDataset<double>("Signals_filtered", GroupId);
            Filtered = new ChunkedDataset<double>("Signals_filtered", GroupId);
            Timestamps = new ChunkedDataset<long>("timestamps", GroupId);
            PacketIds = new ChunkedDataset<ulong>("packetids", GroupId);
            EcgTaskWriter = Task.Factory.StartNew(() =>
            {
                var buffer = pool.Rent(ChunkSize);
                completed = false;
                int count = 0;
                foreach (ECGFrame data in EcgSamplesData.GetConsumingEnumerable())
                {
                    buffer[count++] = data;
                    if (count == ChunkSize)
                    {
                        AppendSample(buffer, chunkSize);
                        count = 0;
                    }
                }
                if (count != 0)
                {
                    AppendSample(buffer, count);
                }
                FlushData();//end of data samples. flush data
                pool.Return(buffer);
            });
        }

        private void AppendSample(ECGFrame[] samples, int length)
        {
            var sampleForSize = samples.First();

            double[,] unFilteredData = new double[length * sampleForSize.FrameData.First().Count, sampleForSize.FrameData.Count];
            double[,] filteredData = new double[length * sampleForSize.FilteredFrameData.First().Count, sampleForSize.FilteredFrameData.Count];
            long[,] timestampData = new long[length * sampleForSize.FilteredFrameData.First().Count, 1];
            ulong[,] packetIdData = samples[0].PacketId == ulong.MaxValue ? null : new ulong[length, 1];
            ulong[,] kalpaClockData = samples[0].KalpaClock == ulong.MaxValue ? null : new ulong[length, 1];

            for (var i = 0; i < length; i++)
            {
                var dataSample = samples[i];
                var rows = dataSample.FrameData.First().Count;
                var columns = dataSample.FrameData.Count;
                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                    {
                        unFilteredData[i * rows + rowIndex, columnIndex] = dataSample.FrameData[columnIndex][rowIndex];
                    }
                }

                rows = dataSample.FilteredFrameData.First().Count;
                columns = dataSample.FilteredFrameData.Count;
                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                    {
                        filteredData[i * rows + rowIndex, columnIndex] = dataSample.FilteredFrameData[columnIndex][rowIndex];
                    }
                }

                int rowsTimestamps = dataSample.FilteredFrameData.First().Count;

                for (int k = 0; k < rowsTimestamps; k++)
                {
                    var date = dataSample.Timestamp;
                    if (SamplingRate > 0)
                    {
                        date = (long)(dataSample.Timestamp + k * 1000.0 / SamplingRate);
                    }
                    timestampData[i * rowsTimestamps + k, 0] = date;
                    EndDateTime = date;
                }
                if (packetIdData != null)
                {
                    packetIdData[i, 0] = dataSample.PacketId;
                }

                if (kalpaClockData != null)
                {
                    kalpaClockData[i, 0] = dataSample.KalpaClock;
                }
            }
            UnFiltered.AppendOrCreateDataset(unFilteredData);
            Filtered.AppendOrCreateDataset(filteredData);
            Timestamps.AppendOrCreateDataset(timestampData);
            if (packetIdData != null)
            {
                PacketIds.AppendOrCreateDataset(packetIdData);
            }
        }

        public void Enqueue(ECGFrame ecgFrame)
        {
            if (!completed)
            {
                if (!StartDateTime.HasValue)
                {
                    StartDateTime = ecgFrame.Timestamp;
                }

                EcgSamplesData.Enqueue(ecgFrame);
            }
        }
        public void CompleteAdding()
        {
            if (completed)
            {
                return;
            }

            completed = true;
            EcgSamplesData.CompleteAdding();
        }

        public async Task WaitForDataWritten()
        {
            CompleteAdding();
            await EcgTaskWriter;
        }

        public void Dispose()
        {
            try
            {
                if (!Disposed)
                {
                    UnFiltered.Dispose();
                    Filtered.Dispose();
                    Timestamps.Dispose();
                    PacketIds?.Dispose();
                    EcgSamplesData.Dispose();
                    EcgTaskWriter.Dispose();
                    Hdf5.CloseGroup(GroupId);
                    Disposed = true;
                }
            }
            catch (Exception e)
            {
                Logger?.LogError($"Error during dispose of ECG: {e.Message}");
            }
        }

        public void AppendEcgCycleDescriptionSample(ECGCycleDescription e)
        {
            Hdf5.WriteStrings(GroupId, "ecg_cycle_description", new List<string> { e.AsJson() });
        }
    }
}