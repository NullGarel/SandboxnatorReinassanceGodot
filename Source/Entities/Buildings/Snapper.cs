using Godot;
namespace NullGarel.Sandboxnator.Placeables;

public partial class Snapper : Node3D
{
    public bool InsideBody { get; private set; }

    public override void _EnterTree()
    {
        SandboxnatorMain.World.snappers.Add(this);
    }

    public override void _ExitTree()
    {
        if (SandboxnatorMain.World == null) return;
        SandboxnatorMain.World.snappers.Remove(this);
    }

    public void _on_area_3d_body_entered(Node3D body)
    {
        if (body is Placeable)
        {
            InsideBody = true;
        }
    }

    public void _on_area_3d_body_exited(Node3D body)
    {
        if (body is Placeable)
        {
            InsideBody = false;
        }
    }
}
