using System;
using System.Collections.Generic;
using System.Text;

namespace HDF5CSharp.Example.DataTypes
{
    public class ElectrodeAmpCalcConfig
    {
        public double BFLimit { get; set; }
        public double CFLimit { get; set; }
        public double CurrentAt04BF { get; set; }
        public double CurrentAt04CF { get; set; }
    }
}