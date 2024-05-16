using Godot;
using System;

public partial class TwinzBitScript : Node2D
{
	public bool Dead = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void OnCollision(Area2D area)
	{
		Dead = true;
		// may not actually let these die, idk
	}
}
