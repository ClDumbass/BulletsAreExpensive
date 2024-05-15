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

	private PriorityQueue<Node, double> LevelScript = new PriorityQueue<Node, double>();
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

		/// Formation 1
		float formation1start = 0;
		AddLaserEmitters(new Vector2(530, 20), new Vector2(500, 180), formation1start + 0);
		AddLaserEmitters(new Vector2(530, 90), new Vector2(500, 250), formation1start + 1);
		AddLaserEmitters(new Vector2(530, 90), new Vector2(560, 250), formation1start + 1);
		AddLaserEmitters(new Vector2(530, 20), new Vector2(560, 180), formation1start + 2);
		AddPeekaboo(33, Vector2.Left, formation1start + 2);


		/// Formation 2
		float formation2start = 8;
		//formation 2, horizontals
		AddLaserEmitters(new Vector2(540, 45), new Vector2(760, 45), formation2start);
		AddLaserEmitters(new Vector2(660, 105), new Vector2(760, 105), formation2start);
		AddLaserEmitters(new Vector2(600, 165), new Vector2(660, 165), formation2start);
		AddLaserEmitters(new Vector2(600, 225), new Vector2(780, 225), formation2start);
		//formation 2, diagonals
		AddLaserEmitters(new Vector2(540, 45), new Vector2(660, 165), formation2start);
		AddLaserEmitters(new Vector2(660, 105), new Vector2(780, 225), formation2start);
		//formation 2, peekaboo
		AddPeekaboo(180, Vector2.Left, formation2start + 2);
		AddPeekaboo(205, Vector2.Left, formation2start + 2);
		AddPeekaboo(60, Vector2.Left, formation2start + 6);
		AddPeekaboo(85, Vector2.Left, formation2start + 6);
		//formation 2, bomber that messes with you
		AddFlybyBomb(false, formation2start + 4);


		/// Formation 3
		float formation3start = 22;
		AddLaserEmitters(new Vector2(500, 50), new Vector2(600, 50), formation3start);
		AddLaserEmitters(new Vector2(500, 220), new Vector2(600, 220), formation3start);
		AddLaserEmitters(new Vector2(500, 50), new Vector2(500, 220), formation3start);
		AddLaserEmitters(new Vector2(600, 50), new Vector2(600, 220), formation3start);
		AddGigaspinny(135, formation3start + 1.67f);
		AddPeekaboo(35, Vector2.Left, formation3start + 1.67f);
		AddPeekaboo(235, Vector2.Left, formation3start + 1.67f);


		///Formation 4
		float formation4Start = 36;
		AddFlybyBomb(true, formation4Start + 0);
		AddPeekaboo(135, Vector2.Left.Rotated(MathF.PI / 32.0f), formation4Start + 2);
		AddPeekaboo(135, Vector2.Left.Rotated(-MathF.PI / 32.0f), formation4Start + 2);
		AddLaserEmitters(new Vector2(530, 20), new Vector2(500, 180), formation4Start + 1);
		AddLaserEmitters(new Vector2(530, 90), new Vector2(500, 250), formation4Start + 2);
		AddFlybyBomb(false, formation4Start + 2);
		AddLaserEmitters(new Vector2(530, 90), new Vector2(560, 250), formation4Start + 3);
		AddLaserEmitters(new Vector2(530, 20), new Vector2(560, 180), formation4Start + 4);

		///Formation 6, just a gift
		float formation6Start = 49;
		AddPeekaboo(16, Vector2.Down.Rotated(MathF.PI / 16.0f), formation6Start + 2);
		AddPeekaboo(254, Vector2.Up.Rotated(-MathF.PI / 16.0f), formation6Start + 2);

		//boss here, have them slap in from the left with a laser between them that you "dodge" because you're eating those bullets up ahead
		AddTwinzies(60);

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

		LevelScript.TryPeek(out peekNode, out peekPriority);
		while (peekNode != null && timer >= peekPriority) {
			Node poppedNode = LevelScript.Dequeue();
			AddChild(poppedNode);
			LevelScript.TryPeek(out peekNode, out peekPriority);
			if (LevelScript.Count == 0) {
				EnrageTimerPanel.Visible = true;
				bossObject = poppedNode as BaseBossScript;
			}
		}

		if (bossObject != null && bossObject.Dead) {
			EndStage();
		}
	}

	private void AddSpikeball(float height, float time) {
		LevelScript.Enqueue(EnemyMaster.MakeSpikeballEnemy(height), time);
	}

	private void AddFlybyBullet(bool flip, float time) {
		LevelScript.Enqueue(EnemyMaster.MakeFlybyBulletEnemy(flip), time);
	}

	private void AddFlybyBomb(bool flip, float time) {
		LevelScript.Enqueue(EnemyMaster.MakeFlybyBombEnemy(flip), time);
	}

	private void AddClumpus (float time) {
		LevelScript.Enqueue(EnemyMaster.MakeClumpus(), time);
	}

	private void AddLaserEmitters(Vector2 start, Vector2 end, float time) {
		LevelScript.Enqueue(EnemyMaster.MakeLaserEmitter(start, end), time);
	}

	private void AddPeekaboo(float height, Vector2 direction, float time) {
		LevelScript.Enqueue(EnemyMaster.MakePeekabooEnemy(height, direction), time);
	}
	private void AddGigaspinny(float height, float time) {
		LevelScript.Enqueue(EnemyMaster.MakeGigaspinny(height), time);
	}
	private void AddTwinzies(float time) {
		LevelScript.Enqueue(EnemyMaster.MakeTwinzies(), time);
	}
}
