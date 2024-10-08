﻿using FastExpressionCompiler.LightExpression;
using LinqFasterer;
using SpriteMaster.Types.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Extensions.Reflection;

internal static partial class ReflectionExt {
    private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
    private const BindingFlags AllInstanceBinding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags AllNonFlatBinding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private const DynamicallyAccessedMemberTypes AllFields = DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields;
    private const DynamicallyAccessedMemberTypes AllProperties = DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties;
    private const DynamicallyAccessedMemberTypes AllMethods = DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static MemberInfo? GetPropertyOrField(this Type type, string name, BindingFlags bindingAttr) {
        // TODO : GetMembers might be better?
        // TODO : Or we should cache everything?
        if (type.GetField(name, bindingAttr) is { } field) {
            return field;
        }
        if (type.GetProperty(name, bindingAttr) is { } property) {
            return property;
        }
        return null;
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    internal static MemberInfo? GetPropertyOrField(this Type type, string name) => type.GetPropertyOrField(name, DefaultLookup);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static T? GetValue<T>(this FieldInfo field, object instance) => (T?)field.GetValue(instance);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static T? GetValue<T>(this PropertyInfo property, object instance) => (T?)property.GetValue(instance);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool HasAttribute<T>(this MemberInfo member) where T : Attribute => member.GetCustomAttribute<T>() is not null;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool GetAttribute<T>(this MemberInfo member, [NotNullWhen(true)] out T? attribute) where T : Attribute {
        attribute = member.GetCustomAttribute<T>();
        return attribute is not null;
    }

    /*
	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static T[] GetCustomAttributes<T>(this MemberInfo member) where T : Attribute {
		return (T[])member.GetCustomAttributes(typeof(T), inherit: false);
	}
	*/

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields)]
    internal static bool TryGetField(this Type type, string name, [NotNullWhen(true)] out FieldInfo? field, BindingFlags bindingAttr) {
        field = type.GetField(name, bindingAttr);
        return field is not null;
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields)]
    internal static bool TryGetField(this Type type, string name, [NotNullWhen(true)] out FieldInfo? field) =>
        type.TryGetField(name, out field, AllNonFlatBinding);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllProperties)]
    internal static bool TryGetProperty(this Type type, string name, [NotNullWhen(true)] out PropertyInfo? property, BindingFlags bindingAttr) {
        property = type.GetProperty(name, bindingAttr);
        return property is not null;
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllProperties)]
    internal static bool TryGetProperty(this Type type, string name, [NotNullWhen(true)] out PropertyInfo? property) =>
        type.TryGetProperty(name, out property, AllNonFlatBinding);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static bool TryGetMethod(this Type type, string name, [NotNullWhen(true)] out MethodInfo? method, BindingFlags bindingAttr) {
        method = type.GetMethod(name, bindingAttr);
        return method is not null;
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static bool TryGetMethod(this Type type, string name, [NotNullWhen(true)] out MethodInfo? method) =>
        type.TryGetMethod(name, out method, AllNonFlatBinding);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields)]
    internal static object? GetField(this object? obj, string name) => obj?.GetType().GetField(
        name,
        AllInstanceBinding | BindingFlags.FlattenHierarchy
    )?.GetValue(obj);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields)]
    internal static T? GetField<T>(this object? obj, string name) => (T?)obj?.GetType().GetField(
        name,
        AllInstanceBinding | BindingFlags.FlattenHierarchy
    )?.GetValue(obj);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllProperties)]
    internal static object? GetProperty(this object? obj, string name) => obj?.GetType().GetProperty(
        name,
        AllInstanceBinding | BindingFlags.FlattenHierarchy
    )?.GetValue(obj);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllProperties)]
    internal static T? GetProperty<T>(this object? obj, string name) => (T?)obj?.GetType().GetProperty(
        name,
        AllInstanceBinding | BindingFlags.FlattenHierarchy
    )?.GetValue(obj);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static IList<MethodInfo> GetMethods(this Type type, string name, BindingFlags bindingFlags) {
        return type.GetMethods(bindingFlags).WhereF(t => t.Name == name);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static IList<MethodInfo> GetStaticMethods(this Type type, string name) {
        return type.GetMethods(name, ShallowStaticFlags);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static MethodInfo[] GetStaticMethods(this Type type) {
        return type.GetMethods(ShallowStaticFlags);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static MethodInfo? GetStaticMethod(this Type type, string name) {
        return type.GetMethod(name, ShallowStaticFlags);
    }


    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields)]
    internal static FieldInfo? GetStaticField(this Type type, string name) {
        return type.GetField(name, ShallowStaticFlags);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static TDelegate? GetStaticDelegate<TDelegate>(this Type type, string name) where TDelegate : Delegate {
        return type.GetMethod(name, ShallowStaticFlags)?.CreateDelegate<TDelegate>();
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static IList<MethodInfo> GetInstanceMethods(this Type type, string name) {
        return type.GetMethods(name, ShallowInstanceFlags);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static MethodInfo[] GetInstanceMethods(this Type type) {
        return type.GetMethods(ShallowInstanceFlags);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static MethodInfo? GetInstanceMethod(this Type type, string name) {
        return type.GetMethod(name, ShallowInstanceFlags);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static TDelegate? GetInstanceDelegate<TDelegate>(this Type type, string name) where TDelegate : Delegate {
        return type.GetMethod(name, ShallowInstanceFlags)?.CreateDelegate<TDelegate>();
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static IList<MethodInfo> GetMethods<T>(string name, BindingFlags bindingFlags) {
        return typeof(T).GetMethods(name, bindingFlags);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static IList<MethodInfo> GetStaticMethods<T>(string name) {
        return typeof(T).GetStaticMethods(name);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllMethods)]
    internal static IList<MethodInfo> GetInstanceMethods<T>(string name) {
        return typeof(T).GetInstanceMethods(name);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties | AllMethods)]
    internal static MemberInfo[] GetInstanceMembers<T>(string name) =>
        GetInstanceMembers(typeof(T), name);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties | AllMethods)]
    internal static MemberInfo[] GetInstanceMembers(this Type type, string name) =>
        type.GetMember(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static VariableInfo[] GetInstanceVariables<T>(string name) =>
        GetInstanceVariables(typeof(T), name);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static VariableInfo[] GetInstanceVariables(this Type type, string name) {
        var members = type.GetInstanceMembers(name);
        return members.WhereF(member => member is (FieldInfo or PropertyInfo)).SelectF(VariableInfo.From).ToArrayF()!;
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties | AllMethods)]
    internal static MemberInfo[] GetStaticMembers<T>(string name) =>
        GetStaticMembers(typeof(T), name);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties | AllMethods)]
    internal static MemberInfo[] GetStaticMembers(this Type type, string name) =>
        type.GetMember(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static VariableInfo[] GetStaticVariables<T>(string name) =>
        GetStaticVariables(typeof(T), name);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static VariableInfo[] GetStaticVariables(this Type type, string name) {
        var members = type.GetStaticMembers(name);
        return members.WhereF(member => member is (FieldInfo or PropertyInfo)).SelectF(VariableInfo.From).ToArrayF()!;
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static VariableInfo? GetInstanceVariable<T>(string name) =>
        GetInstanceVariable(typeof(T), name);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static VariableInfo? GetInstanceVariable(this Type type, string name) {
        var members = type.GetInstanceMembers(name);
        return VariableInfo.From(members.WhereF(member => member is (FieldInfo or PropertyInfo)).FirstOrDefaultF());
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static VariableInfo? GetStaticVariable<T>(string name) =>
        GetStaticVariable(typeof(T), name);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    [DynamicallyAccessedMembers(AllFields | AllProperties)]
    internal static VariableInfo? GetStaticVariable(this Type type, string name) {
        var members = type.GetStaticMembers(name);
        return VariableInfo.From(members.WhereF(member => member is (FieldInfo or PropertyInfo)).FirstOrDefaultF());
    }

    internal static Predicate<object?> GetIsDelegate(this Type? type) {
        if (type is null) {
            return _ => false;
        }

        var objectExpression = Expression.Parameter(typeof(object), "obj");
        var isNullExpression = Expression.ReferenceEqual(objectExpression, Expression.NullConstant);
        var isTypeExpression = Expression.TypeIs(objectExpression, type);
        var resultExpression = Expression.IfThenElse(isNullExpression, Expression.FalseConstant, isTypeExpression);

        return Expression.Lambda<Predicate<object?>>(resultExpression, objectExpression).CompileFast();
    }

    internal static Predicate<TObject?> GetIsDelegate<TObject>(this Type? type) {
        if (type is null) {
            return _ => false;
        }

        var objectExpression = Expression.Parameter(typeof(object), "obj");
        var isNullExpression = Expression.ReferenceEqual(objectExpression, Expression.NullConstant);
        var isTypeExpression = Expression.TypeIs(objectExpression, type);
        var resultExpression = Expression.IfThenElse(isNullExpression, Expression.FalseConstant, isTypeExpression);

        return Expression.Lambda<Predicate<TObject?>>(resultExpression, objectExpression).CompileFast();
    }
}
