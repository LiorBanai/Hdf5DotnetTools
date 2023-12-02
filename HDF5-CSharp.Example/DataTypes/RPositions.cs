using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
namespace HDF5CSharp.Example.DataTypes
{
    [Serializable]
    [MessagePackObject()]
    public class RPositionsMessagePack
    {
        [Key("timestamp")] public ulong Timestamp { get; set; }
        [Key("nav")] public List<NavigationData> NavigationData { get; set; }

        public RPositionsMessagePack()
        {
            NavigationData = new List<NavigationData>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RPositionsMessagePack Deserialize(byte[] data)
        {
            try
            {
                return MessagePackSerializer.Deserialize<RPositionsMessagePack>(data);
            }
            catch (Exception e)
            {
                string path = $@"RPosition_Error_{DateTime.Now.Ticks}.bin";
                File.WriteAllBytes(path, data);
                var fallback = RPositionsOld.Deserialize(data);
                RPositionsMessagePack msg = new RPositionsMessagePack();
                msg.Timestamp = fallback.Timestamp;
                msg.NavigationData = new List<NavigationData>(fallback.Data.Count);
                foreach (var rpos in fallback.Data)
                {
                    msg.NavigationData.Add(new NavigationData { Name = rpos.Name, Points = rpos.Points.ToList() });
                }
                return msg;
            }
        }

        public void AddNavigationData(NavigationData navData) => NavigationData.Add(navData);

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
    [Serializable]
    [MessagePackObject()]
    public class NavigationData
    {
        [Key("name")] public string Name { get; set; }
        [Key("position")] public List<Vector3> Points { get; set; }
        [Key("trajectory")] public Vector3 Trajectory { get; set; }

        public NavigationData()
        {
            Points = new List<Vector3>();
        }

        public void AddPoint(float x, float y, float z) => Points.Add(new Vector3(x, y, z));
    }


    [Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    internal class RPositionsOld
    {
        [Key("timestamp")] internal ulong Timestamp { get; set; }

        [Key("nav")] //keys are coming from Python code
        //public Dictionary<string, float[][]> Data { get; set; }
        internal List<RpositionsDataPoints> Data;

        [IgnoreMember] public DateTime Time => DateTime.FromBinary(Convert.ToInt64(Timestamp));

        //[IgnoreMember]
        //public string AsJson => JsonConvert.SerializeObject(this);
        internal static RPositionsOld Deserialize(byte[] data) =>
            TranslateDictionary(MessagePackSerializer.Deserialize<Dictionary<object, object>>(data));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RPositionsOld TranslateDictionary(Dictionary<object, object> data)
        {
            IEnumerable<(string Key, object Value)> IterateOver(KeyValuePair<object, object> item)
            {
                var key = string.Empty;
                if (item.Key is byte[] bytes) //pre python 3 it is byte array. can be removed post moving to 3
                {
                    key = Encoding.Default.GetString(bytes);
                }
                else if (item.Key is string value) //python 3 is string
                {
                    key = value;
                }

                object val = item.Value;
                if (val is Dictionary<object, object> dic)
                {
                    List<(string Key, object Value)> innerItems = new List<(string Key, object Value)>(0);
                    foreach (KeyValuePair<object, object> itm in dic)
                    {
                        innerItems.AddRange(IterateOver(itm).ToList());
                    }

                    yield return (key, innerItems);
                }
                else
                {
                    yield return (key, val);
                }
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (KeyValuePair<object, object> itm in data)
            {
                var converted = IterateOver(itm).ToList();
                foreach ((string key, object value) in converted)
                {
                    result.Add(key, value);
                }
            }

            RPositionsOld p = new RPositionsOld();
            p.Timestamp = (ulong)result["timestamp"];
            /////////////////////////////////////////////////////////
            p.Data = (result["nav"] as List<(string, object)>)
                .Select(e => new RpositionsDataPoints(e.Item1, e.Item2 as object[])).ToList();
            return p;
        }
    }

    [MessagePackObject(keyAsPropertyName: false)]
    [Serializable]
    internal class RpositionsDataPoints
    {
        [Key("name")] public string Name;
        [Key("points")] public List<Vector3> Points;

        public RpositionsDataPoints()
        {
        }

        public RpositionsDataPoints(string name, object[] points)
        {
            Name = name;
            Points = new List<Vector3>();
            foreach (object[] p in points)
            {
                Points.Add(new Vector3(Convert.ToSingle(p[0]), Convert.ToSingle(p[1]), Convert.ToSingle(p[2])));
            }
        }

        public RpositionsDataPoints(string name, List<Vector3> points)
        {
            Name = name;
            Points = points;
        }
    }
}