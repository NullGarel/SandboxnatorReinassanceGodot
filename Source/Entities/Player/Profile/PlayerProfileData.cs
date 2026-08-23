using Godot;
using NullGarel.Util.Log;
using NullGarel.Sandboxnator.Registry;
using NullGarel.Util;
using NullGarel.Util.GodotHelpers;

namespace NullGarel.Sandboxnator.Entity;

[GlobalClass]
public partial class PlayerProfileData : Resource
{
    [Export]
    [Packable]
    public string PlayerName { get; set; } = "DEFAULT_PLAYER";

    private Color _playerColor = Colors.White;
    [Export]
    [Packable]
    public Color PlayerColor
    {
        get => _playerColor;
        set => _playerColor = value;
    }
    
    [Export]
    [Packable]
    public string PlayerFaceId { get; set; } = "TinySmile";

    public PlayerProfileData() { }

    public PlayerProfileData(string name, Color color, string faceId)
    {
        PlayerName = name;
        PlayerColor = color;
        PlayerFaceId = faceId;
    }

    public void RandomizeProfile()
    {
        GD.Randomize();
        PlayerName = FillNameField();
        PlayerColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
        PlayerFaceId = GameRegistries.Instance.PlayerFaceRegistry.GetRandomEntry().Value.PlayerFaceId;
    }

    private string FillNameField()
    {
        NameGenerator nameGen = NameGenerator.Create();
        if (GD.Randf() <= 0.7f) nameGen.UseDictedPatterns();
        else nameGen.UseSimplePatterns();

        string name = nameGen.GenerateName();
        return char.ToUpper(name[0]) + name.Substring(1);
    }
    public override string ToString()
    {
        return $"[color={PlayerColor.ToHtml()}] name:{PlayerName} colour:{PlayerColor.ToHtml()}[/color] faceID:{PlayerFaceId}";
    }

    public void PrintProperties(string message = "")
    {
        NcLogger.Log($"{message} : {ToString()}");
    }
}