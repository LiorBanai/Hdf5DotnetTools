using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace HDF5CSharp.Example.DataTypes
{
    [Serializable]
    public struct ECGCycleDescription
    {
        //QRS
        public double StartOfQRSAmplitude;//QRS onset amplitude
        public double EndOfQRSAmplitude;  //QRS offset amplitude
        public double RPeakAmplitude;     //R-Peak amplitude

        //P-Peak
        public double StartOfPWaveAmplitude;//P-wave onset amplitude
        public double EndOfPWaveAmplitude;  //P-wave offset amplitude
        public double PPeakAmplitude;       //P-Peak amplitude

        //T-wave
        public double StartOfTWaveAmplitude;//T-wave onset amplitude
        public double EndOfTWaveAmplitude;  //T-wave offset amplitude
        public double TPeakAmplitude;       //T-Peak amplitude

        //Internal
        //A
        public byte IsAPeakExist;//0 - false; >0 - true
        public double APeakAmplitude;//A-peak amplitude [mkV]

        //V
        public byte IsVPeakExist;//0 - false; >0 - true
        public double VPeakAmplitude;//V-peak amplitude [mkV]

        public byte IsLumenMarkerExist;   //0 - false; >0 - true
        public int LumenPosition;        //Lumen market position.

        public long TimestampStartOfQRSInterval;
        public long TimestampEndOfQRSInterval;
        public long TimestampRPeak;
        public long TimestampStartOfPWaveInterval;
        public long TimestampEndOfPWaveInterval;
        public long TimestampPPeak;
        public long TimestampAPeak;
        public long TimestampVPeak;
        public long TimestampTPeak;
        public long CurrentTimestamp;

        public ECGCycleDescription(ECGCycleDescription marshelledData, long currentTimestamp, double samplingRate, long currentIndex, int shiftBS, int shiftIC)
        {
            CurrentTimestamp = currentTimestamp;
            StartOfQRSAmplitude = marshelledData.StartOfQRSAmplitude;
            EndOfQRSAmplitude = marshelledData.EndOfQRSAmplitude;
            RPeakAmplitude = marshelledData.RPeakAmplitude;
            StartOfPWaveAmplitude = marshelledData.StartOfPWaveAmplitude;
            EndOfPWaveAmplitude = marshelledData.EndOfPWaveAmplitude;
            PPeakAmplitude = marshelledData.PPeakAmplitude;
            StartOfTWaveAmplitude = marshelledData.StartOfTWaveAmplitude;
            EndOfTWaveAmplitude = 0;
            TPeakAmplitude = marshelledData.TPeakAmplitude;
            IsAPeakExist = marshelledData.IsAPeakExist;
            APeakAmplitude = marshelledData.APeakAmplitude;
            IsVPeakExist = marshelledData.IsVPeakExist;
            VPeakAmplitude = marshelledData.VPeakAmplitude;
            TimestampStartOfQRSInterval = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftBS);
            TimestampEndOfQRSInterval = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftBS);
            TimestampRPeak = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftBS);
            TimestampStartOfPWaveInterval = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftBS);
            TimestampEndOfPWaveInterval = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftBS);
            TimestampPPeak = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftBS);
            TimestampAPeak = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftIC);
            TimestampVPeak = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftIC);

            //todo: check calculation
            TimestampTPeak = ConvertIndexToTimestamp(0, currentTimestamp, samplingRate, currentIndex, shiftIC);
            IsLumenMarkerExist = marshelledData.IsLumenMarkerExist;
            LumenPosition = marshelledData.LumenPosition;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ConvertIndexToTimestamp(int relevantIndex, long currentTimestamp, double samplingRate, long currentIndex, int shift)
        {
            return (long)(currentTimestamp - (currentIndex - relevantIndex - shift) * samplingRate);
        }

        public string AsJson() => JsonConvert.SerializeObject(this);
        public static ECGCycleDescription FromJson(string jsonData) => JsonConvert.DeserializeObject<ECGCycleDescription>(jsonData);
    }
}