using NullGarel.Sandboxnator.Chat;
using NullGarel.Sandboxnator.Entity;
using NullGarel.Sandboxnator.Registry;
using NullGarel.Util;
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