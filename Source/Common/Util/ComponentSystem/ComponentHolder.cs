using System.Collections.Generic;
using Godot;
namespace NullGarel.Util.ComponentSystem;

/// <summary>
/// Acts as a representation of an entity, it should be a direct child of a node that is to be considered an entity in this
/// custom entity system.
/// </summary>
[Icon("res://Assets/Textures/Components/componentHolder.png")]
[GodotClassName(nameof(ComponentHolder))]
public partial class ComponentHolder : Node
{
    public int EntityId { get; set; }

    public override void _EnterTree()
    {
        foreach (Node child in GetChildren())
        {
            if (child is IComponent component)
            {
                component.Initialize(this);
            }
        }
    }

    // Utility for typed components to grab other components
    public T GetComponent<T>() where T : class, IComponent
    {
        foreach (Node child in GetChildren())
            if (child is T match)
                return match;
        return null;
    }

    public IEnumerable<IComponent> GetAllComponents()
    {
        foreach (Node child in GetChildren())
            if (child is IComponent component)
                yield return component;
    }

    public bool HasComponent<T>() where T : class, IComponent
    {
        foreach (Node child in GetChildren())
            if (child is T)
                return true;
        return false;
    }

    public bool TryGetComponent<T>(out T component) where T : class, IComponent
    {
        component = GetComponent<T>();
        return component != null;
    }

    public IEnumerable<T> GetComponents<T>() where T : class, IComponent
    {
        foreach (Node child in GetChildren())
            if (child is T match)
                yield return match;
    }

    public IComponent GetComponentByTypeName(string typeName)
    {
        foreach (Node child in GetChildren())
            if (child is IComponent component && component.GetType().Name == typeName)
                return component;
        return null;
    }
}
