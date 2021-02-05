//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;

namespace Cliver
{
    /// <summary>
    /// Settings attributes which are defined by a Settings field.
    /// </summary>
    abstract public class SettingsMemberInfo
    {
        /// <summary>
        /// Settings' full name is the string that is used in code to refer to this field/property. 
        /// It defines exactly the Settings field/property in code but has nothing to do with the one's type. 
        /// </summary>
        public readonly string FullName;

        /// <summary>
        /// Path of the storage file. It consists of a directory which defined by the Settings based type and the file name which is the field's full name in the code.
        /// </summary>
        public readonly string File;

        /// <summary>
        /// Path of the init file. It consists of the directory where the entry assembly is located and the file name which is the field's full name in the code.
        /// </summary>
        public readonly string InitFile;

        /// <summary>
        /// Whether serialization to string is to be done with indention.
        /// </summary>
        public bool Indented;

        /// <summary>
        /// Settings derived type.
        /// </summary>
        public readonly Type Type;

        internal Settings GetObject()
        {
            lock (this)
            {
                return (Settings)getObject();
            }
        }
        //abstract protected Settings getObject();
        readonly Func<object> getObject;

        internal void SetObject(Settings settings)
        {
            lock (this)
            {
                setObject(settings);
            }
        }
        //abstract protected void setObject(Settings settings);
        readonly Action<Settings> setObject;

        internal readonly SettingsAttribute Attribute;

        protected SettingsMemberInfo(MemberInfo settingsTypeMemberInfo, Type settingType, Func<object> getObject, Action<Settings> setObject)
        {
            Type = settingType;
            this.getObject = getObject;
            this.setObject = setObject;
            FullName = settingsTypeMemberInfo.DeclaringType.FullName + "." + settingsTypeMemberInfo.Name;
            /*//version with static __StorageDir
            string storageDir;
            for (; ; )
            {
                PropertyInfo fi = settingType.GetProperty(nameof(Settings.__StorageDir), BindingFlags.Static | BindingFlags.Public);
                if (fi != null)
                {
                    storageDir = (string)fi.GetValue(null);
                    break;
                }
                settingType = settingType.BaseType;
                if (settingType == null)
                    throw new Exception("Settings type " + Type.ToString() + " or some of its ancestors must define the public static getter " + nameof(Settings.__StorageDir));
            }
            File = storageDir + System.IO.Path.DirectorySeparatorChar + FullName + "." + Config.FILE_EXTENSION;
            */
            Settings s = (Settings)Activator.CreateInstance(Type); //!!!slightly slowler than calling a static by reflection. Doesn't run slower for a bigger class though.
            File = s.__StorageDir + System.IO.Path.DirectorySeparatorChar + FullName + "." + Config.FILE_EXTENSION;
            InitFile = Log.AppDir + System.IO.Path.DirectorySeparatorChar + FullName + "." + Config.FILE_EXTENSION;
            Attribute = settingsTypeMemberInfo.GetCustomAttributes<SettingsAttribute>(false).FirstOrDefault();
            Indented = Attribute == null ? true : Attribute.Indented;
        }
    }

    public class SettingsFieldInfo : SettingsMemberInfo
    {
        //override protected object getObject()
        //{
        //    return FieldInfo.GetValue(null);
        //}

        //override protected void setObject(Settings settings)
        //{
        //    FieldInfo.SetValue(null, settings);
        //}

        //readonly FieldInfo FieldInfo;

        internal SettingsFieldInfo(FieldInfo settingsTypeFieldInfo) : base(
            settingsTypeFieldInfo,
            settingsTypeFieldInfo.FieldType,
            getGetValue(settingsTypeFieldInfo),
            getSetValue(settingsTypeFieldInfo)
            )
        {
            //FieldInfo = settingsTypeFieldInfo;
        }
        protected static Func<object> getGetValue(FieldInfo fieldInfo)//faster than FieldInfo.GetValue
        {
            MemberExpression me = Expression.Field(null, fieldInfo);
            return Expression.Lambda<Func<object>>(me).Compile();

        }
        protected static Action<Settings> getSetValue(FieldInfo fieldInfo)//faster than FieldInfo.SetValue
        {
            ParameterExpression pe = Expression.Parameter(typeof(object));
            UnaryExpression ue = Expression.Convert(pe, fieldInfo.FieldType);
            MemberExpression me = Expression.Field(null, fieldInfo);
            BinaryExpression be = Expression.Assign(me, ue);
            return Expression.Lambda<Action<Settings>>(be, pe).Compile();
        }
    }

    public class SettingsPropertyInfo : SettingsMemberInfo
    {
        //override protected object getObject()
        //{
        //    return PropertyInfo.GetValue(null);
        //}

        //override protected void setObject(Settings settings)
        //{
        //    PropertyInfo.SetValue(null, settings);
        //}

        //readonly PropertyInfo PropertyInfo;

        internal SettingsPropertyInfo(PropertyInfo settingsTypePropertyInfo) : base(
            settingsTypePropertyInfo,
            settingsTypePropertyInfo.PropertyType,
            getGetValue(settingsTypePropertyInfo.GetGetMethod(true)),
            getSetValue(settingsTypePropertyInfo.GetSetMethod(true))
            )
        {
            //PropertyInfo = settingsTypePropertyInfo;
        }
        protected static Func<object> getGetValue(MethodInfo methodInfo)//faster than PropertyInfo.GetValue
        {
            MethodCallExpression mce = Expression.Call(methodInfo);
            return Expression.Lambda<Func<object>>(mce).Compile();
        }
        protected static Action<Settings> getSetValue(MethodInfo methodInfo)//faster than PropertyInfo.SetValue
        {
            ParameterExpression pe = Expression.Parameter(typeof(object));
            UnaryExpression ue = Expression.Convert(pe, methodInfo.GetParameters().First().ParameterType);
            MethodCallExpression mce = Expression.Call(methodInfo, ue);
            return Expression.Lambda<Action<Settings>>(mce, pe).Compile();
        }
    }
}
