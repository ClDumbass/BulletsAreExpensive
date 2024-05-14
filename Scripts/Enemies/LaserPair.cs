using Godot;
using System;

public partial class LaserPair : Node2D
{
	[Export]
	public Area2D EmitterA { get; set; }
	[Export]
	public Area2D EmitterB { get; set; }
	[Export]
	public CollisionShape2D CollisionShape { get; set; }
	[Export]
	public float Speed { get; set; } = 30f;

	public Vector2 InitialPosA { get; set; } = new Vector2(300, 20);
	public Vector2 InitialPosB { get; set; } = new Vector2(500, 180);
	// Called when the node enters the scene tree for the first time.

	private int animationState = 0;
	private float timer = 0;
	public override void _Ready()
	{
		EmitterA.Position = InitialPosA;
		EmitterB.Position = InitialPosB;
		SegmentShape2D lineCollider = new SegmentShape2D();
		lineCollider.A = InitialPosA;
		lineCollider.B = InitialPosB;
		CollisionShape.Shape = lineCollider;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += (float)delta;

		Position -= new Vector2((float)delta * Speed, 0);
		if (EmitterA.GlobalPosition.X <= -100 && EmitterB.GlobalPosition.X <= -100) {
			QueueFree();
		}

		if (timer > 0.1f) {
			animationState = -animationState + 1;
			QueueRedraw();
		}
	}

	public override void _Draw() {
		GD.Print("DRAW");
		DrawLine(EmitterA.Position, EmitterB.Position, new Color(1,0.3f,0.3f,0.7f), 3 + 2*animationState);
		base._Draw();
	}

	public void OnEmitterHit(Area2D area) {
		QueueFree();
	}
}
