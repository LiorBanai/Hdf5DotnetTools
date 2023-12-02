using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;

namespace HDF5CSharp.Example.DataTypes
{
    [Serializable]
    [Hdf5GroupName("patient")]
    public class Patient : Hdf5BaseFile, IDisposable, IEquatable<Patient>
    {
        [Hdf5EntryName("first_name")]
        public string FirstName { get; set; }
        [Hdf5EntryName("last_name")]
        public string LastName { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string Gender { get; set; }
        public float Age { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        [Hdf5EntryName("exam_date")]
        public DateTime ExamDate { get; set; }
        public long StartRecordingTimeStamp { get; set; }

        public Patient(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "patient", logger)
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Id = string.Empty;
            Gender = string.Empty;
        }
        public Patient()
        {
        }

        public void Dispose()
        {
            try
            {
                if (!Disposed)
                {
                    Hdf5.CloseGroup(GroupId);
                    Disposed = true;
                }
            }
            catch (Exception e)
            {
                Logger?.LogError($"Error closing file: {e}");
            }
        }

        public bool Equals(Patient other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return FirstName == other.FirstName && LastName == other.LastName && Id == other.Id &&
                   Gender == other.Gender && Age.Equals(other.Age) && Height.Equals(other.Height) &&
                   Weight.Equals(other.Weight) && ExamDate.EqualsUpToMilliseconds(other.ExamDate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Patient)obj);
        }

        public override int GetHashCode()
        {
            return (FirstName.GetHashCode() * 397) ^ (LastName.GetHashCode() * 397) ^ (Id.GetHashCode() * 397) ^
                   (Gender.GetHashCode() * 397) ^ (Age.GetHashCode() * 397) ^ (Height.GetHashCode() * 397) ^
                   (Weight.GetHashCode() * 397) ^ (ExamDate.GetHashCode() * 397);
        }
    }
}