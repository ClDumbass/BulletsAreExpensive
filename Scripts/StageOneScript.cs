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
	[Export]
	AudioStreamPlayer WarmupBGMStage1 { get; set; }
	[Export]
	AudioStreamPlayer WarmupBGMStage2 { get; set; }
	[Export]
	AudioStreamPlayer WarmupBGMStage3 { get; set; }
	[Export]
	AudioStreamPlayer BossMusic { get; set; }

	public int LevelRequested = 1;

	// Called when the node enters the scene tree for the first time.

	private float timer = 0;
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
			case 3:
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
		WarmupBGMStage1.Play(0);
		WarmupBGMStage1.VolumeDb = 0;
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
		WarmupBGMStage2.Play(0);
		WarmupBGMStage2.VolumeDb = 0;
	}

	private void LevelThree() {
		/////Formation 1
		//AddMine(new Vector2(420, 135), true, 0);
		//AddPeekaboo(125, Vector2.Left, 0);
		//AddPeekaboo(145, Vector2.Left, 0);
		//AddPeekaboo(40, Vector2.Left.Rotated(-MathF.PI / 18), 4);
		//AddPeekaboo(230, Vector2.Left.Rotated(MathF.PI / 18), 4);
		//AddMine(new Vector2(500, 12), true, 8);
		//AddMine(new Vector2(500, 258), true, 8);

		///// Formation 2
		//float formation2base = 13;
		//AddMine(new Vector2(500, 75), false, formation2base);
		//AddMine(new Vector2(530, 75), false, formation2base);
		//AddMine(new Vector2(560, 75), false, formation2base);
		//AddMine(new Vector2(500, 105), false, formation2base);
		//AddMine(new Vector2(500, 135), false, formation2base);
		//AddMine(new Vector2(560, 135), false, formation2base);
		//AddMine(new Vector2(560, 165), false, formation2base);
		//AddMine(new Vector2(500, 195), false, formation2base);
		//AddMine(new Vector2(530, 195), false, formation2base);
		//AddMine(new Vector2(560, 195), false, formation2base);
		//AddMine(new Vector2(500, 258), true, formation2base);
		//AddPeekaboo(105, Vector2.Left, formation2base + 7);
		//AddPeekaboo(55, Vector2.Left, formation2base + 7);
		//AddPeekaboo(45, Vector2.Left, formation2base + 7);

		///// Formation 3
		//float formation3base = 30;
		//AddMine(new Vector2(500, 135), true, formation3base);
		//for (int i = 0; i < 15; i++) {
		//	AddVertiboo(new Vector2(270 - 15 * i, -16), Vector2.Down, false, formation3base + 0.5f * i);
		//}

		///// Formation 4 is kind-of a bait since it links to the boss
		//float formation4base = 40;
		//for (int i = 0; i < 11; i++) {
		//	if (i==5) { continue; }
		//	AddMine(new Vector2(500, 15+24*i), true, formation4base);
		//}
		//for (int i = 0; i < 11; i++) {
		//	if (i == 5) { continue; }
		//	AddMine(new Vector2(524, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 6; i < 11; i++) {
		//	AddMine(new Vector2(548, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 1; i < 11; i++) {
		//	AddMine(new Vector2(572, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 6; i < 11; i++) {
		//	if (i >= 7 && i <= 9) { continue; }
		//	AddMine(new Vector2(596, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 0; i < 11; i++) {
		//	if (i == 5 || i==7 || i==9) { continue; }
		//	AddMine(new Vector2(620, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 0; i < 11; i++) {
		//	if (i == 5 || i==7 || i==9) { continue; }
		//	AddMine(new Vector2(644, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 0; i < 11; i++) {
		//	if ((i >= 5 && i <= 7) || i==9) { continue; }
		//	AddMine(new Vector2(668, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 0; i < 11; i++) {
		//	if (i == 9) { continue; }
		//	AddMine(new Vector2(692, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 0; i < 11; i++) {
		//	if (i >= 4 && i <= 9) { continue; }
		//	AddMine(new Vector2(716, 15 + 24 * i), true, formation4base);
		//}
		//for (int i = 0; i < 11; i++) {
		//	if (i >= 4 && i <= 5) { continue; }
		//	AddMine(new Vector2(740, 15 + 24 * i), true, formation4base);
		//}
		//AddMine(new Vector2(764, 135), false, formation4base);

		AddRubix(0);

		WarmupBGMStage3.Play();
		WarmupBGMStage3.VolumeDb = 0;
	}

	public void EndStage() {
		GD.Print("Hello?!");
		ScoreScript scoreScript = ScoreScreen.Instantiate() as ScoreScript;
		GetParent<Node>().AddChild(scoreScript);
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
		timer += (float)delta;
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

		if (timer >= 60 && !BossMusic.Playing) {
			BossMusic.Play();
		}
		if (timer >= 60 && timer <= 62) {
			WarmupBGMStage1.VolumeDb = -100f * (62f - timer) / 2f;
			WarmupBGMStage2.VolumeDb = -100f * (62f - timer) / 2f;
			WarmupBGMStage3.VolumeDb = -100f * (62f - timer) / 2f; 
			BossMusic.VolumeDb = 100f * (timer - 62f) / 2f;
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
	private void AddVertiboo(Vector2 position, Vector2 direction, bool flip, float time) {
		LevelScript.Enqueue(EnemyMaster.MakeVertiboo(position, flip, direction), time);
	}
	private void AddGigaspinny(float height, float time) {
		LevelScript.Enqueue(EnemyMaster.MakeGigaspinny(height), time);
	}
	private void AddTwinzies(float time) {
		LevelScript.Enqueue(EnemyMaster.MakeTwinzies(), time);
	}

	private void AddMine(Vector2 position, bool isCircle, float time) {
		LevelScript.Enqueue(EnemyMaster.MakeMine(position, isCircle), time);
	}

	private void AddRubix(float time) {
		LevelScript.Enqueue(EnemyMaster.MakeRubix(), time);
	}
}
