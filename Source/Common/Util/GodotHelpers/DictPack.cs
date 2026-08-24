// CLANKER GENERATED. POTENTIALLY SLOPPY CODE.
// Generated: 202608231931
// Agent/model: Claude (Sonnet 5, claude.ai)
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace NullGarel.Util.GodotHelpers;

/// <summary>
/// Marks a property as part of the wire format on a GodotObject-derived type (Node, Resource,
/// etc). For those types DictPack is opt-in, not opt-out - a property without this attribute is
/// never touched by Pack/Unpack. This matters because a Node/Resource's public settable-property
/// surface has nothing to do with your data model (Owner, Name, ProcessMode, ResourcePath...),
/// and Owner in particular can drag in a reference back to the whole scene subtree, recursively.
/// Plain POCO DTOs (not GodotObject-derived) don't need this - every public settable property on
/// those is included automatically, same as before.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PackableAttribute : Attribute
{
}

/// <summary>
/// Reflection-based serializer/deserializer between plain C# DTOs and Godot.Collections.Dictionary.
/// Intended to replace the old legacy msgpack approach <see cref="IO.BinPack"/>.
/// </summary>
public static class DictPack
{
    // Reflection is expensive; cache per-type property lists instead of re-querying every call.
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    private static PropertyInfo[] GetProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, BuildPropertyList);
    }

    private static PropertyInfo[] BuildPropertyList(Type type)
    {
        // Skip indexers and write-only/read-only props - SetValue/GetValue would throw on them.
        var candidates = System.Array.FindAll(
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance),
            p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);

        // GodotObject-derived types (Node, Resource, and everything that inherits from them,
        // which covers components) have a public surface area that's mostly Godot engine
        // plumbing, not your data model - require explicit [Packed] opt-in for those. Plain
        // POCO DTOs keep the original opt-out behavior: everything settable is included by
        // default, since that's the whole point of writing a small DTO by hand.
        if (typeof(GodotObject).IsAssignableFrom(type))
        {
            candidates = System.Array.FindAll(candidates,
                p => p.IsDefined(typeof(PackableAttribute), inherit: true));
        }

        return candidates;
    }

    /// <summary>
    /// Converts a complex data type into a Godot Dictionary suitable for RPC/DictPack transport.
    /// </summary>
    public static Dictionary Pack<T>(T data) where T : class
    {
        return data == null ? new Dictionary() : SerializeObject(data, typeof(T));
    }

    /// <summary>
    /// Same as <see cref="Pack{T}"/>, but for when the concrete type is only known at runtime
    /// (e.g. serializing a polymorphic component where you have the instance's Type, not a
    /// compile-time generic argument).
    /// </summary>
    public static Dictionary Pack(object data, Type type)
    {
        return data == null ? new Dictionary() : SerializeObject(data, type);
    }

    private static Dictionary SerializeObject(object data, Type type)
    {
        var dict = new Dictionary();

        foreach (var prop in GetProperties(type))
        {
            object val = prop.GetValue(data);
            if (val == null) continue;

            dict[prop.Name] = ObjectToVariant(val);
        }

        return dict;
    }

    public static Array SerializeList(IEnumerable items)
    {
        var arr = new Array();
        if (items == null) return arr;

        foreach (var item in items)
        {
            if (item == null) continue;
            arr.Add(ObjectToVariant(item));
        }

        return arr;
    }

    private static Variant ObjectToVariant(object val)
    {
        switch (val)
        {
            case string s: return Variant.From(s);
            case Color c: return Variant.From(c);
            case int i: return Variant.From(i);
            case long l: return Variant.From(l);
            case float f: return Variant.From(f);
            case double d: return Variant.From(d);
            case bool b: return Variant.From(b);
            case Vector2 v2: return Variant.From(v2);
            case Vector3 v3: return Variant.From(v3);
            case Enum e:
                // Store enums as their underlying integral value; Unpack re-hydrates them.
                return Variant.From(Convert.ToInt64(e));
        }

        Type valType = val.GetType();

        // Lists/arrays of any supported element type. This MUST come before the nested-DTO
        // check below - List<T>/T[] are themselves reference types (IsClass == true), so if
        // checked in the other order every collection gets misidentified as a nested DTO and
        // reflected over as a plain object instead of having its elements serialized.
        if (val is IEnumerable enumerable)
        {
            var arr = new Array();
            foreach (var item in enumerable)
            {
                if (item == null) continue;
                arr.Add(ObjectToVariant(item));
            }
            return Variant.From(arr);
        }

        // Nested DTO - recurse instead of silently dropping the field.
        if (valType.IsClass && valType != typeof(string))
        {
            return Variant.From(SerializeObject(val, valType));
        }

        // Previously this silently returned an empty Variant, which meant an unsupported
        // field just vanished with no trace. That's a nasty class of bug to chase down in
        // an RPC payload, so at least surface it.
        GD.PushWarning($"DictPack: no Variant conversion for type '{valType}', field will be dropped.");
        return default;
    }

    /// <summary>
    /// Converts a dictionary into a complex data type, reconstructing it.
    /// </summary>
    public static T Unpack<T>(Dictionary dict) where T : class, new()
    {
        if (dict == null || dict.Count == 0) return new T();
        return (T)DeserializeObject(dict, typeof(T));
    }

    /// <summary>
    /// Same as <see cref="Unpack{T}"/>, but for when the target type is only known at runtime.
    /// Caller is responsible for casting the result to the expected type.
    /// </summary>
    public static object Unpack(Dictionary dict, Type type)
    {
        if (dict == null || dict.Count == 0) return Activator.CreateInstance(type);
        return DeserializeObject(dict, type);
    }

    /// <summary>
    /// Applies a dict's values onto an ALREADY-EXISTING instance, instead of constructing a new
    /// one. Use this for anything with real construction/lifecycle requirements that Activator
    /// can't replicate - components attached via ComponentHolder being the prime example. Spawn
    /// or construct the object the normal way first, then call this to hydrate saved values onto
    /// it, rather than ever letting DictPack own construction for a live Node.
    /// </summary>
    public static void Populate(object instance, Dictionary dict)
    {
        if (instance == null || dict == null) return;
        PopulateObject(instance, dict, instance.GetType());
    }

    private static void PopulateObject(object instance, Dictionary dict, Type type)
    {
        foreach (var prop in GetProperties(type))
        {
            if (!dict.TryGetValue(prop.Name, out var variant)) continue;

            object rawVal = variant.Obj;
            if (rawVal == null) continue;

            try
            {
                prop.SetValue(instance, ConvertValue(rawVal, prop.PropertyType));
            }
            catch (Exception ex)
            {
                // A malformed/unexpected field shouldn't take down the whole RPC handler -
                // log it and leave the property at its default so the caller can keep going.
                GD.PushWarning($"DictPack: failed to set '{prop.Name}' on '{type.Name}': {ex.Message}");
            }
        }
    }

    private static object DeserializeObject(Dictionary dict, Type type)
    {
        var instance = Activator.CreateInstance(type);
        PopulateObject(instance, dict, type);
        return instance;
    }

    private static object ConvertValue(object rawVal, Type targetType)
    {
        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying.IsEnum)
        {
            return Enum.ToObject(underlying, Convert.ToInt64(rawVal));
        }

        if (rawVal is Dictionary nested && underlying.IsClass && underlying != typeof(string))
        {
            return DeserializeObject(nested, underlying);
        }

        if (rawVal is Array godotArray && typeof(IEnumerable).IsAssignableFrom(underlying) && underlying != typeof(string))
        {
            return ConvertArray(godotArray, underlying);
        }

        if (underlying.IsInstanceOfType(rawVal))
        {
            return rawVal;
        }

        return Convert.ChangeType(rawVal, underlying);
    }

    private static object ConvertArray(Array godotArray, Type targetType)
    {
        // Handles T[] and List<T>; anything else falls back to boxing as object.
        Type elementType = targetType.IsArray
            ? targetType.GetElementType()
            : targetType.IsGenericType ? targetType.GetGenericArguments()[0] : typeof(object);

        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType!));
        foreach (var item in godotArray)
        {
            object rawItem = item.Obj;
            if (rawItem == null) continue;
            list.Add(ConvertValue(rawItem, elementType!));
        }

        if (targetType.IsArray)
        {
            var arr = System.Array.CreateInstance(elementType!, list.Count);
            list.CopyTo(arr, 0);
            return arr;
        }

        return list; // List<T> assignable directly.
    }
}