// CLANKER GENERATED. POTENTIALLY SLOPPY CODE.
// Generated: 202608231920
// Agent/model: Claude (Sonnet 5, claude.ai)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using NullGarel.Util.ComponentSystem;

namespace NullGarel.Sandboxnator.Data;

/// <summary>
/// Marks a component with an explicit serialization ID, overriding the default (the class name).
/// Use this on any component that's actually shipping in save files - it decouples the wire ID
/// from the C# class name, so renaming the class later doesn't silently break old save data
/// (the type.Name default is convenient but IS a footgun for anything long-lived).
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ComponentIdAttribute : Attribute
{
    public string Id { get; }
    public ComponentIdAttribute(string id) => Id = id;
}

public static class ComponentRegistry
{
    private static readonly Dictionary<string, Type> IdToType = [];
    private static readonly Dictionary<Type, string> TypeToId = [];
    private static bool _initialized;

    public static void Register(string id, Type type)
    {
        if (IdToType.TryGetValue(id, out var existing) && existing != type)
        {
            GD.PushError($"ComponentRegistry: id '{id}' already registered to '{existing.Name}', " +
                          $"cannot also register '{type.Name}'. Give one of them an explicit [ComponentId].");
            return;
        }

        IdToType[id] = type;
        TypeToId[type] = id;
    }

    public static Type Resolve(string id) => IdToType.GetValueOrDefault(id);
    public static string GetId(Type type) => TypeToId.GetValueOrDefault(type);

    /// <summary>
    /// Scans the given assembly (defaults to the assembly this method is called from) for every
    /// non-abstract class deriving from AbstractComponent&lt;T&gt; and registers it automatically.
    /// ID is the [ComponentId] attribute value if present, otherwise the class name.
    /// Call this once at game bootstrap, alongside wherever your other registries (items, etc.)
    /// get initialized - idempotent, so a second call is harmless but pointless.
    /// </summary>
    public static void AutoRegisterAll(Assembly assembly = null)
    {
        if (_initialized) return;
        assembly ??= Assembly.GetCallingAssembly();

        var componentTypes = assembly.GetTypes().Where(IsConcreteComponentType);

        foreach (var type in componentTypes)
        {
            string id = type.GetCustomAttribute<ComponentIdAttribute>()?.Id ?? type.Name;
            Register(id, type);
        }

        _initialized = true;
        GD.Print($"ComponentRegistry: registered {IdToType.Count} component type(s).");
    }

    private static bool IsConcreteComponentType(Type type)
    {
        if (type.IsAbstract || type.IsGenericTypeDefinition) return false;

        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(AbstractComponent<>))
                return true;
            baseType = baseType.BaseType;
        }

        return false;
    }
}