//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using System.IO;
using Newtonsoft.Json;

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
        }

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