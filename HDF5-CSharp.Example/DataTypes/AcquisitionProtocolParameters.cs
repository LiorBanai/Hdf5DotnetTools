using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace HDF5CSharp.Example.DataTypes
{
    public class AcquisitionProtocolParameters
    {
        // Scan Protocol the scan is based on => change might be apply on top of it
        public string BaseScanProtocolName { get; set; } = string.Empty;
        public ScanDescription ScanDescription { get; set; } = new ScanDescription();

        // Description of the acquisition 
        public string AcquisitionDescription { get; set; } = string.Empty;
        public string RecordingName { get; set; } = string.Empty;
        public string AcquisitionFolder { get; set; } = string.Empty;
        public static AcquisitionProtocolParameters Empty { get; } = new AcquisitionProtocolParameters();

        public AcquisitionProtocolParameters()
        {

        }

        public AcquisitionProtocolParameters(string protocolName, ScanDescription scan)
        {
            BaseScanProtocolName = protocolName;
            ScanDescription = scan;
        }

        public string AsJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static AcquisitionProtocolParameters FromJson(string data)
        {
            return JsonConvert.DeserializeObject<AcquisitionProtocolParameters>(data);
        }


        public ElectrodeAcquisitionProtocolParameters GenerateElectrodeAcquisitionParameters()
        {
            (double[] Amplitude, double[] Frequency, double[] Phases) GenerateArrayData(int size,
                List<(string Channel, double Amplitude, double Frequency, double Phase)> sorted)
            {
                double[] amp = new double[size];
                double[] freq = new double[size];
                double[] phases = new double[size];
                foreach ((string channel, double amplitude, double frequency, double phase) in sorted)
                {
                    int position = int.Parse(channel.Substring(1, channel.Length - 1)) - 1;
                    amp[position] = amplitude;
                    freq[position] = frequency;
                    phases[position] = phase;
                }

                return (amp, freq, phases);
            }


            var ElectrodeParams = new ElectrodeAcquisitionProtocolParameters();

            if (ScanDescription.ElectrodeParams.GroundChannels == null ||
                ScanDescription.ElectrodeParams.GroundChannels.Count == 0)
            {
                throw new ArgumentNullException("Missing ground channel!");
            }

            if (ScanDescription.ElectrodeParams.GroundChannels.Count > 1)
            {
                throw new ArgumentOutOfRangeException("Number of ground channels in kx should be 1!");
            }

            ElectrodeParams.GroundChannel = ScanDescription.ElectrodeParams.GroundChannels[0];

            if (ScanDescription.ElectrodeParams.DebugChannels.Count > 1)
            {
                throw new ArgumentOutOfRangeException("Number of debug channels in kx should be 1!");
            }

            if (ScanDescription.ElectrodeParams.DebugChannels.Count == 1)
            {
                ElectrodeParams.DebugChannel = ScanDescription.ElectrodeParams.DebugChannels[0];
            }

            ElectrodeParams.UseBodySurfacePadCalibration = ScanDescription.ElectrodeParams.UseBodySurfacePadCalibration;
            // Added button ignore - button channels if exist shouldn't be check, always in saturation (hack for now) ///////////
            var buttonDevice = ScanDescription.DevicesDescription.Find(dd => dd.Subtype.ToLower() == "button");
            if (buttonDevice != null)
            {
                ElectrodeParams.IgnoreSaturationChannels = buttonDevice.Channels.ToArray();
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var signals = ScanDescription.Signals.GroupBy(s => s.Channel[0]);
            foreach (IGrouping<char, (string Channel, double Amplitude, double Frequency, double Phase)> group in
                signals)
            {
                var sorted = group.OrderBy(data => int.Parse(data.Channel.Substring(1, data.Channel.Length - 1)))
                    .ToList();
                switch (group.Key)
                {
                    case ElectrodeAcquisitionProtocolParameters.AId:
                        {
                            (double[] amplitude, double[] frequency, double[] phases) =
                                GenerateArrayData(ElectrodeAcquisitionProtocolParameters.ASize, sorted);
                            ElectrodeParams.A_Amplitude = amplitude;
                            ElectrodeParams.A_Frequencies = frequency;
                            ElectrodeParams.A_Phase = phases;
                        }
                        continue;
                    case ElectrodeAcquisitionProtocolParameters.BId:
                        {
                            (double[] amplitude, double[] frequency, double[] phases) =
                                GenerateArrayData(ElectrodeAcquisitionProtocolParameters.BSize, sorted);
                            ElectrodeParams.B_Amplitude = amplitude;
                            ElectrodeParams.B_Frequencies = frequency;
                            ElectrodeParams.B_Phase = phases;
                        }
                        continue;
                    case ElectrodeAcquisitionProtocolParameters.CId:
                        {
                            (double[] amplitude, double[] frequency, double[] phases) =
                                GenerateArrayData(ElectrodeAcquisitionProtocolParameters.CSize, sorted);
                            ElectrodeParams.C_Amplitude = amplitude;
                            ElectrodeParams.C_Frequencies = frequency;
                            ElectrodeParams.C_Phase = phases;

                        }
                        continue;
                    case ElectrodeAcquisitionProtocolParameters.DId:
                        {
                            (double[] amplitude, double[] frequency, double[] phases) =
                                GenerateArrayData(ElectrodeAcquisitionProtocolParameters.DSize, sorted);
                            ElectrodeParams.D_Amplitude = amplitude;
                            ElectrodeParams.D_Frequencies = frequency;
                            ElectrodeParams.D_Phase = phases;

                        }
                        continue;
                    case ElectrodeAcquisitionProtocolParameters.EId:
                        {
                            (double[] amplitude, double[] frequency, double[] phases) =
                                GenerateArrayData(ElectrodeAcquisitionProtocolParameters.ESize, sorted);
                            ElectrodeParams.E_Amplitude = amplitude;
                            ElectrodeParams.E_Frequencies = frequency;
                            ElectrodeParams.E_Phase = phases;

                        }
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return ElectrodeParams;
        }

        public EITProtocolParameters GenerateKalpaElecAcqParams(bool isKalpa, ElectrodeAmpCalcConfig ampCalcConfig)
        {
            if (ScanDescription.ElectrodeParams.GroundChannels == null ||
                ScanDescription.ElectrodeParams.GroundChannels.Count == 0)
            {
                throw new ArgumentNullException("Missing ground channels!");
            }

            var electrodeParams = new EITProtocolParameters
            {
                Grounds = ScanDescription.ElectrodeParams.GroundChannels,
                UseBodySurfacePadCalibration = ScanDescription.ElectrodeParams.UseBodySurfacePadCalibration,
            };
            if (ScanDescription.ElectrodeParams.DebugChannels == null ||
                ScanDescription.ElectrodeParams.DebugChannels.Count == 0)
            {
                electrodeParams.DebuggingChannel1 = electrodeParams.DebuggingChannel2 = string.Empty;
            }
            else if (ScanDescription.ElectrodeParams.DebugChannels.Count == 1)
            {
                electrodeParams.DebuggingChannel1 = ScanDescription.ElectrodeParams.DebugChannels[0];
                electrodeParams.DebuggingChannel2 = string.Empty;
            }
            else if (ScanDescription.ElectrodeParams.DebugChannels.Count == 2)
            {
                electrodeParams.DebuggingChannel1 = ScanDescription.ElectrodeParams.DebugChannels[0];
                electrodeParams.DebuggingChannel2 = ScanDescription.ElectrodeParams.DebugChannels[1];
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    $"Number of debug channels is {ScanDescription.ElectrodeParams.DebugChannels.Count} and should be 2 maximum!");
            }

            // Added button ignore - button channels if exist shouldn't be check, always in saturation (hack for now) ///////////
            var buttonDevice = ScanDescription.DevicesDescription.Find(dd => dd.Subtype.ToLower() == "button");
            if (buttonDevice != null)
            {
                electrodeParams.IgnoreSaturationChannels = buttonDevice.Channels.ToArray();
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Take first in order the BS signals
            var bsSignals = ScanDescription.BodySensorDescription.Signals.GroupBy(
                s => Regex.Match(s.Channel, "([a-zA-Z]+)").Value);
            // Then take other signals (catheters, cardiac devices)
            var otherSignals = ScanDescription.DevicesDescription.SelectMany(d => d.Signals).GroupBy(
                s => Regex.Match(s.Channel, "([a-zA-Z]+)").Value);
            //var signals = ScanDescription.Signals.GroupBy(s => Regex.Match(s.Channel, "([a-zA-Z]+)").Value);
            // Unite all signals
            var signals = bsSignals.Union(otherSignals);
            foreach (IGrouping<string, (string Channel, double Amplitude, double Frequency, double Phase)> group in
                signals)
            {
                var sorted = group.OrderBy(data => int.Parse(Regex.Match(data.Channel, @"\d+").Value))
                    .ToList();

                // Temporary hack (hopeuflly:) ) - in Kalpa Bs1,Bs1 channels are switched in HW
                SwapBS1AndBS2Channels(sorted);

                foreach (var elecData in sorted)
                {
                    //var chIdx = int.Parse(elecData.Channel.Substring(1, elecData.Channel.Length - 1));
                    var chIdx = Regex.Match(elecData.Channel, @"\d+").Value;
                    electrodeParams.ElectrodesParams.Add(new PAQElectrodeParams()
                    {
                        ElectrodeID = $"{group.Key}{chIdx}",
                        Frequency = (int)elecData.Frequency,
                        Phase = (float)elecData.Phase,
                        // Currently calculate amplitude only on kx
                        VoltageAmplitudeMilliVolt = isKalpa
                            ? (float)CalculateAmpKalpa(elecData.Channel, elecData.Frequency, elecData.Amplitude)
                            : (float)CalculateAmp(ampCalcConfig, elecData.Channel, elecData.Frequency,
                                elecData.Amplitude)
                    });
                }
            }

            return electrodeParams;
        }

        private void SwapBS1AndBS2Channels(
            List<(string Channel, double Amplitude, double Frequency, double Phase)> channelsList)
        {
            int bs1Index = channelsList.FindIndex(t => t.Channel == "BS1");
            int bs2Index = channelsList.FindIndex(t => t.Channel == "BS2");
            if (bs1Index >= 0 && bs2Index >= 0)
            {
                var tmp = channelsList[bs1Index];
                channelsList[bs1Index] = channelsList[bs2Index];
                channelsList[bs2Index] = tmp;
            }
        }

        private double CalculateAmp(ElectrodeAmpCalcConfig ampCalcConfig, string chName, double freq, double amp)
        {
            double limit = ampCalcConfig.CF_Limit;
            double amplitudeScalingFactor = 1;
            double currentAt_0_4 = ampCalcConfig.CurrentAt_0_4_CF;
            // need to check if it is a BF or CF Channel and execute the formula
            //if (std::find(configBFChannels.begin(), configBFChannels.end(), i + 1) != configBFChannels.end())
            var bfChannels = new List<string>() { "A1", "A2", "A3", "A4", "A5", "A6", "B6" };
            if (bfChannels.Contains(chName))
            {
                // BF
                limit = ampCalcConfig.BF_Limit;
                amplitudeScalingFactor = 1;
                currentAt_0_4 = ampCalcConfig.CurrentAt_0_4_BF;
            }

            double power = (-20 * (Math.Log10(freq) - Math.Log10(Math.Pow(10, 4))) - 19.5) / 20;
            double I = limit / Math.Pow(10, power);
            return amp * I / currentAt_0_4 * 0.4 * amplitudeScalingFactor;
        }

        private double CalculateAmpKalpa(string chName, double freq, double amp)
        {
            double usedFreq = Math.Max(10000.0, freq);
            const double registorGainBF = 60000;
            const double registorGainCF = 100000;

            var currentValue = amp * (8e-6 / (Math.Pow(10.0,
                                          ((-20.0 * (Math.Log10(usedFreq) - Math.Log10(Math.Pow(10, 4))) - 19.5) / 20)))) * Math.Sqrt(2.0);


            // Conversion from current in uA to Voltage in DAC
            var dacVoltage = currentValue /*/ 1000000.0 /* uA to A (Amper)*/ / 25.0;
            if (chName.Contains("BS"))
            {
                dacVoltage *= registorGainBF;
            }
            else
            {
                dacVoltage *= registorGainCF;
            }

            return dacVoltage;
        }
    }
    public class EITProtocolParameters
    {
        public List<PAQElectrodeParams> ElectrodesParams { get; set; }

        // “B6”, “BS6” 
        public List<string> Grounds { get; set; }

        public bool UseBodySurfacePadCalibration { get; set; }

        // “” => 
        public string DebuggingChannel1 { get; set; }

        // “” => string.empty => no debugging
        public string DebuggingChannel2 { get; set; }

        // TODO: not active
        public bool UseCableCalibration { get; set; }

        // TODO: not active
        public List<string> TestChannels { get; set; }

        public List<string> CrossTalkChannels { get; set; }

        // For now, will supported only on kx
        public string[] IgnoreSaturationChannels { get; set; }

        public EITProtocolParameters()
        {
            ElectrodesParams = new List<PAQElectrodeParams>();
            Grounds = new List<string>();
            UseBodySurfacePadCalibration = false;
            DebuggingChannel1 = string.Empty;
            DebuggingChannel2 = string.Empty;
            IgnoreSaturationChannels = null;
        }

        public string AsJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static EITProtocolParameters FromJson(string data)
        {
            return JsonConvert.DeserializeObject<EITProtocolParameters>(data);
        }
    }

    public class PAQElectrodeParams
    {
        // string is the connector + channel number such as : BS1, A5, B6, etc…
        public string ElectrodeID { get; set; }

        // multiple frequency is supported by design but no need to implement logic
        public int Frequency { get; set; }

        public float VoltageAmplitudeMilliVolt { get; set; }

        // phase in radian
        public float Phase { get; set; }

        public string AsJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static PAQElectrodeParams FromJson(string data)
        {
            return JsonConvert.DeserializeObject<PAQElectrodeParams>(data);
        }
    }

    public class ElectrodeAcquisitionProtocolParameters
    {
        public const char AId = 'A';
        public const char BId = 'B';
        public const char CId = 'C';
        public const char DId = 'D';
        public const char EId = 'E';
        public const int ASize = 12;
        public const int BSize = 6;
        public const int CSize = 20;
        public const int DSize = 20;
        public const int ESize = 6;
        public bool UseAmplitudeFormula { get; set; }

        public double[] A_Amplitude { get; set; }

        public double[] A_Frequencies { get; set; }

        public double[] A_Phase { get; set; }

        public double[] B_Amplitude { get; set; }

        public double[] B_Frequencies { get; set; }

        public double[] B_Phase { get; set; }

        public double[] C_Amplitude { get; set; }

        public double[] C_Frequencies { get; set; }

        public double[] C_Phase { get; set; }

        public double[] D_Amplitude { get; set; }

        public double[] D_Frequencies { get; set; }

        public double[] D_Phase { get; set; }

        public double[] E_Amplitude { get; set; }

        public double[] E_Frequencies { get; set; }

        public double[] E_Phase { get; set; }

        public float SampleRate { get; set; }

        public string CalibrationFile { get; set; }

        public int MaxSecondsToAcquire { get; set; }

        public string GroundChannel { get; set; }

        public int NumOfSamplesPerFrame { get; set; }

        public double AmplitudeScalingFactorBF { get; set; }

        public double AmplitudeScalingFactorCF { get; set; }

        public int ActiveChannels { get; set; }

        public int[] BodySurfacePads { get; set; }

        public bool UseBodySurfacePadCalibration { get; set; }

        public string[] IgnoreSaturationChannels { get; set; }

        public string DebugChannel { get; set; }

        public ElectrodeAcquisitionProtocolParameters()
        {
            // by default acquisition time is infinite and only stop can stop it
            MaxSecondsToAcquire = 0;

            GroundChannel = string.Empty;

            NumOfSamplesPerFrame = 2; // 2.5ms sampling

            AmplitudeScalingFactorBF = 1;
            AmplitudeScalingFactorCF = 1;
            UseAmplitudeFormula = true;
            UseBodySurfacePadCalibration = true;
            A_Amplitude = new double[ASize];
            A_Frequencies = Enumerable.Range(0, ASize).Select(i => double.MaxValue).ToArray();
            A_Phase = Enumerable.Range(0, ASize).Select(i => 0.0).ToArray();


            B_Amplitude = new double[BSize];
            B_Frequencies = Enumerable.Range(0, BSize).Select(i => double.MaxValue).ToArray();
            B_Phase = Enumerable.Range(0, BSize).Select(i => 0.0).ToArray();

            C_Amplitude = new double[CSize];
            C_Frequencies = Enumerable.Range(0, CSize).Select(i => double.MaxValue).ToArray();
            C_Phase = Enumerable.Range(0, CSize).Select(i => 0.0).ToArray();

            D_Amplitude = new double[DSize];
            D_Frequencies = Enumerable.Range(0, DSize).Select(i => double.MaxValue).ToArray();
            D_Phase = Enumerable.Range(0, DSize).Select(i => 0.0).ToArray();

            E_Amplitude = new double[ESize];
            E_Frequencies = Enumerable.Range(0, ESize).Select(i => double.MaxValue).ToArray();
            E_Phase = Enumerable.Range(0, ESize).Select(i => 0.0).ToArray();

        }

        public string AsJson() => JsonConvert.SerializeObject(this);
    }

    public class ScanDescription
    {
        public MetaData GeneralInformation { get; set; }
        public bool IsKalpa { get; set; } = false;
        public ElectrodeParams ElectrodeParams { get; set; }
        public BodySensorDescription BodySensorDescription { get; set; }
        public List<DeviceDescription> DevicesDescription { get; set; }
        public EcgParams EcgParams { get; set; }

        [JsonIgnore]
        public IEnumerable<(string Channel, double Amplitude, double Frequency, double Phase)> Signals
            => BodySensorDescription.Signals.Union(DevicesDescription.SelectMany(d => d.Signals));

        public ScanDescription()
        {
            ElectrodeParams = new ElectrodeParams();
            BodySensorDescription = new BodySensorDescription();
            DevicesDescription = new List<DeviceDescription>();
            EcgParams = new EcgParams();
        }
        public ScanDescription(bool isKalpa, ElectrodeParams electrodeParams, BodySensorDescription bodySensorDescription, List<DeviceDescription> devicesDescription, EcgParams ecgParams)
        {
            IsKalpa = isKalpa;
            ElectrodeParams = electrodeParams;
            BodySensorDescription = bodySensorDescription;
            DevicesDescription = devicesDescription;
            EcgParams = ecgParams;
        }
        public string AsJson() => JsonConvert.SerializeObject(this);

        public static ScanDescription FromJson(string data) => JsonConvert.DeserializeObject<ScanDescription>(data);

        protected bool Equals(ScanDescription other)
        {
            return Equals(ElectrodeParams, other.ElectrodeParams) &&
                   Equals(BodySensorDescription, other.BodySensorDescription) &&
                   DevicesDescription.SequenceEqual(other.DevicesDescription) &&
                   Equals(EcgParams, other.EcgParams);
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ScanDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ElectrodeParams != null ? ElectrodeParams.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BodySensorDescription != null ? BodySensorDescription.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DevicesDescription != null ? DevicesDescription.GetHashCode() : 0);

                return hashCode;
            }
        }
    }


    [Serializable]
    public class ElectrodeParams
    {
        public int NumOfChannels { get; set; }
        public int ActiveChannels { get; set; }
        public double DefaultAmplitude { get; set; }
        public double DefaultAmplitudeCF { get; set; }
        public double DefaultAmplitudeBF { get; set; }
        public List<string> GroundChannels { get; set; }
        public List<string> DebugChannels { get; set; }
        public bool UseBodySurfacePadCalibration { get; set; }
        // TODO: not active
        public bool UseCableCalibration { get; set; }
        // TODO: not active
        public List<string> TestChannels { get; set; }
        public List<string> CrossTalkChannels { get; set; }

        public ElectrodeParams()
        {
            UseBodySurfacePadCalibration = true;
            GroundChannels = new List<string>();
            DebugChannels = new List<string>();
            UseCableCalibration = false;
            TestChannels = new List<string>();
            CrossTalkChannels = new List<string>();
        }
        public ElectrodeParams(int numOfChannels, int activeChannels, double defaultAmplitude, double defaultAmplitudeCf,
            double defaultAmplitudeBf, List<string> groundChannels, List<string> debugChannels, List<string> testChannels, List<string> crossTalkChannels, bool useCableCalibration)
        {
            NumOfChannels = numOfChannels;
            ActiveChannels = activeChannels;
            DefaultAmplitude = defaultAmplitude;
            DefaultAmplitudeCF = defaultAmplitudeCf;
            DefaultAmplitudeBF = defaultAmplitudeBf;
            GroundChannels = groundChannels;
            DebugChannels = debugChannels;
            TestChannels = testChannels;
            CrossTalkChannels = crossTalkChannels;
            UseCableCalibration = useCableCalibration;
        }

        protected bool Equals(ElectrodeParams other)
        {
            return NumOfChannels == other.NumOfChannels && ActiveChannels == other.ActiveChannels &&
                   DefaultAmplitude.Equals(other.DefaultAmplitude) &&
                   DefaultAmplitudeCF.Equals(other.DefaultAmplitudeCF) &&
                   DefaultAmplitudeBF.Equals(other.DefaultAmplitudeBF) &&
                   GroundChannels.SequenceEqual(other.GroundChannels) &&
                   DebugChannels.SequenceEqual(other.DebugChannels) &&
                   TestChannels.SequenceEqual(other.TestChannels) &&
                   CrossTalkChannels.SequenceEqual(other.CrossTalkChannels) &&
                   UseCableCalibration == other.UseCableCalibration;
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ElectrodeParams)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = NumOfChannels;
                hashCode = (hashCode * 397) ^ ActiveChannels;
                hashCode = (hashCode * 397) ^ DefaultAmplitude.GetHashCode();
                hashCode = (hashCode * 397) ^ DefaultAmplitudeCF.GetHashCode();
                hashCode = (hashCode * 397) ^ DefaultAmplitudeBF.GetHashCode();
                hashCode = (hashCode * 397) ^ (GroundChannels != null ? GroundChannels.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DebugChannels != null ? DebugChannels.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TestChannels != null ? TestChannels.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CrossTalkChannels != null ? CrossTalkChannels.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ UseCableCalibration.GetHashCode();
                return hashCode;
            }
        }
    }
    [Serializable]
    public class EcgParams
    {
        public List<string> BS { get; set; }
        public List<string> IC { get; set; }
        public ECGFilterParamsDef ECGFilter { get; set; }
        public bool LeadOffDetection { get; set; }
        public uint SampleRate { get; set; }
        public uint PacketSize { get; set; }

        public EcgParams()
        {

        }
        public EcgParams(List<string> bsChannels, List<string> icChannels, ECGFilterParamsDef ecgFilter, bool leadOffDetection)
        {
            BS = bsChannels;
            IC = icChannels;
            ECGFilter = ecgFilter;
            LeadOffDetection = leadOffDetection;
        }

        protected bool Equals(EcgParams other)
        {
            return BS.SequenceEqual(other.BS) &&
                   IC.SequenceEqual(other.IC) &&
                   ECGFilter == other.ECGFilter &&
                   LeadOffDetection == other.LeadOffDetection;
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((EcgParams)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LeadOffDetection.GetHashCode();
                hashCode = (hashCode * 397) ^ BS.GetHashCode();
                hashCode = (hashCode * 397) ^ IC.GetHashCode();
                hashCode = (hashCode * 397) ^ ECGFilter.GetHashCode();
                return hashCode;
            }
        }
    }

    [Serializable]
    public class MetaData
    {
        public string Version { get; set; }
        public DateTime Time { get; set; }
        public string HostName { get; set; }


        public MetaData(string version)
        {
            Version = version;
            Time = DateTime.Now;
            HostName = Environment.MachineName;
        }

    }
    public class ECGFilterParamsDef
    {
        public float PowerLineNoiseFreq { get; set; }

        public float BS_HPFilterFreq { get; set; }
        public float BS_LPFilterFreq { get; set; }

        public float IC_Unipolar_HPFilterFreq { get; set; }
        public float IC_Unipolar_LPFilterFreq { get; set; }
        public bool IC_UseUnipolarFilter { get; set; }


        public string AsJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static ECGFilterParamsDef FromJson(string data)
        {
            return JsonConvert.DeserializeObject<ECGFilterParamsDef>(data);
        }
    }

    public class BodySensorDescription
    {
        public List<string> Channels { get; set; }
        public List<double> Amplitudes { get; set; }
        public List<double> Frequencies { get; set; }
        public List<double> Phases { get; set; }

        [JsonIgnore]
        public IEnumerable<(string Channel, double Amplitude, double Frequency, double Phases)> Signals
        => Channels.Select((channel, index) => (channel, Amplitudes[index], Frequencies[index], index < Phases.Count ? Phases[index] : 0));


        public BodySensorDescription()
        {
            Channels = new List<string>();
            Amplitudes = new List<double>();
            Frequencies = new List<double>();
            Phases = new List<double>();
        }

        public BodySensorDescription(List<string> channels, List<double> amplitudes, List<double> frequencies)
        {
            Channels = channels;
            Amplitudes = amplitudes;
            Frequencies = frequencies;
            Phases = new List<double>();
        }

        protected bool Equals(BodySensorDescription other)
        {
            return Channels.SequenceEqual(other.Channels) &&
                   Amplitudes.SequenceEqual(other.Amplitudes) &&
                   Frequencies.SequenceEqual(other.Frequencies);
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((BodySensorDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Channels != null ? Channels.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Amplitudes != null ? Amplitudes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Frequencies != null ? Frequencies.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Phases != null ? Frequencies.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class DeviceDescription
    {
        public string Name { get; set; }

        public int NumberOfElectrodes { get; set; }

        public string DeviceImage { get; set; }

        public string ConnectionImage { get; set; }
        public string BoxConnector { get; set; }

        public string Type { get; set; }

        public string Subtype { get; set; }


        public List<string> Channels { get; set; }

        public List<double> Amplitudes { get; set; }

        public List<double> Frequencies { get; set; }
        public List<double> Phases { get; set; }

        public string ID;

        public float[] Color; // RGB 

        public float ElectrodeSize;



        public IEnumerable<(string Channel, double Amplitude, double Frequency, double phases)> Signals
            => Channels.Select((channel, index) => (channel, Amplitudes[index], Frequencies[index], index < Phases.Count ? Phases[index] : 0));


        public DeviceDescription()
        {
            Name = string.Empty;
            DeviceImage = string.Empty;
            ConnectionImage = string.Empty;
            BoxConnector = string.Empty;
            Type = string.Empty;
            Subtype = string.Empty;
            Channels = new List<string>();
            Amplitudes = new List<double>();
            Frequencies = new List<double>();
            Phases = new List<double>();
        }

        public DeviceDescription(string name, int numberOfElectrodes, string deviceImage, string connectionImage, string type, string subtype,
            List<string> channels, List<double> amplitudes, List<double> frequencies, List<double> phases)
        {
            Name = name;
            NumberOfElectrodes = numberOfElectrodes;
            DeviceImage = deviceImage;
            ConnectionImage = connectionImage;
            Type = type;
            Subtype = subtype;
            Channels = channels;
            Amplitudes = amplitudes;
            Frequencies = frequencies;
            Phases = phases;
        }

        protected bool Equals(DeviceDescription other)
        {
            return Name == other.Name && NumberOfElectrodes == other.NumberOfElectrodes &&
                   DeviceImage == other.DeviceImage && ConnectionImage == other.ConnectionImage && Type == other.Type &&
                   Subtype == other.Subtype && Channels.SequenceEqual(other.Channels) &&
                   Amplitudes.SequenceEqual(other.Amplitudes) && Frequencies.SequenceEqual(other.Frequencies) &&
                   Phases.SequenceEqual(other.Phases);
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((DeviceDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ NumberOfElectrodes;
                hashCode = (hashCode * 397) ^ (DeviceImage != null ? DeviceImage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConnectionImage != null ? ConnectionImage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Subtype != null ? Subtype.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Channels != null ? Channels.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Amplitudes != null ? Amplitudes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Frequencies != null ? Frequencies.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Phases != null ? Phases.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
