//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
#define COMPILE_GetObject_SetObject1 //!!!Stopwatch shows that compiling is not faster. Probably the reflection was improved.

using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Cliver.Newtonsoft
{
    abstract public class SettingsFieldInfo : Cliver.SettingsFieldInfo
    {
        protected SettingsFieldInfo(MemberInfo settingsTypeMemberInfo, Type settingsType) : base(settingsTypeMemberInfo, settingsType)
        {
        }

        #region Type Version support

        /// <summary>
        /// Read the storage file as a JObject in order to migrate to the current format.
        /// </summary>
        /// <returns>storage file content presented as JObject</returns>
        public global::Newtonsoft.Json.Linq.JObject ReadStorageFileAsJObject()
        {
            lock (this)
            {
                string file = File;
                if (!System.IO.File.Exists(file))
                    file = InitFile;
                if (!System.IO.File.Exists(file))
                    return null;
                string s = System.IO.File.ReadAllText(file);
                if (Endec != null)
                    s = Endec.Decrypt<string>(s);
                return global::Newtonsoft.Json.Linq.JObject.Parse(s);
            }
        }

        /// <summary>
        /// Write the JObject to the storage file in order to migrate to the current format.
        /// </summary>
        /// <param name="o">JObject presenting Settings field serialized as JSON</param>
        /// <param name="indented">whether the storage file content be indented</param>
        public void WriteStorageFileAsJObject(global::Newtonsoft.Json.Linq.JObject o, bool? indented = null)
        {
            lock (this)
            {
                if (indented == null)
                    indented = Indented;
                string s = o.ToString(indented.Value ? global::Newtonsoft.Json.Formatting.Indented : global::Newtonsoft.Json.Formatting.None);
                if (Endec != null)
                    s = Endec.Decrypt<string>(s);
                System.IO.File.WriteAllText(File, s);
            }
        }

        /// <summary>
        /// Read the storage file as a string in order to migrate to the current format.
        /// </summary>
        /// <returns>storage file content</returns>
        public string ReadStorageFileAsString()
        {
            lock (this)
            {
                string file = File;
                if (!System.IO.File.Exists(file))
                    file = InitFile;
                if (!System.IO.File.Exists(file))
                    return null;
                string s = System.IO.File.ReadAllText(file);
                if (Endec != null)
                    s = Endec.Decrypt<string>(s);
                return s;
            }
        }

        /// <summary>
        /// Write the string to the storage file in order to migrate to the current format.
        /// </summary>
        /// <param name="s">serialized Settings field</param>
        public void WriteStorageFileAsString(string s)
        {
            lock (this)
            {
                if (Endec != null)
                    s = Endec.Decrypt<string>(s);
                System.IO.File.WriteAllText(File, s);
            }
        }

        /// <summary>
        /// Update __TypeVersion value in the storage file content. __TypeVersion must exist in it to be updated. 
        /// </summary>
        /// <param name="typeVersion">new __TypeVersion</param>
        /// <param name="s">serialized Settings field</param>
        public void UpdateTypeVersionInStorageFileString(uint typeVersion, ref string s)
        {
            s = Regex.Replace(s, @"(?<=\""__TypeVersion\""\:\s*)\d+(?=\s*(,|)})", typeVersion.ToString(), RegexOptions.Singleline);
        }

        #endregion   
    }
}
