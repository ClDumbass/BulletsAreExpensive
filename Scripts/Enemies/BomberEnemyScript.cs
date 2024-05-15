using Godot;
using System;

public partial class BomberEnemyScript : Node2D
{
	[Export]
	PackedScene BombScene { get; set; }
	[Export]
	public float Speed { get; set; } = 30.0f;
	[Export]
	public bool Flip { get; set; } = false;
	[Export]
	public AudioStreamPlayer BombFireSound { get; set; }

	private float timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() { 
		GetNode<Sprite2D>("Sprite").FlipV = Flip;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (!Visible) return;

		timer += (float)delta;
		if (timer > 3.75) {
			timer = 0;
			BombScript bombClone;

			for (int i = 0; i < 7; i++) {
				bombClone = BombScene.Instantiate() as BombScript;

				bombClone.InitialPosition = GlobalPosition;
				if (Flip) {
					bombClone.TargetLocation = GlobalPosition - new Vector2(0, 32 * (i + 1));
				} else {
					bombClone.TargetLocation = GlobalPosition + new Vector2(0, 32 * (i + 1));
				}
				bombClone.Radius = 20;
				bombClone.Speed = 10 + 10 * (i + 1);
				bombClone.Visible = true;

				GetParent().AddChild(bombClone);
				BombFireSound.Play();
			}
		}
		Position += new Vector2((float)(-Speed * delta), 0);

		if (Position.X <= -100) {
			QueueFree();
		}
	}
	private void OnCollision(Area2D area) {
		QueueFree();
	}
}
