using Godot;
using System;
using Yotf;

public partial class Titlescreen : Control
{
	Button playBtn, quitBtn;
	Node mainScene;
	public override void _Ready()
	{
		playBtn = GetNode<Button>("PlayButton");
		quitBtn = GetNode<Button>("Quit Button");
		playBtn.Pressed += Play;
		quitBtn.Pressed += Quit;
		mainScene = ResourceLoader.Load<PackedScene>("res://src/toplevel_scenes/grid_level_test.tscn").Instantiate();
	}

	private void Play()
	{
		Audio.PlaySfx(Sfx.UISelect);
		GoToScene(mainScene);
	}
	
	private void Quit()
	{
		GetTree().Quit();
	}
	
	private void GoToScene(Node node){
		var tree = GetTree();
		var cur_scene = tree.GetCurrentScene();
		tree.GetRoot().AddChild(node);
		tree.GetRoot().RemoveChild(cur_scene);
		tree.SetCurrentScene(node);
	}
}
