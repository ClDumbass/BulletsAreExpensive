using Godot;
using System;

public partial class Absorbed : Node2D
{
	public float Radius {get; set;}
	// Called when the node enters the scene tree for the first time.
	public Node2D player { get; set; }

	const float ABSORB_TIME = 1f;
	float timer = ABSORB_TIME;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer -= (float)delta;
		if (timer <= 0) {
			QueueFree();
			return;
		}
		float percentage = (float)delta / timer;

		Vector2 path = GlobalPosition - player.GlobalPosition;
		GlobalPosition -= new Vector2(path.X * percentage, path.Y * percentage);

		if (Radius > 1.0f && delta > 0) {
			Radius -= 1.5f * Radius * (float)delta;
			QueueRedraw();
		}
		if (Radius < 1) {
			Radius = 1;
		}
	}

	public override void _Draw() {
		DrawCircle(Vector2.Zero, Radius, Colors.White);
	}
}
