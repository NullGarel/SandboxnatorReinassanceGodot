// CLANKER GENERATED. POTENTIALLY SLOPPY CODE.
// Generated: 202608232218
// Agent/model: Claude (Sonnet 5, claude.ai)
using Godot;
using Godot.Collections;
using NullGarel.Util.ComponentSystem;
using NullGarel.Util.GodotHelpers;
using System.Collections;
using Array = Godot.Collections.Array;

namespace NullGarel.Sandboxnator.Data;

public static class DictPackExtensions
{
    public static Array SerializeComponents(IEnumerable components)
    {
        var arr = new Array();
        foreach (var c in components)
        {
            if (c == null) continue;

            string typeName = ComponentRegistry.GetId(c.GetType());
            if (typeName == null)
            {
                GD.PushWarning($"DictPack: no registered id for component type '{c.GetType().Name}', skipping.");
                continue;
            }

            var entry = new Dictionary
            {
                ["TypeName"] = typeName,
                ["Properties"] = DictPack.Pack(c, c.GetType())
            };
            arr.Add(entry);
        }
        return arr;
    }

    /// <summary>
    /// Applies a saved component entry ({"TypeName", "Properties"}) onto the matching LIVE
    /// component already attached to the given holder. Never constructs the component itself -
    /// that has to happen through ComponentHolder's normal attach path so lifecycle/authority
    /// setup still runs. If the holder has no live component of that type, this is a no-op
    /// (with a warning) rather than a crash - a save file with a component the current build
    /// doesn't spawn by default shouldn't take down the whole load.
    /// </summary>
    public static void HydrateComponent(ComponentHolder holder, Dictionary entry)
    {
        if (!entry.TryGetValue("TypeName", out var typeNameVariant)) return;
        string typeName = typeNameVariant.AsString();

        var type = ComponentRegistry.Resolve(typeName);
        if (type == null)
        {
            GD.PushWarning($"DictPack: unknown component TypeName '{typeName}', skipping.");
            return;
        }

        // NOTE: assumes ComponentHolder exposes a way to fetch an already-attached component
        // by its runtime Type. Adjust to whatever the real accessor is named/shaped as.
        object component = holder.GetComponent(type);
        if (component == null)
        {
            GD.PushWarning($"DictPack: no live '{typeName}' component on this placeable to hydrate, skipping.");
            return;
        }

        var props = entry.TryGetValue("Properties", out var propsVariant)
            ? propsVariant.AsGodotDictionary()
            : new Dictionary();

        DictPack.Populate(component, props);
    }
}