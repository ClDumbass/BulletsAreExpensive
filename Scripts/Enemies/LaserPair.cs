using Godot;
using System;

public partial class LaserPair : Node2D
{
	[Export]
	public Area2D EmitterA { get; set; }
	[Export]
	public Area2D EmitterB { get; set; }
	[Export]
	public BeamScript Beam { get; set; }
	[Export]
	public float Speed { get; set; } = 30f;

	public Vector2 InitialPosA { get; set; } = new Vector2(300, 20);
	public Vector2 InitialPosB { get; set; } = new Vector2(500, 180);
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		EmitterA.Position = InitialPosA;
		EmitterB.Position = InitialPosB;
		Beam.PositionA = InitialPosA;
		Beam.PositionB = InitialPosB;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position -= new Vector2((float)delta * Speed, 0);
		if (EmitterA.GlobalPosition.X <= -100 && EmitterB.GlobalPosition.X <= -100) {
			QueueFree();
		}
	}

	public void OnEmitterHit(Area2D area) {
		QueueFree();
	}
}
