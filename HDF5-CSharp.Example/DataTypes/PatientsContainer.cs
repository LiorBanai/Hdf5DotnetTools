using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HDF5CSharp.Example.DataTypes
{
    [Serializable]
    [Hdf5GroupName("patients")]
    public class PatientsContainer : Hdf5BaseFile, IDisposable, IEquatable<PatientsContainer>
    {
        [Hdf5EntryName("ManyPatients")] public List<Patient[]> Patients { get; set; }
        [Hdf5EntryName("ManyPatientsField")] public List<Patient[]> PatientsField;
        public PatientsContainer()
        {
        }
        public PatientsContainer(long fileId, long groupRoot, ILogger logger) : base(fileId, groupRoot, "patients",
            logger)
        {
            Patients = new List<Patient[]>();
            Patients.Add(new Patient[]
            {
                new Patient()
                {
                    Age = 20, ExamDate = DateTime.Now.AddDays(-10), FirstName = "first1", Gender = "F", Height = 1.65,
                    Id = "000", LastName = "last1", Weight = 60,
                },
                new Patient()
                {
                    Age = 20, ExamDate = DateTime.Now.AddDays(-10), FirstName = "first2", Gender = "M", Height = 1.85,
                    Id = "111", LastName = "last2", Weight = 90,
                },
            });

            PatientsField = new List<Patient[]>();
            PatientsField.Add(new Patient[]
            {
                new Patient()
                {
                    Age = 20, ExamDate = DateTime.Now.AddDays(-10), FirstName = "first1", Gender = "F", Height = 1.65,
                    Id = "000", LastName = "last1", Weight = 60,
                },
                new Patient()
                {
                    Age = 20, ExamDate = DateTime.Now.AddDays(-10), FirstName = "first2", Gender = "M", Height = 1.85,
                    Id = "111", LastName = "last2", Weight = 90,
                },
            });
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

        public bool Equals(PatientsContainer other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            for (var i = 0; i < Patients.Count; i++)
            {
                var p = Patients[i];
                if (!p.SequenceEqual(other.Patients[i]))
                {
                    return false;
                }
            }
            for (var i = 0; i < PatientsField.Count; i++)
            {
                var p = PatientsField[i];
                if (!p.SequenceEqual(other.PatientsField[i]))
                {
                    return false;
                }
            }
            return true;
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

            return Equals((PatientsContainer)obj);
        }

        public override int GetHashCode() => (Patients != null ? Patients.GetHashCode() : 0);
    }
}