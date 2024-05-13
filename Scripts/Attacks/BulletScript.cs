using GameJamBulletHell.Scripts.Attacks;
using Godot;
using System;

public partial class BulletScript : AttackBaseScript
{
	[Export]
	public Vector2 InitialPosition { get; set; }
	[Export]
	///Normal vector representing which direction the bullet is traveling.
	public Vector2 Direction { get; set; }
	[Export]
	public float Speed { get; set; }

	private Area2D bullet;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bullet = GetNode<Area2D>("Bullet");
		bullet.Position = InitialPosition;
		bullet.LookAt(new Vector2(InitialPosition.X + Direction.X * 1000, InitialPosition.Y + Direction.Y *1000));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float distance = (float)delta * Speed;
		bullet.Position += new Vector2(Direction.X * distance, Direction.Y * distance);

		if (bullet.Position.X < -100 || bullet.Position.X > 580 ||
			bullet.Position.Y < -100 || bullet.Position.Y > 370) {
			QueueFree();
		}
	}
	private void OnCollision(Area2D area) {
		QueueFree();
	}
}
