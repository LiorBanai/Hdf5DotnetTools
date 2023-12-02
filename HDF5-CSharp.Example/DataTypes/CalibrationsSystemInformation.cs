using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HDF5CSharp.Example.DataTypes
{
    [Serializable]
    public class CalibrationsSystemInformation
    {
        public string SystemNewHWPath { get; set; }
        public string SystemNewHWContent { get; set; }
        public string PatchBoxCalibrationPath { get; set; }
        public string PatchBoxCalibrationContent { get; set; }
        public List<(string FilePath, string FileContent)> Configurations { get; set; }

        public CalibrationsSystemInformation()
        {
            Configurations = new List<(string FilePath, string FileContent)>();
            SystemNewHWContent = string.Empty;
            SystemNewHWPath = string.Empty;
            PatchBoxCalibrationPath = string.Empty;
            PatchBoxCalibrationContent = string.Empty;
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
        public static CalibrationsSystemInformation FromJson(string calibrationPath)
            => JsonConvert.DeserializeObject<CalibrationsSystemInformation>(calibrationPath);
    }
}