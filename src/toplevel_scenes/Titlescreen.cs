using System;
using Godot;
using Yotf;

public partial class Titlescreen : Control
{
    Button playBtn,
        quitBtn;

    PackedScene mainScene = GD.Load<PackedScene>("res://src/toplevel_scenes/grid_level_test.tscn");

    public override void _Ready()
    {
        playBtn = GetNode<Button>("PlayButton");
        quitBtn = GetNode<Button>("Quit Button");
        playBtn.Pressed += Play;
        quitBtn.Pressed += Quit;
    }

    private void Play()
    {
        Audio.PlaySfx(Sfx.UISelect);
        GetTree().ChangeSceneToPacked(mainScene);
    }

    private void Quit()
    {
        GetTree().Quit();
    }
}
