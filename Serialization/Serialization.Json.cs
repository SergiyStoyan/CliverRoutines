//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************


using System;
using System.Text.Json.Serialization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Drawing;

namespace Cliver
{
    /// <summary>
    /// Serialization helpers.
    /// </summary>
    public static partial class Serialization
    {
        /// <summary>
        /// Serialization to JSON.
        /// </summary>
        public static class Json
        {
            public static string Serialize(object o, bool indented = true, bool polymorphic = true, bool ignoreNullValues = true, bool ignoreDefaultValues = false)
            {
                if (o == null)
                    return null;
                var options = new JsonSerializerOptions
                {
                    TypeInfoResolver = new PolymorphicTypeResolver(),
                    WriteIndented = indented,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
                    IncludeFields = true,
                };
                return JsonSerializer.Serialize(o, options);
            }

            public static string Serialize(object o, JsonSerializerOptions  jsonSerializerOptions, bool indented = true)
            {
                if (jsonSerializerOptions == null)
                    return Serialize(o, indented);
                jsonSerializerOptions.WriteIndented = indented;
                return JsonSerializer.Serialize(o, jsonSerializerOptions);
            }

            public static T Deserialize<T>(string json, bool polymorphic = true, bool createNewObjects = true)
            {
                //System.Runtime.Serialization.FormatterServices.GetUninitializedObject();
                return JsonSerializer.Deserialize<T>(json,
                    new JsonSerializerOptions { TypeNameHandling = polymorphic ? TypeNameHandling.Auto : TypeNameHandling.None, ObjectCreationHandling = createNewObjects ? ObjectCreationHandling.Replace : ObjectCreationHandling.Auto }
                    );
            }

            public static T Deserialize<T>(string json, JsonSerializerOptions jsonSerializerOptions)
            {
                return JsonSerializer.Deserialize<T>(json,
                    jsonSerializerOptions
                    );
            }

            public static object Deserialize(Type type, string json, bool polymorphic = true, bool createNewObjects = true)
            {
                return JsonSerializer.Deserialize(json,
                    type,
                    new JsonSerializerOptions { TypeNameHandling = polymorphic ? TypeNameHandling.Auto : TypeNameHandling.None, ObjectCreationHandling = createNewObjects ? ObjectCreationHandling.Replace : ObjectCreationHandling.Auto }
                    );
            }

            public static object Deserialize(Type type, string json, JsonSerializerOptions jsonSerializerOptions)
            {
                return JsonSerializer.Deserialize(json,
                    type,
                    jsonSerializerOptions
                    );
            }

            public static void Save(string file, object o, bool indented = true, bool polymorphic = true, bool ignoreNullValues = true, bool ignoreDefaultValues = false)
            {
                FileSystemRoutines.CreateDirectory(PathRoutines.GetFileDir(file));
                File.WriteAllText(file, Serialize(o, indented, polymorphic, ignoreNullValues, ignoreDefaultValues));
            }

            public static T Load<T>(string file, bool polymorphic = true, bool createNewObjects = true)
            {
                return Deserialize<T>(File.ReadAllText(file), polymorphic, createNewObjects);
            }

            public static object Load(Type type, string file, bool polymorphic = true, bool createNewObjects = true)
            {
                return Deserialize(type, File.ReadAllText(file), polymorphic, createNewObjects);
            }

            public static O Clone<O>(O o, JsonSerializerOptions jsonSerializerOptions = null)
            {
                if (jsonSerializerOptions == null)
                    jsonSerializerOptions = new JsonSerializerOptions
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                    };
                return Deserialize<O>(Serialize(o, jsonSerializerOptions, false));
            }

            public static object Clone(Type type, object o, JsonSerializerOptions jsonSerializerOptions = null)
            {
                if (jsonSerializerOptions == null)
                    jsonSerializerOptions = new JsonSerializerOptions
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                    };
                return Deserialize(type, Serialize(o, jsonSerializerOptions, false));
            }

            public static object Clone2(object o, JsonSerializerOptions jsonSerializerOptions = null)
            {
                if (jsonSerializerOptions == null)
                    jsonSerializerOptions = new JsonSerializerOptions
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                    };
                return Deserialize(o.GetType(), Serialize(o, jsonSerializerOptions, false));
            }

            public static bool IsEqual(object a, object b, JsonSerializerOptions jsonSerializerOptions = null)
            {
                if (jsonSerializerOptions == null)
                    jsonSerializerOptions = new JsonSerializerOptions
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                    };
                return Serialize(a, jsonSerializerOptions, false) == Serialize(b, jsonSerializerOptions, false);
            }

            public static IEnumerable<string> GetMemberNames(object o)
            {
                JObject jo = (JObject)JToken.FromObject(o);
                return jo.Properties().Select(x => x.Name);
            }

            /// <summary>
            /// Usage: [Newtonsoft.Json.JsonConverter(typeof(Serialization.Json.NoIndentConverter))]
            /// !!!Issue: does not work on types
            /// </summary>
            public class NoIndentConverter : JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return true;
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    //if (reader.TokenType == JsonToken.Null)
                    //    return existingValue;
                    //var jObject = Newtonsoft.Json.Linq.JObject.Load(reader);
                    //return jObject.ToObject(objectType);
                    object o = serializer.Deserialize(reader, objectType);
                    if (o == null)
                        return existingValue;
                    return o;
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    writer.WriteRawValue(JsonConvert.SerializeObject(value, Formatting.None));
                }
            }
        }
    }
}