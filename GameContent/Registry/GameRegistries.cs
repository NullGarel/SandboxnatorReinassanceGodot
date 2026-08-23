using Godot;
using System;
using NullGarel.Sandboxnator.Commands;
using NullGarel.Sandboxnator.Item;
using NullGarel.Sandboxnator.Settings;
using NullGarel.Util;
using NullGarel.Util.Log;
using NullGarel.Util.GodotHelpers;
using NullGarel.Sandboxnator.Entity.PlayerCosmetics;
using NullGarel.Util.IO;
using NullGarel.Sandboxnator.Data;
using System.Reflection;
namespace NullGarel.Sandboxnator.Registry;

/// <summary>
/// Monolithic class for game-wide data
/// </summary>
public partial class GameRegistries : Singleton<GameRegistries>
{
    // Database
    [Export] public GameContentDatabase ContentDatabase { get; private set; }

    //Registries
    public Registry<ItemData> ItemRegistry { get; set; } = new();
    public Registry<PlayerFaceData> PlayerFaceRegistry { get; set; } = new();
    public Registry<ChatCommand> CommandRegistry { get; set; } = new();
    public Registry<PackedScene> PlaceableRegistry { get; set; } = new();

    //ingame settings
    private GameSettingsData _settingsData = new();
    public GameSettingsData SettingsData
    {
        get => _settingsData;
        set
        {
            _settingsData = value;
            SaveLoader.WriteResource(SaveFolder.Config, UserSettingsFileName, SettingsData);
            OnSettingsChanged?.Invoke();
        }
    }
    public const string UserSettingsFileName = "UserSettings.tres";
    public Action OnSettingsChanged { get; set; }

    public static string GameVersion => ProjectSettings.GetSetting("application/config/version").ToString();

    public override void _Ready()
    {
        SaveLoader.SetupGameSavePath("Sandboxnator", GameVersion);
        NcLogger.Log("GAME REGISTRIES INITIALIZED", NcLogger.LogType.Register);
        try
        {
            LoadUserSettings();
        }
        catch (System.IO.FileNotFoundException)
        {
            //To avoid null reference exceptions.
            LoadDefaultSettings();
        }
        InitializeRegistries();
    }

    private void LoadDefaultSettings()
    {
        var settings = GD.Load<GameSettingsData>("res://GameContent/DefaultSettings.tres");
        if (settings == null)
        {
            NcLogger.Log("Failed to load default settings! Creating fallback.", NcLogger.LogType.Error);
            SettingsData = new();
        }
        else
        {
            SettingsData = settings;
        }
    }

    private void LoadUserSettings()
    {
        var settings = SaveLoader.ReadResource<GameSettingsData>(SaveFolder.Config, UserSettingsFileName);
        if (settings == null)
        {
            NcLogger.Error("User settings do not exist yet");
            throw new System.IO.FileNotFoundException();
        }
        NcLogger.Info("User settings do in fact exist.");
        SettingsData = settings;
    }

    private void InitializeRegistries()
    {
        ComponentRegistry.AutoRegisterAll(Assembly.GetCallingAssembly());
        ItemRegistryManager itemRegistryManager = new();
        itemRegistryManager.Register();
        CommandRegistryManager commandRegistryManager = new();
        commandRegistryManager.Register();
        PlayerFaceRegistryManager playerFaceRegistryManager = new();
        playerFaceRegistryManager.Register();
    }

}