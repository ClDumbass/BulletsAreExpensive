using Godot;
using System;

public partial class MineScript : Node2D
{
	[Export]
	AudioStreamPlayer MineBeep { get; set; }
	[Export]
	Area2D BodyNode { get; set; }
	[Export]
	Area2D ExplosionNode { get; set; }
	CircleShape2D ExplosionHitboxShape { get; set; }

	private float animationTimer = 0;
	/// <summary>
	/// 0 - Idle, no changes.
	/// 1 - Make a white circle that starts small and grows until it hits the edge, then disappears.
	/// 2 - Mine has been hit, just draw an expanding circle
	/// </summary>
	private int animationStage = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ExplosionHitboxShape = ExplosionNode.GetNode<CollisionShape2D>("Hitbox").Shape as CircleShape2D; 
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		animationTimer += (float)delta;

		if (animationStage != 2) {
			Position -= new Vector2(20f * (float)delta, 0);
		}

		if (animationStage == 0 && animationTimer >= 0.4f) {
			animationStage = 1;
			animationTimer = 0;
			MineBeep.Play();
		} 
		if (animationStage == 1) {
			QueueRedraw();
			if (animationTimer >= 0.1f) {
				animationStage = 0;
				animationTimer = 0;
			}
		}
		if (animationStage == 2) {
			QueueRedraw();
			MineBeep.Stop();
			ExplosionHitboxShape.Radius = GetRadiusFromTimer(animationTimer);
			if (animationTimer >= 1f) {
				QueueFree();
			}
		}
	}

	public override void _Draw() {
		if (animationStage < 2) {
			DrawCircle(Vector2.Zero, 12, Colors.White);
			DrawCircle(Vector2.Zero, 11, Colors.Black);
			if (animationStage == 1) {
				float radius = animationTimer * 120;
				DrawCircle(Vector2.Zero, radius, Colors.White);
			}
			DrawLine(new Vector2(0, -7), new Vector2(0, 7), Colors.Orange, 2);
			DrawLine(new Vector2(-7, 0), new Vector2(7, 0), Colors.Orange, 2);
			DrawCircle(Vector2.Zero, 2, Colors.Black);
		} else {
			DrawArc(Vector2.Zero, GetRadiusFromTimer(animationTimer), 0, MathF.Tau, 36, Colors.White, 5f);
		}
	}

	/// <summary>
	/// Determines the radius of explosion from current time.
	/// </summary>
	/// <param name="timer"></param>
	/// <returns></returns>
	private float GetRadiusFromTimer(float timer ) {
		return 551 * animationTimer;
	}

	public void OnStruck(Area2D area) {
		BodyNode.SetDeferred("monitorable", false);
		BodyNode.SetDeferred("monitoring", false);
		ExplosionNode.SetDeferred("monitorable", true);
		ExplosionNode.SetDeferred("monitoring", true);
		animationStage = 2;
		animationTimer = 0;
		QueueRedraw();
	}
}
