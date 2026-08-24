using System;
using System.Linq;
using System.Text;
using Godot;
using Godot.Collections;
using NullGarel.Sandboxnator.Chat;
using NullGarel.Sandboxnator.Data;
using NullGarel.Sandboxnator.Entity;
using NullGarel.Sandboxnator.Placeables;
using NullGarel.Sandboxnator.Registry;
using NullGarel.Util;
using NullGarel.Util.GodotHelpers;
using NullGarel.Util.Log;

namespace NullGarel.Sandboxnator.Commands;

public class CommandRegistryManager : IRegistryManager
{
    /// <summary>
    /// Register game chat commands
    /// </summary>
    public void Register()
    {
        RegisterCommand(new CommandHelp());
        RegisterCommand(new CommandPlayers());
        RegisterCommand(new CommandGetPos());
        RegisterCommand
        (
            new ChatCommand()
            .WithName("serialize")
            .WithDescription("serializer")
            .WithHandler((ctx) =>
            {
                Array<Dictionary> placeables = [];
                foreach (var item in SandboxnatorMain.World.GetPlaceables())
                {
                    var serialized = item.Serialized();
                    placeables.Add(serialized);
                    GD.Print(Json.Stringify(serialized));
                }

                Dictionary saveDict = new() { ["Placeables"] = placeables };

                byte[] buf = GD.VarToBytes(saveDict);
                byte[] zlibbedBuf = ZlibPack.Pack(buf);

                StringBuilder sb = new();
                sb.AppendLine("Done serializing!");
                sb.AppendLine($"Uncompressed: {buf.Length}");
                sb.AppendLine($"Compressed  : {zlibbedBuf.Length}");
                sb.AppendLine($"Delta       : {buf.Length - zlibbedBuf.Length}");
                //sb.AppendLine($"JSON        : {Json.Stringify(saveDict)}");
                sb.AppendLine($"Base64      : {Convert.ToBase64String(zlibbedBuf)}");
                ctx.Reply(sb.ToString());
            })
        );
        RegisterCommand
        (
            new ChatCommand()
            .WithName("loadb64")
            .WithDescription("deserializer")
            .WithHandler((ctx) =>
            {
                string b64buff = ctx.Args[0];
                byte[] zlibbedBuff = Convert.FromBase64String(b64buff);
                byte[] buf = ZlibPack.Unpack(zlibbedBuff);
                Dictionary saveDict = (Dictionary)GD.BytesToVar(buf);
                Array<Dictionary> placeables = (Array<Dictionary>)saveDict["Placeables"];

                int loaded = 0;
                foreach (var p in placeables)
                {
                    var spawnData = DictPack.Unpack<PlaceableSpawnData>(p);
                    Placeable placeable = (Placeable)SandboxnatorMain.World.PlaceableSpawner.Spawn(p);
                    if (placeable == null)
                    {
                        GD.PushWarning($"loadb64: failed to spawn placeable for ItemId '{spawnData.ItemId}', skipping.");
                        continue;
                    }

                    if (p.TryGetValue("ComponentHolder", out var holderVariant))
                    {
                        var holderDict = holderVariant.AsGodotDictionary();
                        if (holderDict.TryGetValue("Components", out var componentsVariant))
                        {
                            foreach (var entryVariant in componentsVariant.AsGodotArray())
                            {
                                DictPackExtensions.HydrateComponent(placeable.componentHolder, entryVariant.AsGodotDictionary());
                            }
                        }
                    }

                    loaded++;
                }

                ctx.Reply($"Loaded {loaded}/{placeables.Count} placeable(s).");
            })
        );
    }


    public static void RegisterCommand(ChatCommand command)
    {
        if (GameRegistries.Instance.CommandRegistry.Contains(command.Name))
        {
            NcLogger.Log($"[Commands] Command '{command.Name}' already exists!", NcLogger.LogType.Error);
            return;
        }

        GameRegistries.Instance.CommandRegistry.Register(command.Name.ToLower(), command);
        NcLogger.Log($"[Commands] Registered '{command.Name}'", NcLogger.LogType.Register);
    }

    public static bool ExecuteCommand(Player sender, string rawInput)
    {

        if (!rawInput.StartsWith("!"))
            return false;

        string[] split = rawInput.Substring(1).Split(' ');
        string commandKeyName = split[0].ToLower();
        string[] args = split.Length > 1 ? split[1..] : [];

        try
        {
            ChatCommand cmd = GameRegistries.Instance.CommandRegistry.Get(commandKeyName);
            if (cmd != null)
            {
                CommandContext context = new(rawInput, commandKeyName, args, sender);
                cmd.Execute(context);
                return true;
            }
            else
            {
                return SendInvalidCommandWarning(commandKeyName, sender);
            }
        }
        catch (System.Exception)
        {
            return SendInvalidCommandWarning(commandKeyName, sender);
        }


    }

    private static bool SendInvalidCommandWarning(string commandKeyName, Player sender)
    {
        ChatManager.Instance.SendPlayerlessMessage($"Unknown command or invalid arguments when attempting to execute: {commandKeyName}, try typing \"!help\" in order to see available in-game commands and their respective usage instructions.", sender.componentHolder.EntityId);
        //Seems counterintuitive, but returnig true means that there was an attempt, not necessarily success.
        return true;
    }
}