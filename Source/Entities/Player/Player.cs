using Godot;
using NullGarel.Sandboxnator.UI;
using NullGarel.Util.ComponentSystem;
namespace NullGarel.Sandboxnator.Entity;

//The most basic and central class to a player.
public partial class Player : CharacterBody3D
{
	//Components
	//Since characterbody3d doesn't inherit AbstractComponent, this reference is necessary to set the
	//entity ID.
	[Export] public ComponentHolder componentHolder;

	public PlayerProfileData ProfileData { get; set; }


	//Individual client graphical user interface and camera holders
	[Export] public Camera3D camera;
	[Export] public Node hud;

	[Export] public PlayerSounds playerSounds;

	public override void _EnterTree()
	{
		componentHolder.EntityId = int.Parse(Name);
		SetMultiplayerAuthority(componentHolder.EntityId);

		if (IsMultiplayerAuthority())
		{
			//install ui sound on player Hud
			UiSoundManager.Instance.TryInstallSounds(hud);
		}
		else
		{
			//disable camera and HUD from other players
			camera.Current = false;
			//make sure HUD is client side only
			hud.QueueFree();
		}

	}

	public override void _ExitTree()
	{
		//PlayerManager.Instance.RemovePlayer(componentHolder.entityId); // Clean up references
	}

}
