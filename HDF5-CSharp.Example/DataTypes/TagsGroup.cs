using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5GroupName("tags")]
    public class TagsGroup : Hdf5BaseFile, IDisposable
    {
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private List<string> TagsData { get; set; }
        [Hdf5ReadWrite(Hdf5ReadWrite.DoNothing)] private bool record;
        public TagsGroup(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "tags", logger)
        {
            TagsData = new List<string>();
        }

        public void Enqueue(string tagJson)
        {
            if (record)
            {
                TagsData.Add(string.IsNullOrEmpty(tagJson) ? "[]" : tagJson);
            }
        }

        public Task WaitForDataWritten()
        {
            record = false;
            Logger?.LogInformation("Start write of Tags");
            var status = Hdf5.WriteObject(GroupId, TagsData.ToArray());
            Logger?.LogInformation("End write of Tags with status " + status);
            return Task.CompletedTask;
        }

        public void StopRecording() => record = false;

        public void StartLogging() => record = true;
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