using HDF.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HDF5CSharp.DataTypes
{
    public enum Hdf5ElementType
    {
        Unknown = 0,
        Group,
        Dataset,
        Attribute,
        CommitedDatatype,
    }

    public class Hdf5AttributeElement
    {
        public string Name { get; set; }
        public object Values { get; set; }
        public string ElementType { get; set; }
        public Hdf5AttributeElement(string name, object values, string elementType)
        {
            Name = name;
            Values = values;
            ElementType = elementType;
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Values)}: {string.Join(Environment.NewLine, Values)}, {nameof(ElementType)}: {ElementType}";
        }
    }
    public abstract class Hdf5ElementBase
    {
        public string Name { get; set; }
        public Hdf5ElementType Type { get; set; }

        public Hdf5ElementBase Parent { get; set; }
        public long Id { get; set; }
        protected bool HasLoadedOnce { get; set; }
        public abstract string GetPath();
        public abstract string GetDisplayName();
        public abstract IEnumerable<Hdf5ElementBase> GetChildren();
        protected abstract long GetId(long fileId);
        protected abstract void CloseId(long id);

        protected Hdf5ElementBase(string name, Hdf5ElementType type, Hdf5ElementBase parent, long id)
        {
            Name = name;
            Type = type;
            Parent = parent;
            Id = id;
        }

        public override string ToString() => $"{nameof(Name)}: {Name} ({Type}) ID:{Id}";
    }

    public class Hdf5Element : Hdf5ElementBase
    {
        public List<Hdf5Element> Children { get; private set; }
        public List<Hdf5AttributeElement> Attributes { get; private set; }
        public Hdf5Element(string name, Hdf5ElementType type, Hdf5ElementBase parent, long id) : base(name, type, parent, id)
        {
            Children = new List<Hdf5Element>();
            Attributes = new List<Hdf5AttributeElement>();
        }

        public bool HasChildren => Children.Any();

        public void AddAttribute(string attrName, object value, string type)
        {
            Attributes.Add(new Hdf5AttributeElement(attrName, value, type));
        }
        public override string GetPath()
        {
            return Name;
        }

        public override string GetDisplayName()
        {
            return Name;
        }

        public override IEnumerable<Hdf5ElementBase> GetChildren()
        {
            return Children.ToList();
        }

        protected override long GetId(long fileId)
        {
            return H5G.open(fileId, GetPath());
        }

        protected override void CloseId(long id)
        {
            if (H5I.is_valid(id) > 0)
            {
                H5G.close(id);
            }
        }

        public void AddChild(Hdf5Element child)
        {
            Children.Add(child);
        }

        public Hdf5Element GetChildWithName(string childName)
        {
            var child = Children.FirstOrDefault(c => c.Name == childName);
            if (child != null)
            {
                return child;
            }

            var subChildren = Children.Where(c => c.HasChildren).Select(c => c.GetChildWithName(childName));
            return subChildren.FirstOrDefault();
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Children)}: {Children.Count}, {nameof(Attributes)}: {Attributes.Count}";
        }
    }
}