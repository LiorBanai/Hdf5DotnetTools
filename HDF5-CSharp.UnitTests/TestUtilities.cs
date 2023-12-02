using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HDF5CSharp.UnitTests
{
    public static class UtilsExtensions
    {
        /// <summary>
        /// Compares each property of an object with the fields of another object to see if they are the same
        /// </summary>
        /// <typeparam name="T">generic class type</typeparam>
        /// <param name="self">The object to compare</param>
        /// <param name="to">The second object</param>
        /// <param name="ignore">names of fields to ignore</param>
        /// <returns></returns>
        public static bool PublicInstancePropertiesEqual<T>(this T self, T to, params string[] ignore) where T : class
        {
            var equal = false;
            if (self != null && to != null)
            {
                var type = typeof(T);
                var ignoreList = new List<string>(ignore);
                var unequalProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).
                    Where(pi => !ignoreList.Contains(pi.Name));
                foreach (var pi in unequalProperties)
                {
                    var selfValue = type.GetField(pi.Name).GetValue(self);
                    var toValue = type.GetField(pi.Name).GetValue(to);
                    Type selfType = selfValue.GetType();
                    TypeCode code = Type.GetTypeCode(selfType);
                    if (code == TypeCode.DateTime)
                    {
                        if (DateTime.Compare((DateTime)selfValue, (DateTime)toValue) != 0)
                        {
                            return false;
                        }
                    }

                    if (selfType == typeof(TimeSpan))
                    {
                        if (TimeSpan.Compare((TimeSpan)selfValue, (TimeSpan)toValue) != 0)
                        {
                            return false;
                        }
                    }

                    if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                    {
                        return false;
                    }
                }
                equal = true;
            }
            return equal;
        }

        /// <summary>
        /// Compares each field of an object with the fields of another object to see if they are the same
        /// </summary>
        /// <typeparam name="T">generic class type</typeparam>
        /// <param name="self">The object to compare</param>
        /// <param name="to">The second object</param>
        /// <param name="ignore">names of fields to ignore</param>
        /// <returns></returns>
        public static bool PublicInstanceFieldsEqual<T>(this T self, T to, params string[] ignore) where T : class
        {
            var equal = false;
            if (self != null && to != null)
            {
                var type = typeof(T);
                var ignoreList = new List<string>(ignore);
                var unequalProperties = type.GetFields(BindingFlags.Public | BindingFlags.Instance).
                    Where(pi => !ignoreList.Contains(pi.Name));
                foreach (var pi in unequalProperties)
                {
                    var selfValue = type.GetField(pi.Name).GetValue(self);
                    var toValue = type.GetField(pi.Name).GetValue(to);
                    Type selfType = selfValue.GetType();
                    TypeCode code = Type.GetTypeCode(selfType);
                    if (code == TypeCode.DateTime)
                    {
                        if (DateTime.Compare((DateTime)selfValue, (DateTime)toValue) != 0)
                        {
                            return false;
                        }
                    }

                    if (selfType == typeof(TimeSpan))
                    {
                        if (TimeSpan.Compare((TimeSpan)selfValue, (TimeSpan)toValue) != 0)
                        {
                            return false;
                        }
                    }

                    if (selfValue != toValue && (!selfValue.Equals(toValue)))
                    {
                        return false;
                    }
                }
                equal = true;
            }
            return equal;
        }
    }
}