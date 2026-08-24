using System;
using Godot;
using NullGarel.Sandboxnator.Network;
using NullGarel.Sandboxnator.Registry;
using NullGarel.Sandboxnator.Settings;
using NullGarel.Sandboxnator.UI;
using NullGarel.Sandboxnator.WorldAndScenes;
using NullGarel.Util.GodotHelpers;
namespace NullGarel.Sandboxnator;

public partial class SandboxnatorMain : Singleton<SandboxnatorMain>, ISettingsLoader
{
	[ExportCategory("Screens")]
	[Export] private CanvasLayer _mainMenu;
	[Export] private CanvasLayer _profileEditMenu;
	[Export] private CanvasLayer _settingsMenu;
	[Export] private CanvasLayer _worldMenu;
	[ExportCategory("Debug UI")]
	[Export] private CanvasLayer _debugLayer;
	[ExportCategory("World")]
	[Export] private Node3D _worldContainer;
	[Export] private PackedScene _worldScene;

	private World _world;

	public static World World => Instance?._world;
	public bool HasWorld { get => _worldContainer.GetChildCount() != 0; }

	private Node[] Screens => [
		_mainMenu,
		_profileEditMenu,
		_world,
		_worldMenu
	];

	private bool _settingsOpen;

	public override void _Ready()
	{


		UpdateSettingsData();
		GameRegistries.Instance.OnSettingsChanged += UpdateSettingsData;
		// Make the main menu the defualt when the game boots up.
		ActivateMainMenu();
		UiSoundManager.Instance.TryInstallSounds();
	}

	public void Activate(Node screen)
	{
		foreach (Node candidate in Screens)
		{
			if (candidate == null || !IsInstanceValid(candidate))
				continue;

			bool visibilityCondition = candidate == screen;

			if (candidate is CanvasLayer cl)
				cl.Visible = visibilityCondition;
			else if (candidate is CanvasItem citem)
				citem.Visible = visibilityCondition;
			else if (candidate is Node3D n3d)
				n3d.Visible = visibilityCondition;
			else
				throw new InvalidOperationException(
					$"{candidate.GetType().Name} has no supported visibility property.");
		}
	}

	public void ToggleSettingsMenu()
	{
		_settingsOpen = !_settingsOpen;
		_settingsMenu.Visible = _settingsOpen;
	}

	public void LoadWorld()
	{
		if (_world != null && IsInstanceValid(_world))
			return;

		_world = _worldScene.Instantiate<World>();
		_world.Name = "world";
		_worldContainer.AddChild(_world);
	}

	public void UnloadWorld()
	{
		if (_world == null || !IsInstanceValid(_world))
			return;

		_world.QueueFree();
		_world = null;
		// i wont call activate world here because it's responsibilyit opf the handshake
	}

	public void LeaveWorld()
	{
		NetworkManager.Instance.QuitConnection();

		UnloadWorld();
		ActivateMainMenu();

	}

	public void ActivateWorld() => Activate(_world);
	public void ActivateMainMenu() => Activate(_mainMenu);
	public void ActivateProfileEditMenu() => Activate(_profileEditMenu);
	public void ActivateWorldMenu() => Activate(_worldMenu);

	//Apply game-wide settings data.
	public void UpdateSettingsData()
	{
		GameSettingsData d = GameRegistries.Instance.SettingsData;

		//full screen
		DisplayServer.WindowMode windowMode = d.FullScreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
		DisplayServer.WindowSetMode(windowMode);

		//debug UI
		_debugLayer.Visible = d.DebugUiEnabled;

		//render scale
		GetViewport().Scaling3DScale = d.RenderScale;

		//vsync
		DisplayServer.VSyncMode vSyncMode = d.VSync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled;
		DisplayServer.WindowSetVsyncMode(vSyncMode);


	}

}
