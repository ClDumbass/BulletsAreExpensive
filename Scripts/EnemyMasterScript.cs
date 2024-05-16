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
	[Export]
	public PackedScene laserEmitterEnemy { get; set; }
	[Export]
	public PackedScene peekabooEnemy { get; set; }
	[Export]
	public PackedScene vertibooEnemy { get; set; }
	[Export]
	public PackedScene gigaspinnyEnemy { get; set; }
	[Export]
	public PackedScene twinziesScene { get; set; }
	[Export]
	public Label EnrageTimerLabel { get; set; }
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
		clumpusScript.EnrageTimerLabel = EnrageTimerLabel;

		return clumpusScript;
	}

	/// <summary>
	/// Creates a laser beam enemy with destroyables at the given endpoints,
	/// dealing damage to the player at point between.
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <returns></returns>
	public Node MakeLaserEmitter(Vector2 start, Vector2 end) {
		LaserPair laserPairScript = laserEmitterEnemy.Instantiate() as LaserPair;
		laserPairScript.InitialPosA = start;
		laserPairScript.InitialPosB = end;
		return laserPairScript;
	}

	public Node MakePeekabooEnemy(float height, Vector2 fireDirection) {
		PeekabooEnemy enemy = peekabooEnemy.Instantiate<PeekabooEnemy>();

		enemy.Position = new Vector2(500, height);
		enemy.FireDirection = fireDirection;

		return enemy;
	}

	public Node MakeVertiboo(Vector2 initialPos, bool flip, Vector2 fireDirection) {
		VertibooEnemyScript enemy = vertibooEnemy.Instantiate<VertibooEnemyScript>();

		enemy.Position = initialPos;
		enemy.flip  = flip;
		enemy.FireDirection = fireDirection;

		return enemy;
	}

	public Node MakeGigaspinny(float height) {
		GigaspinnyScript enemy = gigaspinnyEnemy.Instantiate<GigaspinnyScript>();

		enemy.Position = new Vector2(500, height);

		return enemy;
	}

	public Node MakeTwinzies() {
		TwinziesScript twinziesScript = twinziesScene.Instantiate() as TwinziesScript;

		twinziesScript.PlayerNode = GetNode("../Player") as Node2D;
		twinziesScript.EnemyMaster = this;
		twinziesScript.EnrageTimerLabel = EnrageTimerLabel;

		return twinziesScript;
	}
}
