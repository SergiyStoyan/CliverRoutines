//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using Cliver.Newtonsoft;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cliver
{
    public partial class Settings<SettingsFieldInfoT> where SettingsFieldInfoT : SettingsFieldInfo
    {
        ///// <summary>
        ///// Returns the value of a serializable field identified by its name.
        ///// </summary>
        ///// <param name="fieldName">name of serializable field</param>
        ///// <returns>The value of the serializable field</returns>
        //public object GetFieldValue(string fieldName)
        //{
        //    //!!!while FieldInfo can see property, it loses its attributes if any.
        //    FieldInfo fi = GetType().GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).FirstOrDefault(a => a.Name == fieldName);
        //    return fi?.GetValue(this);
        //}

        ///// <summary>
        ///// Creates a new instance of the given Settings field with cloned values.
        ///// (!)The new instance shares the same __Info object with the original instance.
        ///// </summary>
        ///// <typeparam name="S"></typeparam>
        ///// <param name="jsonSerializerSettings">allows to customize cloning</param>
        ///// <returns></returns>
        //virtual public S Clone<S>(Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null) where S : Settings, new()
        //{
        //    S s = Serialization.Json.Clone((S)this, jsonSerializerSettings);
        //    if (__Info != null)
        //        s.__Info = __Info;
        //    return s;
        //}
    }
}