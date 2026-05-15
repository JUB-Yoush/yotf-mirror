using Godot;
using System;

public partial class Titlescreen : Control
{
	Button playBtn, quitBtn;
	Node mainScene;
	public override void _Ready()
	{
		playBtn = GetNode<Button>("PlayButton");
		quitBtn = GetNode<Button>("PlayButton");
		playBtn.Pressed += Play;
		quitBtn.Pressed += Quit;
		mainScene = ResourceLoader.Load<PackedScene>("res://src/toplevel_scenes/main.tscn").Instantiate();
	}

	private void Play()
	{
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
