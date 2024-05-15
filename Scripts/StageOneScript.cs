using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class StageOneScript : BaseStageScript
{
	[Export]
	public EnemyMasterScript EnemyMaster { get; set; }
	[Export]
	public PlayerControl Player { get; set; }
	[Export]
	public PackedScene ScoreScreen { get; set; }
	[Export]
	public Control EnrageTimerPanel { get; set; }

	public int LevelRequested = 1;

	// Called when the node enters the scene tree for the first time.

	private double timer = 0;
	private float offset = 0;

	private PriorityQueue<Node, double> LevelScriptPreBoss = new PriorityQueue<Node, double>();
	BaseBossScript bossObject;
	public override void _Ready()
	{
		switch (LevelRequested) {
			case 1:
				LevelOne();
				break;
			case 2:
				LevelTwo();
				break;
			default:
				LevelThree();
				break;
		}
	}

	private void LevelOne() {
		AddFlybyBullet(false, 0);
		AddFlybyBullet(false, 6);
		AddFlybyBullet(false, 10);
		AddFlybyBomb(false, 14);
		AddFlybyBomb(false, 18);
		AddFlybyBullet(true, 19.5f);
		AddFlybyBomb(false, 22);
		AddFlybyBullet(true, 24);
		AddFlybyBomb(false, 26);
		AddFlybyBomb(false, 30);
		AddFlybyBomb(true, 32);
		AddFlybyBomb(false, 34);
		AddFlybyBomb(true, 36);
		AddFlybyBullet(false, 40);
		AddFlybyBullet(true, 40);
		AddFlybyBullet(false, 44);
		AddFlybyBullet(true, 44);
		AddClumpus(60);


		for (int i = 8; i < 46; i += 5) {
			//AddSpikeball(35 + (19 * i) % 200, i);
			AddSpikeball(135, i);
		}
	}

	private void LevelTwo() {
		AddLaserEmitters(new Vector2(500, 20), new Vector2(500, 60), 0);
		AddLaserEmitters(new Vector2(490, 50), new Vector2(600, 50), 0);
		AddLaserEmitters(new Vector2(590, 40), new Vector2(590, 100), 0);
	}

	private void LevelThree() {

	}

	public void EndStage() {
		GD.Print("Hello?!");
		ScoreScript scoreScript = ScoreScreen.Instantiate() as ScoreScript;
		AddSibling(scoreScript);
		scoreScript.TabulateScore(Player.Bullets, Player.Bombs, Player.Health);
		scoreScript.stageScript = this;

		this.SetProcess(false);
		Godot.Collections.Array args = new Godot.Collections.Array{ false };
		this.PropagateCall("set_process", args);
		Player.SetProcess(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += delta;
		if (Player.Health <=0 ) {
			EndStage();
			return;
		}

		Node peekNode;
		double peekPriority;

		LevelScriptPreBoss.TryPeek(out peekNode, out peekPriority);
		while (peekNode != null && timer >= peekPriority) {
			Node poppedNode = LevelScriptPreBoss.Dequeue();
			AddChild(poppedNode);
			LevelScriptPreBoss.TryPeek(out peekNode, out peekPriority);
			if (LevelScriptPreBoss.Count == 0) {
				EnrageTimerPanel.Visible = true;
				bossObject = poppedNode as BaseBossScript;
			}
		}

		if (bossObject != null && bossObject.Dead) {
			EndStage();
		}
	}

	private void AddSpikeball(float height, float time) {
		LevelScriptPreBoss.Enqueue(EnemyMaster.MakeSpikeballEnemy(height), time);
	}

	private void AddFlybyBullet(bool flip, float time) {
		LevelScriptPreBoss.Enqueue(EnemyMaster.MakeFlybyBulletEnemy(flip), time);
	}

	private void AddFlybyBomb(bool flip, float time) {
		LevelScriptPreBoss.Enqueue(EnemyMaster.MakeFlybyBombEnemy(flip), time);
	}

	private void AddClumpus (float time) {
		LevelScriptPreBoss.Enqueue(EnemyMaster.MakeClumpus(), time);
	}

	private void AddLaserEmitters(Vector2 start, Vector2 end, float time) {
		LevelScriptPreBoss.Enqueue(EnemyMaster.MakeLaserEmitter(start, end), time);
	}
}
