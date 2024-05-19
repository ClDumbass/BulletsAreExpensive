using Godot;
using System;

public partial class MineScript : Node2D
{
	[Export]
	public AudioStreamPlayer MineBeep { get; set; }
	[Export]
	Area2D BodyNode { get; set; }
	[Export]
	Area2D ExplosionNode { get; set; }
	public bool IsCircle { get; set; } = true;
	CircleShape2D ExplosionHitboxShape { get; set; }
	CollisionShape2D ExplosionRectA { get; set; }
	RectangleShape2D ExplosionRectAShape { get; set; }
	CollisionShape2D ExplosionRectB { get; set; }
	RectangleShape2D ExplosionRectBShape { get; set; }



	private float animationTimer = 0;
	/// <summary>
	/// 0 - Idle, no changes.
	/// 1 - Make a white circle that starts small and grows until it hits the edge, then disappears.
	/// 2 - Mine has been hit, just draw an expanding circle
	/// 
	/// state oscillates between 0 and 1 regularly, changes to 2 when mine is detonated
	/// </summary>
	private int animationStage = 0;

	/// <summary>
	/// A count down for disarming the mine
	/// </summary>
	private float disarmTimer = 2f;
	bool IsInCatchbox = false;

	private const float rectWidth = 24;
	public override void _Ready()
	{
		ExplosionHitboxShape = ExplosionNode.GetNode<CollisionShape2D>("Hitbox_Circle").Shape as CircleShape2D;

		ExplosionRectA = ExplosionNode.GetNode<CollisionShape2D>("Hitbox_RectA");
		ExplosionRectAShape = ExplosionRectA.Shape as RectangleShape2D;
		ExplosionRectB = ExplosionNode.GetNode<CollisionShape2D>("Hitbox_RectB");
		ExplosionRectBShape = ExplosionRectB.Shape as RectangleShape2D;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		animationTimer += (float)delta;


		if (animationStage != 2) {
			Position -= new Vector2(20f * (float)delta, 0);
			if (Position.X <= -12) {
				QueueFree();
				return;
			}

			if (IsInCatchbox) {
				disarmTimer -= (float)delta;
				QueueRedraw();
				if (disarmTimer <= 0) {
					QueueFree();
					return;
				}
			}
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

			if (animationTimer >= 1f) {
				QueueFree();
			}
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (animationStage == 2) {
			if (IsCircle) {
				ExplosionHitboxShape.Radius = GetRadiusFromTimer(animationTimer);
			}
			else {
				float radius = GetRectSizeFromTimer(animationTimer);
				ExplosionRectA.Position = new Vector2(0, 0);
				ExplosionRectAShape.Size = new Vector2(2 * radius, rectWidth);
				ExplosionRectB.Position = new Vector2(0, 0);
				ExplosionRectBShape.Size = new Vector2(rectWidth, 2 * radius);
			}
		}
	}

	public override void _Draw() {
		//if mine is not explode
		if (animationStage < 2) {
			//Draw the basic shape
			DrawCircle(Vector2.Zero, 12, Colors.White);
			DrawCircle(Vector2.Zero, 11, Colors.Black);
			//If it's blink time, draw the blink
			if (animationStage == 1) {
				float radius = animationTimer * 120;
				DrawCircle(Vector2.Zero, radius, Colors.Gray);
			}
			//Draw the indicator for which type of mine it is.
			if (IsCircle) {
				DrawArc(Vector2.Zero, 6, 0, MathF.Tau, 16, Colors.Orange, 1f);
			} else {
				DrawLine(new Vector2(0, -7), new Vector2(0, 7), Colors.Orange, 2);
				DrawLine(new Vector2(-7, 0), new Vector2(7, 0), Colors.Orange, 2);
				DrawCircle(Vector2.Zero, 2, Colors.Black);
			}

			if (IsInCatchbox) {
				DrawLine(new Vector2(-5, -5), new Vector2(-5 + 10 * disarmTimer, -5), Colors.Yellow, 2);
			}
		}
		else { //otherwise, it explode
			if (IsCircle) {
				DrawArc(Vector2.Zero, GetRadiusFromTimer(animationTimer), 0, MathF.Tau, 36, Colors.White, 5f);
			}
			else {
				float width = 2 + 10 * animationTimer;
				float radius = GetRectSizeFromTimer(animationTimer) - width / 2;
				DrawLine(new Vector2(-rectWidth / 2, radius), new Vector2(rectWidth / 2, radius), Colors.White, width);
				DrawLine(new Vector2(-rectWidth / 2, -radius), new Vector2(rectWidth / 2, -radius), Colors.White, width);
				DrawLine(new Vector2(radius, -rectWidth / 2), new Vector2(radius, rectWidth / 2), Colors.White, width);
				DrawLine(new Vector2(-radius, -rectWidth / 2), new Vector2(-radius, rectWidth / 2), Colors.White, width);
				DrawRect(new Rect2(new Vector2(-radius, -rectWidth/2), new Vector2(2*radius, rectWidth)), new Color(1f, 1f, 1f, 0.5f * (1 - animationTimer)));
				DrawRect(new Rect2(new Vector2(-rectWidth/2, -radius), new Vector2(rectWidth, 2*radius)), new Color(1f, 1f, 1f, 0.5f * (1 - animationTimer)));
			}
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

	private float GetRectSizeFromTimer(float timer) {
		return 240 * animationTimer;
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

	public void OnCatchboxEnter(Area2D area) {
		IsInCatchbox = true;
	}

	public void OnCatchboxExit(Area2D area) {
		IsInCatchbox = false;
	}
}
