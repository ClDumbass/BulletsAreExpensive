using Godot;
using System;

public partial class GigaspinnyScript : Node2D
{
	[Export]
	PackedScene bulletMaster { get; set; }
	[Export]
	AudioStreamPlayer BulletSound { get; set; }
	[Export]
	float Speed { get; set; } = 30;

	private float timer = 0;
	private int health = 10;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		Position = new Vector2(Position.X - Speed * (float)delta, Position.Y);
		timer += (float)delta;
		Rotation += (float)delta * (MathF.PI);

		if (timer > 0.1f) {
			timer = 0;
			FireBullet(Vector2.Up.Rotated(Rotation));
			FireBullet(Vector2.Down.Rotated(Rotation));
			FireBullet(Vector2.Left.Rotated(Rotation));
			FireBullet(Vector2.Right.Rotated(Rotation));
			BulletSound.Play();
		}

		if (Position.X <= -100) {
			QueueFree();
		}
	}
	private void FireBullet(Vector2 direction) {
		BulletScript bulletClone = bulletMaster.Instantiate() as BulletScript;
		bulletClone.InitialPosition = Position;
		bulletClone.Direction = direction;
		bulletClone.Visible = true;
		bulletClone.Speed = -30 * Mathf.Cos(direction.Angle()) + 60;
		GetParent().AddChild(bulletClone);
	}
	private void OnCollision(Area2D area) {
		--health;
		if (health <= 0) {
			QueueFree();
		}
	}
}
