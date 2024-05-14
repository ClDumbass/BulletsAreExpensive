using Godot;
using System;

public partial class SpikeballEnemyScript : Node2D
{
	[Export]
	PackedScene bulletMaster { get; set; }
	[Export]
	AudioStreamPlayer BulletSound { get; set; }
	[Export]
	float Speed { get; set; } = 49;

	/// <summary>
	/// 0 = stopped, fires bullets when exiting stage
	/// 1 = stopped
	/// 2 = spin
	/// </summary>
	private int sequence = 1;
	private float timer = 0;
	private float targetRotation = 3 * MathF.PI / 4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position = new Vector2(Position.X - Speed * (float)delta, Position.Y);
		timer += (float)delta;

		switch (sequence) {
			case 0:
				if (timer > 0.15f) {
					sequence = 1;
					timer = 0;
					FireBullet(Vector2.Up.Rotated(Rotation));
					FireBullet(Vector2.Down.Rotated(Rotation));
					FireBullet(Vector2.Left.Rotated(Rotation));
					FireBullet(Vector2.Right.Rotated(Rotation));
					BulletSound.Play();
				}
				break;
			case 1:
				if (timer > 0.15f) {
					timer = 0;
					sequence = 2;
				}
				break;
			case 2:
				//Trying to make it turn exactly 135 degrees every 0.3s
				Rotation += (float)delta * (3 * MathF.PI / 1.2f);

				if (Rotation >= targetRotation) { //using this as the seqeuence change condition allows forcing it on the exact correct frame
					sequence = 0;
					timer = 0;
					Rotation = targetRotation; //force away the little errors
					targetRotation = Rotation + 3 * MathF.PI / 4;
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
