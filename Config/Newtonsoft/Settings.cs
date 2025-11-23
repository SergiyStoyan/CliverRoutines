//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;

namespace Cliver
{
    abstract public class Newtonsoft_Settings : Cliver.Settings
    {
        [JsonIgnore]
        override public SettingsFieldInfo __Info
        {
            get
            {
                return base.__Info;
            }
            set
            {
                base.__Info = value;
            }
        }

        override internal Settings Deserialize(Type type, string json, bool polymorphic = true, bool createNewObjects = true)
        {
            return (Settings)Serialization.Json.Deserialize(type, json, polymorphic, createNewObjects);
            //return (Settings)JsonConvert.DeserializeObject(json,
            //   type,
            //   new JsonSerializerSettings
            //   {
            //       TypeNameHandling = polymorphic ? TypeNameHandling.Auto : TypeNameHandling.None,
            //       ObjectCreationHandling = createNewObjects ? ObjectCreationHandling.Replace : ObjectCreationHandling.Auto,
            //       ContractResolver = new ContractResolver()
            //   }
            //);
        }
        //public class ContractResolver : DefaultContractResolver
        //{
        //    protected override JsonObjectContract CreateObjectContract(Type objectType)
        //    {
        //        var contract = base.CreateObjectContract(objectType);
        //        if (objectType.IsSubclassOf(typeof(Encrypted<>)))
        //            contract.Converter = (JsonConverter)Activator.CreateInstance(typeof(EncryptedConverter<>).MakeGenericType(objectType.GetGenericArguments()));
        //        return contract;
        //    }
        //}
        //class EncryptedConverter<T> : JsonConverter where T : class
        //{
        //    public override bool CanConvert(Type objectType)
        //    {
        //        return true;
        //    }

        //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //    {
        //        if (objectType == typeof(Encrypted<T>))
        //            objectType = typeof(Newtonsoft_Encrypted<T>);

        //        object o = serializer.Deserialize(reader, objectType);
        //        if (o == null)
        //            return existingValue;
        //        return o;
        //    }

        //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //    {
        //        writer.WriteRawValue(JsonConvert.SerializeObject(value, Formatting.None));
        //    }
        //}

        override internal string Serialize(object o, bool indented = true, bool polymorphic = true, bool ignoreNullValues = true, bool ignoreDefaultValues = false)
        {
            return Serialization.Json.Serialize(o, indented, polymorphic, ignoreNullValues, ignoreDefaultValues);
        }

        override internal bool IsEqual(object a, object b)
        {
            return Serialization.Json.IsEqual(a, b);
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]//it serves 2 aims: - ignore when 0; - forces setting through the private setter (yes, it does!)
        override public uint __TypeVersion { get; protected set; } = 0;

        [JsonIgnore]
        override public string __StorageDir { get; protected set; }
    }
}