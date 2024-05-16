using Godot;
using System;

public partial class VertibooEnemyScript : Node2D
{
	[Export]
	PackedScene bulletMaster { get; set; }
	[Export]
	AudioStreamPlayer BulletSound { get; set; }
	[Export]
	float Speed { get; set; } = 49;

	/// <summary>
	/// If true, comes in from the bottom of the screen instead of the top.
	/// Does NOT modify firedirection. That must be set manually and the default remains down.
	/// </summary>
	[Export]
	public bool flip = false;

	public Vector2 FireDirection { get; set; } = Vector2.Down;

	/// <summary>
	/// 0 = coming on screen
	/// 1 = firing bullets
	/// 2 = leaving screen
	/// </summary>
	private int sequence = 0;
	private float timer = 0;
	private float fireTimer = 0;
	private float targetRotation = 3 * MathF.PI / 4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		timer += (float)delta;

		switch (sequence) {
			case 0:
				if (flip) {
					Position = new Vector2(Position.X, Position.Y - Speed * (float)delta);
					if (Position.Y <= 270 - 16) {
						Position = new Vector2(Position.X, 270 - 16);
						sequence = 1;
						timer = 0;
					}
				} else {
					Position = new Vector2(Position.X, Position.Y + Speed * (float)delta);
					if (Position.Y >= 16) {
						Position = new Vector2(Position.X, 16);
						sequence = 1;
						timer = 0;
					}
				}
				break;
			case 1:
				fireTimer += (float)delta;
				if (fireTimer >= 0.2f) {
					fireTimer -= 0.2f;
					FireBullet(FireDirection);
				}
				if (timer > 8f) {
					timer = 0;
					sequence = 2;
				}
				break;
			case 2:
				if (flip) {
					Position = new Vector2(Position.X, Position.Y + Speed * (float)delta);
					if (Position.Y >= 270 + 16) {
						QueueFree();
					}
				} else {
					Position = new Vector2(Position.X, Position.Y - Speed * (float)delta);
					if (Position.Y <= -16) {
						QueueFree();
					}
				}
				break;
			default:
				sequence = 0;
				timer = 0;
				break;
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
		GetParent().AddChild(bulletClone);
	}
	private void OnCollision(Area2D area) {
		QueueFree();
	}
}
