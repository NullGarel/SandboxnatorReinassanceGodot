using Godot;
using Godot.Collections;
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

    public static object DeserializeComponentEntry(Dictionary entry)
    {
        if (!entry.TryGetValue("TypeName", out var typeNameVariant)) return null;

        string typeName = typeNameVariant.AsString();
        var type = ComponentRegistry.Resolve(typeName);
        if (type == null)
        {
            GD.PushWarning($"DictPack: unknown component TypeName '{typeName}', skipping.");
            return null;
        }

        var props = entry.TryGetValue("Properties", out var propsVariant)
            ? propsVariant.AsGodotDictionary()
            : [];

        return DictPack.Unpack(props, type);
    }
}