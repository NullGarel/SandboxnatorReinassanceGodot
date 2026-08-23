// CLANKER GENERATED. POTENTIALLY SLOPPY CODE.
// Generated: 202608231521
// Agent/model: Claude (Sonnet 5, claude.ai)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using Godot.Collections;
using MessagePack;
using NullGarel.Util.IO;

namespace NullGarel.Util.GodotHelpers;

/// <summary>
/// Drop this on any Node in a throwaway test scene and run it. It fires a pile of DictPack
/// round-trips (Serialize -> Deserialize -> compare) covering primitives, enums, nested DTOs,
/// collections, nulls and partial dictionaries, and dumps a pass/fail report to the Output panel.
/// Not a real unit test harness - just a fast "did I break DictPack" smoke test you can eyeball.
/// Delete/disable before shipping; this isn't meant to live in the final build.
/// </summary>
[GlobalClass]
public partial class DictPackStressTest : Node
{
    private int _passed;
    private int _failed;

    public override void _Ready()
    {
        GD.PrintRich("[b]--- DictPack Stress Test ---[/b]");

        RunPrimitiveTests();
        RunEnumTests();
        RunNestedDtoTests();
        RunCollectionTests();
        RunNullableTests();
        RunEdgeCaseTests();

        GD.PrintRich(_failed == 0
            ? $"[color=green][b]DictPack stress test done: {_passed}/{_passed + _failed} passed.[/b][/color]"
            : $"[color=red][b]DictPack stress test done: {_passed}/{_passed + _failed} passed, {_failed} FAILED.[/b][/color]");
    }

    // ---------------------------------------------------------------
    // Test groups
    // ---------------------------------------------------------------

    private void RunPrimitiveTests()
    {
        var original = new PrimitiveDto
        {
            Name = "goblin_sword",
            Count = 42,
            LongValue = 9_000_000_000L,
            Weight = 3.5f,
            Precision = 1.23456789,
            IsMagic = true,
            Tint = new Color(0.2f, 0.4f, 0.8f, 1f),
            Position = new Vector2(12.5f, -3f),
            SpawnPoint = new Vector3(1f, 2f, 3f)
        };
        Check("primitives round-trip", () =>
        {

            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<PrimitiveDto>(dict);


            return result.Name == original.Name
                   && result.Count == original.Count
                   && result.LongValue == original.LongValue
                   && Mathf.IsEqualApprox(result.Weight, original.Weight)
                   && Math.Abs(result.Precision - original.Precision) < 0.0000001
                   && result.IsMagic == original.IsMagic
                   && result.Tint.IsEqualApprox(original.Tint)
                   && result.Position.IsEqualApprox(original.Position)
                   && result.SpawnPoint.IsEqualApprox(original.SpawnPoint);
        });

        Check("null fields are simply omitted, not crash", () =>
        {
            var original = new PrimitiveDto { Name = null };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<PrimitiveDto>(dict);
            return result.Name == null;
        });

        Check("JSON round-trip", () =>
        {
            var dict = DictPack.Pack(original);
            var json = Json.Stringify(dict);
            GD.Print(json);
            var parsedDict = Json.ParseString(json).AsGodotDictionary();
            var jsonResult = DictPack.Unpack<PrimitiveDto>(parsedDict);

            return jsonResult.Name == original.Name && jsonResult.Count == original.Count;
        });

        Check("BinPack(MsgPack)", () =>
        {

            var bin = BinPack.Pack(original);
            GD.Print(Convert.ToHexString(bin));
            GD.Print(Convert.ToBase64String(bin));

            var binResult = BinPack.Unpack<PrimitiveDto>(bin);

            return binResult.Name == original.Name && binResult.Count == original.Count;
        });
    }

    private void RunEnumTests()
    {
        Check("enum round-trip", () =>
        {
            var original = new EnumDto { Rarity = ItemRarity.Legendary };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<EnumDto>(dict);
            return result.Rarity == ItemRarity.Legendary;
        });

        Check("enum default value survives", () =>
        {
            var original = new EnumDto { Rarity = ItemRarity.Common };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<EnumDto>(dict);
            return result.Rarity == ItemRarity.Common;
        });
    }

    private void RunNestedDtoTests()
    {
        Check("nested DTO round-trip", () =>
        {
            var original = new OuterDto
            {
                Label = "chest_01",
                Contents = new InnerDto { ItemId = "core:fist", StackSize = 1 }
            };

            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<OuterDto>(dict);

            return result.Label == original.Label
                   && result.Contents != null
                   && result.Contents.ItemId == original.Contents.ItemId
                   && result.Contents.StackSize == original.Contents.StackSize;
        });

        Check("null nested DTO doesn't throw", () =>
        {
            var original = new OuterDto { Label = "empty_chest", Contents = null };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<OuterDto>(dict);
            return result.Label == original.Label && result.Contents == null;
        });
    }

    private void RunCollectionTests()
    {
        Check("List<int> round-trip", () =>
        {
            var original = new CollectionDto { Numbers = [1, 2, 3, 5, 8] };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<CollectionDto>(dict);
            return result.Numbers != null && result.Numbers.SequenceEqual(original.Numbers);
        });

        Check("string[] round-trip", () =>
        {
            var original = new CollectionDto { Tags = new[] { "wood", "stackable", "flammable" } };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<CollectionDto>(dict);
            return result.Tags != null && result.Tags.SequenceEqual(original.Tags);
        });

        Check("List<InnerDto> round-trip", () =>
        {
            var original = new CollectionDto
            {
                Items =
                [
                    new() { ItemId = "core:wood", StackSize = 16 },
                    new() { ItemId = "core:stone", StackSize = 32 }
                ]
            };

            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<CollectionDto>(dict);

            return result.Items != null
                   && result.Items.Count == 2
                   && result.Items[0].ItemId == "core:wood"
                   && result.Items[1].StackSize == 32;
        });

        Check("empty list survives as empty, not null", () =>
        {
            var original = new CollectionDto { Numbers = [] };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<CollectionDto>(dict);
            // Note: DictPack currently skips null/empty checks on the *field*, not the elements,
            // so an empty list still serializes as an empty Godot Array - confirm that here.
            return result.Numbers != null && result.Numbers.Count == 0;
        });
    }

    private void RunNullableTests()
    {
        Check("nullable int with value round-trips", () =>
        {
            var original = new NullableDto { OptionalDurability = 75 };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<NullableDto>(dict);
            return result.OptionalDurability == 75;
        });

        Check("nullable int left null stays null", () =>
        {
            var original = new NullableDto { OptionalDurability = null };
            var dict = DictPack.Pack(original);
            var result = DictPack.Unpack<NullableDto>(dict);
            return result.OptionalDurability == null;
        });
    }

    private void RunEdgeCaseTests()
    {
        Check("Serialize(null) returns empty dict, not throw", () =>
        {
            var dict = DictPack.Pack<PrimitiveDto>(null);
            return dict != null && dict.Count == 0;
        });

        Check("Deserialize(null) returns default instance, not throw", () =>
        {
            var result = DictPack.Unpack<PrimitiveDto>(null);
            return result != null;
        });

        Check("Deserialize(empty dict) returns default instance", () =>
        {
            var result = DictPack.Unpack<PrimitiveDto>([]);
            return result != null && result.Name == null && result.Count == 0;
        });

        Check("Deserialize with missing keys leaves defaults, doesn't throw", () =>
        {
            var partial = new Dictionary { ["Name"] = "partial_only" };
            var result = DictPack.Unpack<PrimitiveDto>(partial);
            return result.Name == "partial_only" && result.Count == 0;
        });

        Check("Deserialize with unexpected/extra key is ignored", () =>
        {
            var withJunk = new Dictionary { ["Name"] = "ok", ["TotallyNotAField"] = 12345 };
            var result = DictPack.Unpack<PrimitiveDto>(withJunk);
            return result.Name == "ok";
        });
    }

    // ---------------------------------------------------------------
    // Harness
    // ---------------------------------------------------------------

    private void Check(string label, Func<bool> test)
    {
        try
        {
            bool ok = test();
            if (ok)
            {
                _passed++;
                GD.PrintRich($"[color=green]  [PASS][/color] {label}");
            }
            else
            {
                _failed++;
                GD.PrintRich($"[color=red]  [FAIL][/color] {label}");
            }
        }
        catch (Exception ex)
        {
            _failed++;
            GD.PrintRich($"[color=red]  [FAIL][/color] {label} -- threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    // ---------------------------------------------------------------
    // Throwaway DTOs, only used by this test
    // ---------------------------------------------------------------

    private enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }

    [MessagePackObject]
    public class PrimitiveDto
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public int Count { get; set; }
        [Key(2)] public long LongValue { get; set; }
        [Key(3)] public float Weight { get; set; }
        [Key(4)] public double Precision { get; set; }
        [Key(5)] public bool IsMagic { get; set; }
        [Key(6)] public Color Tint { get; set; }
        [Key(7)] public Vector2 Position { get; set; }
        [Key(8)] public Vector3 SpawnPoint { get; set; }
    }

    private class EnumDto
    {
        public ItemRarity Rarity { get; set; }
    }

    private class InnerDto
    {
        public string ItemId { get; set; }
        public int StackSize { get; set; }
    }

    private class OuterDto
    {
        public string Label { get; set; }
        public InnerDto Contents { get; set; }
    }

    private class CollectionDto
    {
        public List<int> Numbers { get; set; }
        public string[] Tags { get; set; }
        public List<InnerDto> Items { get; set; }
    }

    private class NullableDto
    {
        public int? OptionalDurability { get; set; }
    }
}