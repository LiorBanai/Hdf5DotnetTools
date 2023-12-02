using HDF5CSharp.DataTypes;
using Microsoft.Extensions.Logging;

namespace HDF5CSharp.Example.DataTypes
{
    [Hdf5GroupName("calibrations")]
    public class CalibrationGroup : Hdf5BaseFile
    {
        [Hdf5EntryName("system_new_hw_path")]
        public string SystemNewHWPath { get; set; }
        [Hdf5EntryName("system_new_hw_content")]
        public string SystemNewHWContent { get; set; }
        [Hdf5EntryName("patch_box_calibration_Path")]
        public string PatchBoxCalibrationPath { get; set; }
        [Hdf5EntryName("patch_box_calibration_content")]
        public string PatchBoxCalibrationContent { get; set; }

        public CalibrationGroup(in long fileId, in long groupRoot, ILogger logger) : base(fileId, groupRoot, "calibrations", logger)
        {
        }

        public void AddCalibrationsData(CalibrationsSystemInformation calibrationsSystemInformation)
        {
            SystemNewHWPath = calibrationsSystemInformation.SystemNewHWPath;
            SystemNewHWContent = calibrationsSystemInformation.SystemNewHWContent;
            PatchBoxCalibrationPath = calibrationsSystemInformation.PatchBoxCalibrationPath;
            PatchBoxCalibrationContent = calibrationsSystemInformation.PatchBoxCalibrationContent;
            int fileNum = 0;
            foreach ((string filePath, string fileContent) in calibrationsSystemInformation.Configurations)
            {
                string key = $"file {fileNum++}";
                Hdf5.WriteStrings(GroupId, key, new[] { filePath, fileContent });
                Hdf5.WriteAttribute(GroupId, key, "first entry is file name. Second is content");
            }
        }
    }
}