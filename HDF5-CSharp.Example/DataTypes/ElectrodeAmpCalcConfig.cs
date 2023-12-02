using System;
using System.Collections.Generic;
using System.Text;

namespace HDF5CSharp.Example.DataTypes
{
    public class ElectrodeAmpCalcConfig
    {
        public double BF_Limit { get; set; }
        public double CF_Limit { get; set; }
        public double CurrentAt_0_4_BF { get; set; }
        public double CurrentAt_0_4_CF { get; set; }
    }
}