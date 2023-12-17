/********************************************************************************************
        Author: Sergiy Stoyan
CREDITS TO: https://stackoverflow.com/questions/17660097/is-it-possible-to-speed-this-method-up/17669142#17669142
        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
        http://www.cliversoft.com
********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic;

namespace Cliver
{
    public class FastAccessor<T, ValueT>
    {
        public FastAccessor(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo pi)
            {
                Get = BuildFastGetter(pi);
                Set = BuildFastSetter(pi);
            }
            else
            {
                Get = BuildGetter(memberInfo);
                Set = BuildSetter(memberInfo);
            }
        }

        public FastAccessor(string memberName, BindingFlags bindingFlags = BindingFlags.Default) : this(getMemberInfo(memberName, bindingFlags))
        { }
        static MemberInfo getMemberInfo(string memberName, BindingFlags bindingFlags)
        {
            var mis = typeof(T).GetMember(memberName, bindingFlags);
            if (mis.Length < 1)
                throw new Exception("No MemberInfo[memberName=" + memberName + ", bindingFlags=" + bindingFlags + "] found.");
            return mis[0];
        }

        readonly public Func<T, ValueT> Get;
        readonly public Action<T, ValueT> Set;

        public static Func<T, ValueT> BuildGetter(MemberInfo memberInfo)
        {
            var targetType = memberInfo.DeclaringType;
            var exInstance = Expression.Parameter(targetType, "t");

            var exMemberAccess = Expression.MakeMemberAccess(exInstance, memberInfo);       // t.PropertyName
            var exConvertToObject = Expression.Convert(exMemberAccess, typeof(ValueT));
            var lambda = Expression.Lambda<Func<T, ValueT>>(exConvertToObject, exInstance);

            var action = lambda.Compile();
            return action;
        }

        public static Action<T, ValueT> BuildSetter(MemberInfo memberInfo)
        {
            var targetType = memberInfo.DeclaringType;
            var exInstance = Expression.Parameter(targetType, "t");

            var exMemberAccess = Expression.MakeMemberAccess(exInstance, memberInfo);

            // t.PropertValue(Convert(p))
            var exValue = Expression.Parameter(typeof(ValueT), "p");
            var exConvertedValue = Expression.Convert(exValue, GetUnderlyingType(memberInfo));
            var exBody = Expression.Assign(exMemberAccess, exConvertedValue);

            var lambda = Expression.Lambda<Action<T, ValueT>>(exBody, exInstance, exValue);
            var action = lambda.Compile();
            return action;
        }

        public static Func<T, ValueT> BuildFastGetter(PropertyInfo propertyInfo)
        {
            Func<T, ValueT> reflGet = (Func<T, ValueT>)Delegate.CreateDelegate(typeof(Func<T, ValueT>), propertyInfo.GetGetMethod());
            return reflGet;
        }

        public static Action<T, ValueT> BuildFastSetter(PropertyInfo propertyInfo)
        {
            return (Action<T, ValueT>)Delegate.CreateDelegate(typeof(Action<T, ValueT>), propertyInfo.GetSetMethod());
        }

        //public static Action<T, ValueT> BuildSetter<T, ValueT>(FieldInfo fieldInfo)
        //{
        //    //return (Action<T, ValueT>)Delegate.CreateDelegate(typeof(Action<T, ValueT>), fieldInfo.FieldHandle.Value);
        //    string methodName = fieldInfo.ReflectedType.FullName + ".set_" + fieldInfo.Name;
        //    Delegate setterMethod = new DynamicMethod(methodName, null, new Type[] { typeof(S), typeof(T) }, true);
        //    ILGenerator gen = setterMethod.GetILGenerator();

        //    gen.Emit(OpCodes.Ldarg_0);
        //    gen.Emit(OpCodes.Ldarg_1);
        //    gen.Emit(OpCodes.Stfld, field);
        //    gen.Emit(OpCodes.Ret);

        //    return (Action<T, FieldT>)setterMethod.CreateDelegate(typeof(Action<T, FieldT>));
        //}

        internal static Type GetUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
    }

    //public class FastAccessor<T>
    //{
    //    public FastAccessor(MemberInfo memberInfo)
    //    {
    //        if (memberInfo is PropertyInfo pi)
    //        {
    //            Get = BuildFastGetter(pi);
    //            Set = BuildFastSetter(pi);
    //        }
    //        else
    //        {
    //            Get = BuildGetter(memberInfo);
    //            Set = BuildSetter(memberInfo);
    //        }
    //    }

    //    public FastAccessor(T @object, string memberName, BindingFlags bindingFlags) : this(getMemberInfo(@object, memberName, bindingFlags))
    //    { }
    //    static MemberInfo getMemberInfo(T @object, string memberName, BindingFlags bindingFlags)
    //    {
    //        var mis = typeof(T).GetMember(memberName, bindingFlags);
    //        if (mis.Length < 1)
    //            throw new Exception("No MemberInfo[memberName=" + memberName + ", bindingFlags=" + bindingFlags + "] found.");
    //        return mis[0];
    //    }

    //    readonly public Func<T, ValueT> Get;
    //    readonly public Action<T, ValueT> Set;

    //    public static Func<T, ValueT> BuildGetter(MemberInfo memberInfo)
    //    {
    //        var targetType = memberInfo.DeclaringType;
    //        var exInstance = Expression.Parameter(targetType, "t");

    //        var exMemberAccess = Expression.MakeMemberAccess(exInstance, memberInfo);       // t.PropertyName
    //        var exConvertToObject = Expression.Convert(exMemberAccess, typeof(ValueT));
    //        var lambda = Expression.Lambda<Func<T, ValueT>>(exConvertToObject, exInstance);

    //        var action = lambda.Compile();
    //        return action;
    //    }

    //    public static Action<T, ValueT> BuildSetter(MemberInfo memberInfo)
    //    {
    //        var targetType = memberInfo.DeclaringType;
    //        var exInstance = Expression.Parameter(targetType, "t");

    //        var exMemberAccess = Expression.MakeMemberAccess(exInstance, memberInfo);

    //        // t.PropertValue(Convert(p))
    //        var exValue = Expression.Parameter(typeof(ValueT), "p");
    //        var exConvertedValue = Expression.Convert(exValue, GetUnderlyingType(memberInfo));
    //        var exBody = Expression.Assign(exMemberAccess, exConvertedValue);

    //        var lambda = Expression.Lambda<Action<T, ValueT>>(exBody, exInstance, exValue);
    //        var action = lambda.Compile();
    //        return action;
    //    }

    //    public static Func<T, ValueT> BuildFastGetter(PropertyInfo propertyInfo)
    //    {
    //        Func<T, ValueT> reflGet = (Func<T, ValueT>)Delegate.CreateDelegate(typeof(Func<T, ValueT>), propertyInfo.GetGetMethod());
    //        return reflGet;
    //    }

    //    public static Action<T, ValueT> BuildFastSetter(PropertyInfo propertyInfo)
    //    {
    //        return (Action<T, ValueT>)Delegate.CreateDelegate(typeof(Action<T, ValueT>), propertyInfo.GetSetMethod());
    //    }

    //    //public static Action<T, ValueT> BuildSetter<T, ValueT>(FieldInfo fieldInfo)
    //    //{
    //    //    //return (Action<T, ValueT>)Delegate.CreateDelegate(typeof(Action<T, ValueT>), fieldInfo.FieldHandle.Value);
    //    //    string methodName = fieldInfo.ReflectedType.FullName + ".set_" + fieldInfo.Name;
    //    //    Delegate setterMethod = new DynamicMethod(methodName, null, new Type[] { typeof(S), typeof(T) }, true);
    //    //    ILGenerator gen = setterMethod.GetILGenerator();

    //    //    gen.Emit(OpCodes.Ldarg_0);
    //    //    gen.Emit(OpCodes.Ldarg_1);
    //    //    gen.Emit(OpCodes.Stfld, field);
    //    //    gen.Emit(OpCodes.Ret);

    //    //    return (Action<T, FieldT>)setterMethod.CreateDelegate(typeof(Action<T, FieldT>));
    //    //}

    //    internal static Type GetUnderlyingType(MemberInfo member)
    //    {
    //        switch (member.MemberType)
    //        {
    //            case MemberTypes.Event:
    //                return ((EventInfo)member).EventHandlerType;
    //            case MemberTypes.Field:
    //                return ((FieldInfo)member).FieldType;
    //            case MemberTypes.Method:
    //                return ((MethodInfo)member).ReturnType;
    //            case MemberTypes.Property:
    //                return ((PropertyInfo)member).PropertyType;
    //            default:
    //                throw new ArgumentException
    //                (
    //                 "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
    //                );
    //        }
    //    }
    //}
}