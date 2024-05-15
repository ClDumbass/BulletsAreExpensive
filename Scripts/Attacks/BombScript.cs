using GameJamBulletHell.Scripts.Attacks;
using Godot;
using System;

public partial class BombScript : AttackBaseScript
{
	public override int Damage { get { return 10; } }

	[Export]
	public AudioStreamPlayer ExplosionSound { get; set; }
	[Export]
	public Vector2 InitialPosition { get; set; }
	[Export]
	public Vector2 TargetLocation { get; set; }
	[Export]
	public float Radius { get; set; }
	[Export]
	public double Speed { get; set; }

	private Node2D bombNode;
	private Node2D explosionNode;
	private Vector2 progressLine;

	private double timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		bombNode = GetNode<Node2D>("Bomb");
		bombNode.Position = InitialPosition;
		bombNode.GetNode<Sprite2D>("Sprite").LookAt(TargetLocation);
		bombNode.Visible = true;


		explosionNode = GetNode<Node2D>("Explosion");
		explosionNode.Scale = new Vector2(Radius / 16, Radius/16);
		explosionNode.Visible = false;

		progressLine = TargetLocation - InitialPosition;
		progressLine = progressLine.Normalized();


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (!Visible) return;

		timer += delta;
		if (explosionNode.Visible) {
			if (timer >= .1f) {
				QueueFree();
			}
		}

		bombNode.Position = new Vector2((float)(InitialPosition.X + progressLine.X * timer * Speed), (float)(InitialPosition.Y + progressLine.Y * timer * Speed));

		if (Math.Sign(TargetLocation.X - bombNode.Position.X) * Math.Sign(progressLine.X) < 0 || 
			Math.Sign(TargetLocation.Y - bombNode.Position.Y) * Math.Sign(progressLine.Y) < 0) {
			bombNode.Visible = false;
			explosionNode.Visible = true;
			explosionNode.Position = TargetLocation;
			timer = 0;
			ExplosionSound.Play();

		} else {
			QueueRedraw();
		}
	}

	public override void _Draw() {
		if (bombNode.Visible) {
			DrawCircle(TargetLocation, Radius, new Color(1, 0, 0, 0.3f));
			DrawArc(TargetLocation, Radius, 0, MathF.Tau, 16, Colors.Black, 1);
			DrawLine(bombNode.Position, TargetLocation, Colors.Yellow, 1); //note: replace this later with a flashing animation when X seconds out
		}
	}

	private void OnCollision(Area2D area) {
		explosionNode.Position = bombNode.Position;
		explosionNode.Visible = true;
		bombNode.Visible = false;
		timer = 0;
		ExplosionSound.Play();
	}
}
