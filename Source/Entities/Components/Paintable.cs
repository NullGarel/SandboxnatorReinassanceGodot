using Godot;
using Godot.Collections;
using NullGarel.Util.ComponentSystem;
using NullGarel.Util.GodotHelpers;

namespace NullGarel.Sandboxnator.Placeables;

//[Tool]
public partial class Paintable : AbstractComponent<Placeable>
{
	[Export] private Array<MeshInstance3D> _targetMeshes;

	[Export]
	public Color CurrentColor
	{
		get => _currentColor;
		set
		{
			_currentColor = value;
			ApplyColor(value);
		}
	}

	private Color _currentColor = Colors.White;

	public override void _Ready()
	{
		SetMultiplayerAuthority(1);
		ApplyColor(_currentColor);
	}

	public void TriggerPaint(Color color)
	{
		if (!Multiplayer.IsServer())
			return;

		CurrentColor = color;
	}

	private void ApplyColor(Color color)
	{
		foreach (MeshInstance3D mesh in _targetMeshes)
		{
			if (!IsInstanceValid(mesh)) continue;
			mesh.ChangeMeshColor(color);
		}
	}
}
