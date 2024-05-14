using Godot;
using System;

public partial class FlybyEnemyScript : Node2D
{
	[Export]
	PackedScene bulletMaster { get; set; }
	[Export]
	public float Speed { get; set; } = 30.0f;
	[Export]
	public bool Flip {get; set;} = false;
	[Export]
	public AudioStreamPlayer BulletFireSound { get; set; }

	private float timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Sprite2D>("Sprite").FlipV = Flip;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!Visible) return;

		timer += (float)delta;
		if (timer > 0.4) {
			timer = 0;
			if (Flip) {
				FireBullet(Vector2.Up);
			} else {
				FireBullet(Vector2.Down);
			}
			BulletFireSound.Play();
		}
		Position += new Vector2((float)(-Speed * delta), 0);

		if (Position.X <= -100) {
			QueueFree();
		}
	}

	private void FireBullet(Vector2 direction) {
		BulletScript bulletClone = bulletMaster.Instantiate() as BulletScript;
		bulletClone.InitialPosition = Position;
		bulletClone.Direction = direction;
		bulletClone.Speed = 40.0f;
		bulletClone.Visible = true;
		GetParent().AddChild(bulletClone);
	}
	private void OnCollision(Area2D area) {
		QueueFree();
	}
}

