using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using System.Linq;

namespace Cores.Helpers
{ 
    public static class ClassHelper
    {
        public static DateTime MyDefautDate = new DateTime(1900, 01, 01);
        public static bool IsPrimitive(System.Type type)
        {
            if (type == typeof(String) || type == typeof(DateTime)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static TResult GetPropertiesData<TResult>(dynamic fromObject, string propertyName)
        {
            if (fromObject == null) return default(TResult);

            //initialising s with the same values as 
            foreach (PropertyInfo property in fromObject.GetType().GetProperties())
            {
                if (property.Name == propertyName)
                {
                    return property.GetValue(fromObject);
                }
            }
            //Not found
            return default(TResult);
        }

        public static void CopyPropertiesDataDateConverted(dynamic source, dynamic dest)
        {
            //Check not null
            if (source == null || dest == null)
            {
                return;
            }

            //type
            var scrType = source.GetType();
            var destType = dest.GetType();

            //Check source or dest is primitive -> Skip
            if (IsPrimitive(scrType) || IsPrimitive(destType))
            {
                return;
            }

            //Not class
            if (!scrType.IsClass || !destType.IsClass)
            {
                return;
            }

            //Copy for class only
            foreach (var srcProperty in source.GetType().GetProperties())
            {
                PropertyInfo destProperty = dest.GetType().GetProperty(srcProperty.Name);
                if (destProperty != null)
                {
                    var valueSrc = srcProperty.GetValue(source, null);
                    //Timestamp->Datetime
                    if (srcProperty.PropertyType.Name == "Timestamp" && destProperty.PropertyType.Name == "DateTime")
                    {
                        if (valueSrc == null)
                        {
                            //Default date
                            destProperty.SetValue(dest, MyDefautDate, null);
                        }
                        else
                        {
                            var scrDate = ((DateTime)valueSrc.ToDateTime()).ToLocalTime();
                            destProperty.SetValue(dest, scrDate, null);
                        }
                    }

                    //Datetime -> Timestamp
                    else if (srcProperty.PropertyType.Name == "DateTime" && destProperty.PropertyType.Name == "Timestamp")
                    {
                        var scrDate = (DateTime)valueSrc;
                        //Minvalue 1/1/1 -> 1000/01/01
                        if (scrDate == DateTime.MinValue)
                        {
                            scrDate = MyDefautDate;
                        }
                        //Local -> Utc
                        scrDate = scrDate.ToUniversalTime();
                        destProperty.SetValue(dest, Timestamp.FromDateTime(scrDate), null);
                    }
                    //BsonObjectId to string
                    else if (valueSrc != null && srcProperty.PropertyType.Name.ToString() == "BsonObjectId"
                                              && destProperty.PropertyType.ToString() == "System.String")
                    {
                        destProperty.SetValue(dest, valueSrc.ToString(), null);
                    }
                    //null BsonObjectId to string
                    else if (valueSrc == null && srcProperty.PropertyType.Name.ToString() == "BsonObjectId"
                                              && destProperty.PropertyType.ToString() == "System.String")
                    {
                        destProperty.SetValue(dest, "", null);
                    }
                    //Google bytes -> .net byte[]
                    else if (valueSrc != null &&
                            srcProperty.PropertyType.ToString() == "Google.Protobuf.ByteString" &&
                            destProperty.PropertyType.ToString() == "System.Byte[]")
                    {
                        destProperty.SetValue(dest, valueSrc.ToByteArray(), null);
                    }
                    //.net byte[] -> Google bytes
                    else if (valueSrc != null &&
                            srcProperty.PropertyType.ToString() == "System.Byte[]" &&
                            destProperty.PropertyType.ToString() == "Google.Protobuf.ByteString")
                    {
                        destProperty.SetValue(dest, ByteString.CopyFrom(valueSrc), null);
                    }
                    else
                    {
                        //Only copy for IsPrimitive property && same datatype
                        if (valueSrc != null
                            && IsPrimitive(destProperty.PropertyType)
                            && srcProperty.PropertyType.Name == destProperty.PropertyType.Name)
                        {
                            destProperty.SetValue(dest, valueSrc, null);
                        }
                    }
                }
            }
        }

        public static void CopyPropertiesData(dynamic source, dynamic dest)
        {
            //Check not null
            if (source == null || dest == null)
            {
                return;
            }

            //type
            var scrType = source.GetType();
            var destType = dest.GetType();

            //Check source or dest is primitive -> Skip
            if (IsPrimitive(scrType) || IsPrimitive(destType))
            {
                return;
            }

            //Not class
            if (!scrType.IsClass || !destType.IsClass)
            {
                return;
            }

            //Copy for class only
            foreach (var srcProperty in source.GetType().GetProperties())
            {
                PropertyInfo destProperty = dest.GetType().GetProperty(srcProperty.Name);
                if (destProperty != null)
                {
                    var valueSrc = srcProperty.GetValue(source, null);
                    //Timestamp->Datetime or Timestamp->Timestamp
                    if (srcProperty.PropertyType.Name == "Timestamp" && destProperty.PropertyType.Name == "DateTime")
                    {
                        if (valueSrc == null)
                        {
                            //Default date
                            destProperty.SetValue(dest, MyDefautDate, null);
                        }
                        else
                        {
                            var scrDate = ((DateTime)valueSrc.ToDateTime());
                            destProperty.SetValue(dest, scrDate, null);
                        }
                    }

                    //Datetime -> Timestamp
                    else if (srcProperty.PropertyType.Name == "DateTime" && destProperty.PropertyType.Name == "Timestamp")
                    {
                        var scrDate = (DateTime)valueSrc;
                        //Minvalue 1/1/1 -> 1000/01/01
                        if (scrDate == DateTime.MinValue)
                        {
                            scrDate = MyDefautDate;
                        }
                        destProperty.SetValue(dest, Timestamp.FromDateTime(scrDate), null);
                    }
                    //BsonObjectId to string
                    else if (valueSrc != null && srcProperty.PropertyType.Name.ToString() == "BsonObjectId"
                                              && destProperty.PropertyType.ToString() == "System.String")
                    {
                        destProperty.SetValue(dest, valueSrc.ToString(), null);
                    }
                    //null BsonObjectId to string
                    else if (valueSrc == null && srcProperty.PropertyType.Name.ToString() == "BsonObjectId"
                                              && destProperty.PropertyType.ToString() == "System.String")
                    {
                        destProperty.SetValue(dest, "", null);
                    }
                    //Google bytes -> .net byte[]
                    else if (valueSrc != null &&
                            srcProperty.PropertyType.ToString() == "Google.Protobuf.ByteString" &&
                            destProperty.PropertyType.ToString() == "System.Byte[]")
                    {
                        destProperty.SetValue(dest, valueSrc.ToByteArray(), null);
                    }
                    //.net byte[] -> Google bytes
                    else if (valueSrc != null &&
                            srcProperty.PropertyType.ToString() == "System.Byte[]" &&
                            destProperty.PropertyType.ToString() == "Google.Protobuf.ByteString")
                    {
                        destProperty.SetValue(dest, ByteString.CopyFrom(valueSrc), null);
                    }
                    //Google bytes
                    else if (valueSrc != null &&
                            srcProperty.PropertyType.ToString() == "Google.Protobuf.ByteString" &&
                            destProperty.PropertyType.ToString() == "Google.Protobuf.ByteString")
                    {
                        destProperty.SetValue(dest, valueSrc, null);
                    }
                    else
                    {
                        //Only copy for IsPrimitive property && same datatype
                        if (valueSrc != null
                            && IsPrimitive(destProperty.PropertyType)
                            && srcProperty.PropertyType.Name == destProperty.PropertyType.Name)
                        {
                            destProperty.SetValue(dest, valueSrc, null);
                        }
                    }
                }
            }
        }

        public static void CopyPropertiesData(DataRow fromDataRow, dynamic toObject)
        {
            //initialising s with the same values as 
            foreach (DataColumn collumn in fromDataRow.Table.Columns)
            {
                PropertyInfo copyProperty = toObject.GetType().GetProperty(collumn.ColumnName);
                if (copyProperty != null)
                {
                    var value = fromDataRow[collumn.ColumnName];
                    copyProperty.SetValue(toObject, value, null);
                }
            }
        }

        public static void CopyPropertiesData(dynamic fromObject, DataRow toDataRow)
        {
            //initialising s with the same values as 
            foreach (var property in fromObject.GetType().GetProperties())
            {
                foreach (DataColumn collumn in toDataRow.Table.Columns)
                {
                    if (collumn.ColumnName == property.Name)
                    {
                        var value = property.GetValue(fromObject, null);

                        if (value.GetType().ToString() == "Google.Protobuf.WellKnownTypes.Timestamp")
                        {
                            toDataRow[collumn.ColumnName] = value.ToDateTime() ?? DBNull.Value;
                        }
                        else
                        {
                            toDataRow[collumn.ColumnName] = value ?? DBNull.Value;
                        }
                    }
                }
            }
        }

        public static void CopyRowData(DataRow srcRow, DataRow destRow)
        {
            //
            foreach (DataColumn collumn in srcRow.Table.Columns)
            {
                destRow[collumn.ColumnName] = srcRow[collumn.ColumnName];
            }
        }

        //public static T CopyObject<T>(T sourceObject)
        //{
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        BinaryFormatter formatter = new BinaryFormatter();
        //        formatter.Serialize(ms, sourceObject);
        //        ms.Position = 0;
        //        return (T)formatter.Deserialize(ms);
        //    }
        //}

        public static List<T> CopyObjectList<T>(this List<T> sourceObjectList)
        {
            string tmpStr = JsonConvert.SerializeObject(sourceObjectList);
            var ret = JsonConvert.DeserializeObject<List<T>>(tmpStr);
            return ret;
        }

        public static T CopyObject<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null");
            return (T)ProcessDeepCopy(obj);
        }

        static object ProcessDeepCopy(object obj)
        {
            if (obj == null)
                return null;
            System.Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }
            else if (type.IsArray)
            {
                System.Type elementType = System.Type.GetType(
                     type.FullName.Replace("[]", string.Empty));
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(ProcessDeepCopy(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }
            else if (type.IsClass)
            {
                object toret = Activator.CreateInstance(obj.GetType());
                FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;
                    field.SetValue(toret, ProcessDeepCopy(fieldValue));
                }
                return toret;
            }
            else
                throw new ArgumentException("Unknown type");
        }

        #region GetValue
        public static string GetString(Object fromObject)
        {
            //Null
            if (fromObject == null)
            {
                return "";
            }
            //DBNull
            if (fromObject is DBNull)
            {
                return "";
            }
            //Has value
            if (fromObject is string)
            {
                return fromObject as string;
            }
            else
            {
                return fromObject.ToString();
            }

        }

        public static dynamic GetValue(System.Type propType, Object propValue)
        {
            //Null DBNull
            if (propValue == null || propValue is DBNull)
            {
                //String
                if (propType == typeof(string))
                {
                    return "";
                }
                //int
                if (propType == typeof(int))
                {
                    return 0;
                }
                //float
                if (propType == typeof(float))
                {
                    return (float)0;
                }
                //decimal
                if (propType == typeof(decimal))
                {
                    return (decimal)0;
                }
                //double
                if (propType == typeof(double))
                {
                    return (double)0;
                }
                //DateTime
                if (propType == typeof(DateTime))
                {
                    return default(DateTime);
                }
            }
            //Has value
            return propValue;
        }

        public static void FixedNullProperties(dynamic dataObject)
        {
            if (dataObject == null) return;

            System.Type type = dataObject.GetType();
            ProcessFixedNull(type, dataObject);
        }

        private static void ProcessFixedNull(System.Type type, object dataObject)
        {
            //
            if (type.IsValueType || type == typeof(string))
            {
                return;
            }
            else if (type.IsArray)
            {
                var array = dataObject as Array;
                for (int i = 0; i < array.Length; i++)
                {
                    var arrItem = array.GetValue(i);
                    System.Type arrItemType = dataObject.GetType().GetElementType(); ;
                    ProcessFixedNull(arrItemType, arrItem);
                }
            }
            else if (dataObject != null && type.IsClass)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    var fieldValue = field.GetValue(dataObject);
                    //Value type
                    if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                    {
                        var fixedValue = GetValue(field.FieldType, fieldValue);
                        field.SetValue(dataObject, fixedValue);
                    }
                    else
                    {
                        //Recursive fixed null for nested class
                        if (fieldValue != null && field.FieldType.IsClass)
                        {
                            ProcessFixedNull(field.FieldType, fieldValue);
                        }
                    }
                }
            }
        }

        #endregion

        #region Convert
        public static void Convert_LocalToUtc(dynamic source)
        {
            //Check not null
            if (source == null) return;
            //type
            var scrType = source.GetType();

            //Check source or dest is primitive -> Skip
            if (IsPrimitive(scrType)) return;

            //Not class
            if (!scrType.IsClass) return;

            //Convert date for class only
            foreach (var srcProperty in source.GetType().GetProperties())
            {
                if (srcProperty.PropertyType.Name.ToString() == "DateTime")
                {
                    var convertedValue = srcProperty.GetValue(source, null);
                    DateTime convertedDate = (DateTime)srcProperty.GetValue(source, null);
                    if (convertedValue != null)
                    {
                        convertedDate = (DateTime)convertedValue();
                        convertedDate = convertedDate.ToUniversalTime();
                        srcProperty.SetValue(source, convertedValue, null);
                    }
                }
            }
        }
        public static byte[] ByteArray_FromByteString(ByteString source)
        {
            return source.ToByteArray();
        }
        public static ByteString ByteString_FromByteArray(byte[] source)
        {
            return ByteString.CopyFrom(source);
        }
        #endregion

    }
}
