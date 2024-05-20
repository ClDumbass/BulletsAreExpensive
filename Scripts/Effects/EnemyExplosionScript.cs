using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyExplosionScript : Node2D
{
	public float MaxRadius { get; set; } = 8f;
	public float MaxTime { get; set; } = 6f;
	public Rect2 SpawnArea { get; set; }

	private float despawnTimer = 0;
	private float spawnTimer = 0;
	private List<SingleExplosion> list = new List<SingleExplosion>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		despawnTimer += (float)delta;
		spawnTimer += (float)delta;
		QueueRedraw();
		if (despawnTimer > MaxTime + 1f) {
			QueueFree();
		}

		if (spawnTimer > 0.2f && despawnTimer < MaxTime) {
			float randomPositionX = SpawnArea.Position.X + Random.Shared.Next((int)SpawnArea.Size.X);
			float randomPositionY = SpawnArea.Position.Y + Random.Shared.Next((int)SpawnArea.Size.Y);
			Vector2 randomPosition = new Vector2(randomPositionX, randomPositionY);

			SingleExplosion newExp = new SingleExplosion();
			newExp.Timer = 1f;
			newExp.Position = randomPosition;
			list.Add(newExp);
		}

		foreach (SingleExplosion e in list) {
			e.Timer -= (float)delta;
			if (e.Timer > 0) {
				e.Radius = MaxRadius * e.Timer;
			}
		}
	}

	public override void _Draw() {
		foreach (SingleExplosion e in list) {
			if (e.Timer > 0) {
				DrawCircle(e.Position, e.Radius, Colors.White);
			}
		}
	}

	private class SingleExplosion {
		public float Radius { get; set; }
		public Vector2 Position { get; set; }
		public float Timer { get; set; }
	}
}
