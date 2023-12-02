using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDF5CSharp.Winforms.Tests
{
    internal class HDF5DataClass
    {
        public double Location { get; set; }
        public byte[] Image { get; set; }

        protected bool Equals(HDF5DataClass other)
        {
            return Location.Equals(other.Location) && Image.SequenceEqual(other.Image);
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

            return Equals((HDF5DataClass)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Location.GetHashCode() * 397) ^ (Image != null ? Image.GetHashCode() : 0);
            }
        }
    }
}