using Godot;
using System;

public partial class EnemyMasterScript : Node
{
	[Export]
	public PackedScene flybyEnemy { get; set; }
	[Export]
	public PackedScene bomberEnemy { get; set; }
	[Export]
	public PackedScene spikeballEnemy { get; set; }
	[Export]
	public PackedScene clumpusScene { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public Node MakeSpikeballEnemy(float height) {
		Node2D spikeballClone = spikeballEnemy.Instantiate() as Node2D;
		spikeballClone.Position = new Vector2(500, height);
		return spikeballClone;
	}

	public Node MakeFlybyBulletEnemy(bool flip) {
		FlybyEnemyScript flybyEnemyClone = flybyEnemy.Instantiate() as FlybyEnemyScript;

		flybyEnemyClone.Flip = flip;
		if (flip) {
			flybyEnemyClone.Position = new Vector2(480 + 32, 270 - 16);
		}
		else {
			flybyEnemyClone.Position = new Vector2(480 + 32, 16);
		}
		return flybyEnemyClone;
	}

	public Node MakeFlybyBombEnemy(bool flip) {
		BomberEnemyScript bomberEnemyScript = bomberEnemy.Instantiate() as BomberEnemyScript;

		bomberEnemyScript.Flip = flip;
		if (flip) {
			bomberEnemyScript.Position = new Vector2(480 + 32, 270 - 16);
		}
		else {
			bomberEnemyScript.Position = new Vector2(480 + 32, 16);
		}
		return bomberEnemyScript;
	}

	public Node MakeClumpus() {
		ClumpusScript clumpusScript = clumpusScene.Instantiate() as ClumpusScript;

		clumpusScript.PlayerNode = GetNode("../Player") as Node2D;
		clumpusScript.EnemyMaster = this;

		return clumpusScript;
	}
}
