using Newtonsoft.Json;
using System;

namespace HDF5CSharp.Example.DataTypes
{
    public class PatientInfo
    {
        public string PatientFirstName { get; set; }
        public string PatientFamilyName { get; set; }
        public string PatientId { get; set; }
        public int PatientAge { get; set; }
        public string PatientGender { get; set; }
        public string Type { get; set; }
        public float PatientWeight { get; set; }
        public float PatientHeight { get; set; }
        public int YearOfBirth { get; set; }
    }

    /// <summary>
    /// Exam information
    /// </summary>
    public class ProcedureInfo
    {
        public Guid Guid { get; set; }
        /// <summary>
        /// Patient Info
        /// </summary>
        public PatientInfo Patient { get; set; }

        /// <summary>
        /// Date of the Exam
        /// </summary>
        public DateTime ExamDate { get; set; }

        public string Procedure { get; set; }
        public string Institute { get; set; }

        public string GeolocationData { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }


        // Timezone information
        public string LocalTimeZone { get; set; }
        public int UtcOffset { get; set; }
        public bool IsDaylightSaving { get; set; }

        public string Reviewer { get; set; }

        public ProcedureInfo()
        {
            Guid = Guid.NewGuid();
            StartTimeUtc = DateTime.UtcNow;
            Institute = string.Empty;
            GeolocationData = string.Empty;
            Reviewer = string.Empty;
        }

        public ProcedureInfo(PatientInfo patient) : base()
        {
            Patient = patient;
            Procedure = "TAVI";
            Guid = Guid.NewGuid();
            StartTimeUtc = DateTime.UtcNow;
            ExamDate = DateTime.Now;
            Institute = string.Empty;
            GeolocationData = string.Empty;
        }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}