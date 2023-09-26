using HDF.PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HDF5CSharp.DataTypes;
using System.Text.RegularExpressions;
using HDF5CSharp.Helpers;

namespace HDF5CSharp
{
    public struct OffsetInfo
    {
        public string name;
        public int offset;
        public int size;
        public Type type;
        public long datatype;
        public string displayName;
    }


    public static partial class Hdf5
    {
        private static readonly IEnumerable<TypeCode> primitiveTypes = Enum.GetValues(typeof(TypeCode)).Cast<TypeCode>().Except(new[] { TypeCode.Empty, TypeCode.DBNull, TypeCode.Object });

        public static T fromBytes<T>(byte[] arr)
        {
            T objectData = Activator.CreateInstance<T>();

            int size = Marshal.SizeOf(objectData);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            objectData = (T)Marshal.PtrToStructure(ptr, objectData.GetType());
            Marshal.FreeHGlobal(ptr);

            return objectData;
        }

        public static byte[] GetBytes<T>(T strct)
        {
            int size = Marshal.SizeOf(strct);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(strct, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
        /// <summary>
        /// Opens a Hdf-5 file
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="readOnly">Wether or not to open readonly</param>
        /// <param name="attemptShortPath">Whether or not to attempt to correct path to windows 8.3 short path form (ASCII-safe)</param>
        /// <returns>File Id</returns>
        /// <exception cref="ArgumentNullException">When filename is null or whitespace</exception>
        /// <exception cref="ArgumentOutOfRangeException">When filename contains illegal characters.</exception>
        /// <exception cref="InvalidOperationException">When attemptShortPath is set to true on a non-windows machine</exception>
        /// <exception cref="FileNotFoundException">If the filename points to a non-existing file, and attemptShortPath is set to true</exception>
        public static long OpenFile(string filename, bool readOnly = false, bool attemptShortPath = false)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException(nameof(filename), "Argument cannot be null, empty or whitespace.");
            }
            if (attemptShortPath)
            {
                filename = filename.ToShortPath();
            }
            else
            {
                if (IllegalCharacterValidator.IsMatch(filename))
                {
                    throw new ArgumentOutOfRangeException(nameof(filename), "Argument contains illegal characters. HDF5.PInvoke cannot handle file paths with non-ascii characters.");
                }
            }
            uint access = (readOnly) ? H5F.ACC_RDONLY : H5F.ACC_RDWR;
            var fileId = H5F.open(filename, access);
            return fileId;
        }

        /// <summary>
        /// Regex that matches characters deemed invalid in file names by HFD.PInvoke library.
        /// </summary>
        private static readonly Regex IllegalCharacterValidator = new("[æøåöäïë€]+", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        /// <summary>
        /// Creates a Hdf-5 file
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>File Id</returns>
        /// <exception cref="ArgumentNullException">When filename is null or whitespace</exception>
        /// <exception cref="ArgumentOutOfRangeException">When filename contains illegal characters.</exception>
        /// <exception cref="InvalidOperationException">When attemptShortPath is set to true on a non-windows machine</exception>
        public static long CreateFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException(nameof(filename), "Argument cannot be null, empty or whitespace.");
            }
            if (IllegalCharacterValidator.IsMatch(filename))
            {
                throw new ArgumentOutOfRangeException(nameof(filename), "Argument contains illegal characters. HDF5.PInvoke cannot handle file paths with non-ascii characters.");
            }
            return H5F.create(filename, H5F.ACC_TRUNC);
        }

        public static long CloseFile(long fileId)
        {
            return H5F.close(fileId);
        }
        public static long Flush(long objectId, H5F.scope_t scope)
        {
            return H5F.flush(objectId, scope);
        }
        //internal static string ToHdf5Name(string name)
        //{
        //    return string.Concat(@"/", name);
        //}


        internal static long GetDatatype(Type type)
        {

            long dataType;

            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Byte:
                    dataType = H5T.NATIVE_UINT8;
                    break;
                case TypeCode.SByte:
                    dataType = H5T.NATIVE_INT8;
                    break;
                case TypeCode.Int16:
                    dataType = H5T.NATIVE_INT16;
                    break;
                case TypeCode.Int32:
                    dataType = H5T.NATIVE_INT32;
                    break;
                case TypeCode.Int64:
                    dataType = H5T.NATIVE_INT64;
                    break;
                case TypeCode.UInt16:
                    dataType = H5T.NATIVE_UINT16;
                    break;
                case TypeCode.UInt32:
                    dataType = H5T.NATIVE_UINT32;
                    break;
                case TypeCode.UInt64:
                    dataType = H5T.NATIVE_UINT64;
                    break;
                case TypeCode.Single:
                    dataType = H5T.NATIVE_FLOAT;
                    break;
                case TypeCode.Double:
                    dataType = H5T.NATIVE_DOUBLE;
                    break;
                //case TypeCode.DateTime:
                //    dataType = H5T.Native_t;
                //    break;
                case TypeCode.Char:
                    //dataType = H5T.NATIVE_UCHAR;
                    dataType = H5T.C_S1;
                    break;
                case TypeCode.String:
                    //dataType = H5T.NATIVE_UCHAR;
                    dataType = H5T.C_S1;
                    break;
                default:
                    if (type.IsValueType)
                    {
                        dataType = CreateType(type);
                        break;
                    }

                    throw new ArgumentOutOfRangeException(type.Name, $"Data Type {type} not supported");
            }
            return dataType;
        }

        internal static long GetDatatypeIEEE(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            long dataType;
            switch (typeCode)
            {
                case TypeCode.Int16:
                    dataType = H5T.STD_I16BE;
                    break;
                case TypeCode.Int32:
                    dataType = H5T.STD_I32BE;
                    break;
                case TypeCode.Int64:
                    dataType = H5T.STD_I64BE;
                    break;
                case TypeCode.UInt16:
                    dataType = H5T.STD_U16BE;
                    break;
                case TypeCode.UInt32:
                    dataType = H5T.STD_U32BE;
                    break;
                case TypeCode.UInt64:
                    dataType = H5T.STD_U64BE;
                    break;
                case TypeCode.Single:
                    dataType = H5T.IEEE_F32BE;
                    break;
                case TypeCode.Double:
                    dataType = H5T.IEEE_F64BE;
                    break;
                case TypeCode.Boolean:
                    dataType = H5T.STD_I8BE;
                    break;
                case TypeCode.Char:
                    //dataType = H5T.NATIVE_UCHAR;
                    dataType = H5T.STD_I8BE;
                    break;
                case TypeCode.String:
                    //dataType = H5T.NATIVE_UCHAR;
                    dataType = H5T.C_S1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(type.Name, $"Data Type {type} not supported");
            }
            return dataType;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/9914230/iterate-through-an-array-of-arbitrary-dimension
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static Array ConvertArray<T1, T2>(this Array array, Func<T1, T2> convertFunc)
        {
            // Gets the lengths and lower bounds of the input array
            int[] lowerBounds = new int[array.Rank];
            int[] lengths = new int[array.Rank];
            for (int numDimension = 0; numDimension < array.Rank; numDimension++)
            {
                lowerBounds[numDimension] = array.GetLowerBound(numDimension);
                lengths[numDimension] = array.GetLength(numDimension);
            }

            int[] FirstIndex(Array a) => Enumerable.Range(0, a.Rank).Select(a.GetLowerBound).ToArray();

            int[] NextIndex(Array a, int[] index)
            {
                for (int i = index.Length - 1; i >= 0; --i)
                {
                    index[i]++;
                    if (index[i] <= array.GetUpperBound(i))
                    {
                        return index;
                    }

                    index[i] = array.GetLowerBound(i);
                }

                return null;
            }

            Type type = typeof(T2);
            Array ar = Array.CreateInstance(type, lengths, lowerBounds);
            if (lowerBounds[0] != 0 || lengths[0] != 0)
            {
                for (var index = FirstIndex(array); index != null; index = NextIndex(array, index))
                {
                    var v = (T1)array.GetValue(index);
                    ar.SetValue(convertFunc(v), index);
                }
            }

            return ar;
        }

        private static T[] Convert2DtoArray<T>(T[,] set)
        {
            int rows = set.GetLength(0);
            int cols = set.GetLength(1);
            T[] output = new T[cols * rows];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    output[i * cols + j] = set[j, i];
                }
            }
            return output;
        }

        private static TOut[] Convert2DtoArray<TIn, TOut>(TIn[,] set, Func<TIn, TOut> convert)
        {
            int rows = set.GetLength(0);
            int cols = set.GetLength(1);
            TOut[] output = new TOut[cols * rows];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    output[i * cols + j] = convert(set[j, i]);
                }
            }
            return output;
        }


        public static bool Similar(this IEnumerable<double> first, IEnumerable<double> second, double precision = 1e-2)
        {
            var result = first.Zip(second, (f, s) => Math.Abs(f - s) < precision);
            return result.All(r => r);
        }

        private static (string, bool) CheckAttribute(Attribute[] attributes)
        {
            string alternativeName = "";
            bool mandatoryElement = false;
            foreach (Attribute attr in attributes)
            {
                if (attr is Hdf5EntryNameAttribute nameAttribute)
                {
                    alternativeName = nameAttribute.Name;
                }

                if (attr is Hdf5MandatoryReadElementAttribute mandatoryReadAttribute)
                {
                    mandatoryElement = mandatoryReadAttribute.MandatoryRead ==
                                       Hdf5MandatoryReadElement.MandatoryForRead;
                }
            }
            return (alternativeName, mandatoryElement);
        }
    }
}
