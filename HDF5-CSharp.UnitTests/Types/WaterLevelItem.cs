using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDF5CSharp.UnitTests.Types
{
    [Serializable]
    public class WaterLevelItem
    {
        public string height { get; set; }
        public string trend { get; set; }

        public WaterLevelItem(string height, string trend)
        {
            this.height = height;
            this.trend = trend;
        }
    }
    [Serializable]
    public class WaterLevelFeature
    {
        public string code { get; set; }
        public string name { get; set; }
        public string uomName { get; set; }
        public string fillValue { get; set; }
        public string dataType { get; set; }
        public string lower { get; set; }
        public string upper { get; set; }
        public string closure { get; set; }

        public WaterLevelFeature(string code, string name, string uomName, string fillValue, string dataType, string lower, string upper, string closure)
        {
            this.code = code;
            this.name = name;
            this.uomName = uomName;
            this.fillValue = fillValue;
            this.dataType = dataType;
            this.lower = lower;
            this.upper = upper;
            this.closure = closure;
        }
    }
}