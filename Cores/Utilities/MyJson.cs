using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cores.Utilities
{
    public class MyJson
    {
        ///// <summary>
        ///// Return String From Object
        ///// </summary>
        ///// <param name="entity">Object in model class format</param>
        ///// <returns></returns>
        //public static string ToString<T>(T mdEntity)
        //{
        //    //validation
        //    if (mdEntity == null)
        //    {
        //        return null;
        //    }
        //    //
        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        WriteIndented = true
        //    };
        //    return System.Text.Json.JsonSerializer.Serialize(mdEntity, options);
        //}
        /// <summary>
        /// Serialize to json string using newtonsoft
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public static string ToString(object objValue)
        {
            try
            {
                return JsonConvert.SerializeObject(objValue);
            }
            catch { }
            //
            return "";
        }

        /// <summary>
        /// Return Utf8 byte From Object 
        /// </summary>
        /// <param name="entity">Object in model class format</param>
        /// <returns></returns>
        public static byte[] SerializeToUtf8Bytes<T>(T mdEntity)
        {
            //validation
            if (mdEntity == null)
            {
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            //var json = System.Text.Json.JsonSerializer.Serialize(mdEntity, options);
            //return Encoding.UTF8.GetBytes(json);
            //return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(mdEntity, options);

            var json = JsonConvert.SerializeObject(mdEntity);
            return Encoding.UTF8.GetBytes(json);
        }
        /// <summary>
        /// Return Object From String using System.Text.Json
        /// </summary>
        /// <param name="jsonString">string in Json format</param>
        /// <returns></returns>
        public static T ToObject<T>(string jsonString)
        {
            //validation
            if (jsonString == null)
            {
                return default(T);
            }
            //
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return System.Text.Json.JsonSerializer.Deserialize<T>(jsonString, options);
        }

        /// <summary>
        /// Deserialize with Newtonsoft.Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static T Deserialize<T>(StreamReader sr)
        {
            //validation
            if (sr == null)
            {
                return default(T);
            }
            //
            var serializer = Newtonsoft.Json.JsonSerializer.CreateDefault();
            JsonReader reader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(reader);
        }
        /// <summary>
        /// Deserialize from json string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            //validation
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }
            //
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Serialize object with Newtonsoft.Json
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        public static string Serialize(object objData)
        {
            return JsonConvert.SerializeObject(objData, Formatting.Indented);
        }

        /// <summary>
        /// Return Object From String
        /// </summary>
        /// <param name="jsonString">string in Json format</param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseToDictionary(string jsonString)
        {
            //validation
            if (jsonString == null)
            {
                return null;
            }
            JObject myJObject = JObject.Parse(jsonString);
            return myJObject.ToObject<Dictionary<string, string>>();
        }

        public static string ToKeyPairJsonString(Dictionary<string, string> dic)
        {
            var output = JsonConvert.SerializeObject(dic);
            return output;
        }



    }// end class
}// end name space
