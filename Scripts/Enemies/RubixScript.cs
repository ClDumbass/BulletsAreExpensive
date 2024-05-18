using GameJamBulletHell.Scripts.Attacks;
using Godot;
using System;

public partial class RubixScript : BaseBossScript
{
	[Export]
	PackedScene BeamScriptMaster { get; set; }
	[Export]
	PackedScene BombScriptMaster { get; set; }
	[Export]
	PackedScene BulletScriptMaster { get; set; }
	[Export]
	PackedScene MineScriptMaster { get; set; }

	private Node2D RubixNode;

	private Control HealthBarGroups;
	private int BodyHealth=600, LaserModuleHealth=250, BulletModuleHealth=250, BombModuleHealth = 250, MineModuleHealth = 250;
	private const int BodyHealthMax = 600, LaserModuleHealthMax = 250, BulletModuleHealthMax = 250, BombModuleHealthMax = 250, MineModuleHealthMax = 250;
	private Label LaserModuleHealthLabel, BulletModuleHealthLabel, BombModuleHealthLabel, MineModuleHealthLabel, BodyHealthLabel;
	private ColorRect LaserModuleHealthRect, BulletModuleHealthRect, BombModuleHealthRect, MineModuleHealthRect, BodyHealthRect;
	private Sprite2D LaserSegment, MineSegment, BulletSegment, BombSegment;
	private float timer = 0;
	private float timer2 = 0;
	/// <summary>
	/// Rotation Notes:
	///		-Pi/4 - Initial rotation with laser forward.
	///		0 - Laser+Bullet
	///		Pi/2 - Bullet+Mine
	///		Pi - Mine+Bomb
	///		3Pi/2 - Bomb+Laser
	/// 
	/// 
	/// 0 - Approaches the screen, starts firing laser just as they get on screen.
	/// 1 - Rotate to bullet+laser side. Note that this will be looped back to later. 
	/// 2 - Do bullet+laser combo
	/// 3 - Rotate to bullet+mine side. 
	/// 4 - Do bullet+mine combo
	/// 5 - Rotate to mine+bomb side. If 
	/// 6 - Do mine+bomb combo
	/// 7 - Rotate to bomb+laser side
	/// 8 - Do bomb+laser combo
	/// (2,4,6,8) - If both modules on the side are dead, call an add-spawning function instead and rotate after waiting like 10s.
	/// 
	/// 169 - Death animation
	/// </summary>
	private int sequence = 0;
	private int subSequence = 0;


	private BeamScript activeBeam;
	private const float beamWidth = 10f;
	private const float standardBeamLength = 300f;
	private Color beamColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);
	private const float rotationSpeed = MathF.PI / 2;
	Vector2 bulletDirection = Vector2.Left;

	public override void _Ready() {
		RubixNode = GetNode<Node2D>("Cube");
		RubixNode.Position = new Vector2(570, 135);
		LaserSegment = RubixNode.GetNode<Sprite2D>("LaserSegment");
		BulletSegment = RubixNode.GetNode<Sprite2D>("BulletSegment");
		MineSegment = RubixNode.GetNode<Sprite2D>("MineSegment");
		BombSegment = RubixNode.GetNode<Sprite2D>("BombSegment");

		HealthBarGroups = GetNode<Control>("HealthBars");
		HealthBarGroups.Visible = false;

		Control LaserHPControl = HealthBarGroups.GetNode<Control>("LaserHP");
		LaserModuleHealthLabel = LaserHPControl.GetNode<Label>("Label");
		LaserModuleHealthRect = LaserHPControl.GetNode<ColorRect>("Foreground");
		Control BulletModuleControl = HealthBarGroups.GetNode<Control>("BulletHP");
		BulletModuleHealthLabel = BulletModuleControl.GetNode<Label>("Label");
		BulletModuleHealthRect = BulletModuleControl.GetNode<ColorRect>("Foreground");
		Control BombModuleControl = HealthBarGroups.GetNode<Control>("BombHP");
		BombModuleHealthLabel = BombModuleControl.GetNode<Label>("Label");
		BombModuleHealthRect = BombModuleControl.GetNode<ColorRect>("Foreground");
		Control MineModuleControl = HealthBarGroups.GetNode<Control>("MineHP");
		MineModuleHealthLabel = MineModuleControl.GetNode<Label>("Label");
		MineModuleHealthRect = MineModuleControl.GetNode<ColorRect>("Foreground");

		Control BodyControl = HealthBarGroups.GetNode<Control>("MainBody");
		BodyHealthLabel = BodyControl.GetNode<Label>("Label");
		BodyHealthRect = BodyControl.GetNode<ColorRect>("Foreground");

		UpdateHealthBarDisplay();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += (float)delta;
		timer2 += (float)delta;
		EnrageTimer -= (float)delta;
		if (EnrageTimer < 0) {
			EnrageTimer = 0;
		}
		EnrageTimerLabel.Text = EnrageTimer.ToString("0.00") + "s"; 

		if (BodyHealth <= 0 && sequence < 169) {
			sequence = 169;
			timer = 0;
			timer2 = 0;
			subSequence = 0;
			PlayerNode.iframes = 1000;
			KillBeam();
		}

		switch (sequence) {
			case 0:
				RubixNode.Position -= new Vector2(30f * (float)delta, 0);
				if (subSequence == 0 && timer >= 1f) {
					subSequence = 1;
					MakeBeam(new Vector2(-10, 135));
					activeBeam.Width = beamWidth * 1.5f; //normally would have to null check activeBeam on this, but this can only happen right as boss spawns so we know laser segment will be alive.
				}
				if (subSequence == 1 && RubixNode.Position.X <= 480) {
					RubixNode.Position = new Vector2(480, 135);
					sequence = 1;
					subSequence = 0;
					timer = 0;
					timer2 = 0;
					HealthBarGroups.Visible = true;
					KillBeam();
				}
				break;
			case 1:
				RubixNode.Rotation += rotationSpeed * (float)delta;
				if (RubixNode.Rotation >= MathF.Tau) {
					RubixNode.Rotation = 0;
					sequence = 2;
					subSequence = 0;
					timer = 0;
					timer2 = 0;
					//prep beam and bullet direction for sequence 2
					MakeBeam(LaserSegment.GlobalPosition + standardBeamLength * Vector2.Up.Rotated(-MathF.PI / 4));
					bulletDirection = Vector2.Left.Rotated(-MathF.PI / 4);
				}
				break;
			case 2: { //laser+bullet
					bulletDirection = bulletDirection.Rotated((float)delta * MathF.PI / 20);
					if (timer2 >= 0.1f) {
						timer2 -= 0.1f;
						float bulletSpeedThisFrame = 60 + 10f * timer;

						FireBullet(bulletDirection, bulletSpeedThisFrame);
						if (timer > 3.5f) {
							FireBullet(bulletDirection.Rotated(MathF.PI / 12), bulletSpeedThisFrame);
						}
						if (timer > 7f) {
							FireBullet(bulletDirection.Rotated(-MathF.PI / 12), bulletSpeedThisFrame);
						}
					}

					if (timer >= 10f) {
						sequence = 3;
						timer = 0;
						timer2 = 0;
						KillBeam();
					}
				}
				break;
			case 3:
				RubixNode.Rotation += rotationSpeed * (float)delta;
				if (RubixNode.Rotation >= MathF.PI/2) {
					RubixNode.Rotation = MathF.PI/2;
					sequence = 4;
					subSequence = 0;
					timer = 0;
					timer2 = 0;

					//prep for sequence 4
					AddMine(new Vector2(500, 200), false);
					AddMine(new Vector2(530, 200), true);
					bulletDirection = Vector2.Left.Rotated(MathF.PI/8);
					//these mines are actually for sequence 6
					AddMine(new Vector2(550, 30), false);
					AddMine(new Vector2(700, 30), false);
					AddMine(new Vector2(700, 230), true);
				}
				break;
			case 4: //mine+bullet
				bulletDirection = bulletDirection.Rotated((float)delta * MathF.PI / -16);
				if (timer2 >= 0.1f && timer <= 8f) {
					timer2 -= 0.1f;
					float bulletSpeed = 90;

					FireBullet(bulletDirection, bulletSpeed);
					FireBullet(bulletDirection.Rotated(MathF.PI / 12), bulletSpeed);
					FireBullet(bulletDirection.Rotated(-MathF.PI / 12), bulletSpeed);
				}
				if (timer >= 10f) {
					sequence = 5;
					timer = 0;
					timer2 = 0;
				}
				break;
			case 5:
				RubixNode.Rotation += rotationSpeed * (float)delta;
				if (RubixNode.Rotation >= MathF.PI) {
					RubixNode.Rotation = MathF.PI;
					sequence = 6;
					subSequence = 0;
					timer = 0;
					timer2 = 0;
				}
				break;
			case 6://TODO: Mine+Bomb
				if (subSequence ==0) {
					FireBomb(new Vector2(200, 50), 55);
					subSequence = 1;
				}
				if (subSequence != 0) {
					if (timer2 >= 0.3f) {
						timer2 -= 0.3f;
						FireBomb(new Vector2(330 - 30 * (subSequence % 8), 200 - 30 * (subSequence / 8)), 60);
						subSequence++;
					}
				}
				if (timer >= 10f) {
					sequence = 7;
					timer = 0;
					timer2 = 0;
				}
				break;
			case 7:
				RubixNode.Rotation += rotationSpeed * (float)delta;
				if (RubixNode.Rotation >= 3 * MathF.PI / 2) {
					RubixNode.Rotation = 3 * MathF.PI / 2;
					sequence = 8;
					subSequence = 0;
					timer = 0;
					timer2 = 0;
					MakeBeam(LaserSegment.GlobalPosition + standardBeamLength * Vector2.Down.Rotated(MathF.PI / 4));
					if (activeBeam != null) {
						activeBeam.GetNode<Area2D>("Beam").CollisionLayer = 259; //hacky, but I want the bombs to splode as they hit the beam
					}
				}
				break;
			case 8: //Laser+Bomb Pattern
				if (timer2 >= 0.3f) {
					timer2 -= 0.3f;
					FireBomb(new Vector2(330 - 30 * (subSequence % 8), 50 + 30 * (subSequence / 8)), 60);
					subSequence++;
				}
				if (timer >= 10f) {
					sequence = 1;
					timer = 0;
					timer2 = 0;
					KillBeam();
				}
				break;
			case 169:
				//allow time for death animation, but make the player invuln so they don't lose
				if (timer >= 2.0f) {
					Dead = true;
				}
				break;
			default:
				break;
		}
	}

	public override void _PhysicsProcess(double delta) {
		switch (sequence) {
			case 0:
				if (activeBeam != null) {
					activeBeam.PositionA = LaserSegment.GlobalPosition;
				}
				break;
			case 2:
				if (activeBeam != null) {
					if (timer < 6f) {
						activeBeam.PositionB = LaserSegment.GlobalPosition + standardBeamLength * Vector2.Up.Rotated(-MathF.PI / 4 - timer * MathF.PI / 12);
					}
					else if (timer < 9f) {
						activeBeam.PositionB = LaserSegment.GlobalPosition + standardBeamLength * Vector2.Up.Rotated(-MathF.PI / 4 - MathF.PI / 2);
					}
					else {
						activeBeam.PositionB = LaserSegment.GlobalPosition + standardBeamLength * Vector2.Up.Rotated(-MathF.PI / 4 - MathF.PI / 2 + (timer - 9F) * MathF.PI / 12);
					}
				}
				break;
			case 8:
				if (activeBeam != null) {
					if (timer < 6f) {
						activeBeam.PositionB = LaserSegment.GlobalPosition + standardBeamLength * Vector2.Down.Rotated(MathF.PI / 4 + timer * MathF.PI / 12);
					}
					else if (timer < 9f) {
						activeBeam.PositionB = LaserSegment.GlobalPosition + standardBeamLength * Vector2.Down.Rotated(MathF.PI / 4 + MathF.PI / 2);
					}
					else {
						activeBeam.PositionB = LaserSegment.GlobalPosition + standardBeamLength * Vector2.Down.Rotated(MathF.PI / 4 + MathF.PI / 2 - (timer - 9F) * MathF.PI / 12);
					}
				}
				break;
			default:
				break;
		}
	}

	private void AddSpawnHandler() {

	}

	private void MakeBeam(Vector2 target) {
		if (LaserModuleHealth <= 0) {
			return;
		}
		activeBeam = BeamScriptMaster.Instantiate<BeamScript>();
		activeBeam.PositionA = LaserSegment.GlobalPosition;
		activeBeam.PositionB = target;
		activeBeam.Width = beamWidth;
		activeBeam.Color = beamColor;
		AddChild(activeBeam);
	}

	private void FireBullet(Vector2 direction, float speed) {
		if (BulletModuleHealth <= 0) {
			return;
		}
		BulletScript bullet = BulletScriptMaster.Instantiate<BulletScript>();
		bullet.InitialPosition = BulletSegment.GlobalPosition;
		bullet.Direction = direction;
		bullet.Speed = speed;
		AddChild(bullet);
	}

	private void AddMine(Vector2 position, bool isCircle=true) {
		if (MineModuleHealth <= 0) { return; }

		MineScript newMine = MineScriptMaster.Instantiate<MineScript>();

		newMine.Position = position;
		newMine.IsCircle = isCircle;

		AddChild(newMine);
	}

	private void FireBomb(Vector2 target, float speed) {
		if (BombModuleHealth <= 0) { return; }

		BombScript newBomb = BombScriptMaster.Instantiate<BombScript>();

		newBomb.InitialPosition = BombSegment.GlobalPosition;
		newBomb.TargetLocation = target;
		newBomb.Speed = speed;
		newBomb.Radius = 32;

		AddChild(newBomb);
	}

	private void KillBeam() {
		activeBeam?.QueueFree();
		activeBeam = null;
	}

	private void UpdateHealthBarDisplay() {
		if (BodyHealth < 0) {
			BodyHealth = 0;
		}
		BodyHealthLabel.Text = BodyHealth.ToString();
		BodyHealthRect.Scale = new Vector2((float)BodyHealth / (float)BodyHealthMax, 1);

		if (LaserModuleHealth < 0) {
			LaserModuleHealth = 0;
		}
		LaserModuleHealthLabel.Text = LaserModuleHealth.ToString();
		LaserModuleHealthRect.Scale = new Vector2((float)LaserModuleHealth / (float)LaserModuleHealthMax, 1);

		if (BulletModuleHealth < 0) {
			BulletModuleHealth = 0;
		}
		BulletModuleHealthLabel.Text = BulletModuleHealth.ToString();
		BulletModuleHealthRect.Scale = new Vector2((float)BulletModuleHealth / (float)BulletModuleHealthMax, 1);
		if (MineModuleHealth < 0) {
			MineModuleHealth = 0;
		}
		MineModuleHealthLabel.Text = MineModuleHealth.ToString();
		MineModuleHealthRect.Scale = new Vector2((float)MineModuleHealth / (float)MineModuleHealthMax, 1);
		if (BombModuleHealth < 0) {
			BombModuleHealth = 0;
		}
		BombModuleHealthLabel.Text = BombModuleHealth.ToString();
		BombModuleHealthRect.Scale = new Vector2((float)BombModuleHealth / (float)BombModuleHealthMax, 1);
	}

	private void OnLaserModuleHit(Area2D area) {
		LaserModuleHealth -= area.GetParent<AttackBaseScript>().Damage;
		BodyHealth -= 1;
		UpdateHealthBarDisplay();
		if (LaserModuleHealth <= 0 && LaserSegment.RegionRect.Position.X < 100) {
			Rect2 newRegion = new Rect2(new Vector2(LaserSegment.RegionRect.Position.X + 128, LaserSegment.RegionRect.Position.Y),
										LaserSegment.RegionRect.Size);
			LaserSegment.RegionRect = newRegion;
			KillBeam();
		}
	}

	private void OnBulletModuleHit(Area2D area) {
		BulletModuleHealth -= area.GetParent<AttackBaseScript>().Damage;
		BodyHealth -= 1;
		UpdateHealthBarDisplay();
		if (BulletModuleHealth <= 0 && BulletSegment.RegionRect.Position.X < 100) {
			Rect2 newRegion = new Rect2(new Vector2(BulletSegment.RegionRect.Position.X + 128, BulletSegment.RegionRect.Position.Y),
										BulletSegment.RegionRect.Size);
			BulletSegment.RegionRect = newRegion;
		}
	}

	private void OnMineModuleHit(Area2D area) {
		MineModuleHealth -= area.GetParent<AttackBaseScript>().Damage;
		BodyHealth -= 1;
		UpdateHealthBarDisplay();
		if (MineModuleHealth <= 0 && MineSegment.RegionRect.Position.X < 100) {
			Rect2 newRegion = new Rect2(new Vector2(MineSegment.RegionRect.Position.X + 128, MineSegment.RegionRect.Position.Y),
										MineSegment.RegionRect.Size);
			MineSegment.RegionRect = newRegion;
		}
	}

	private void OnBombModuleHit(Area2D area) {
		BombModuleHealth -= area.GetParent<AttackBaseScript>().Damage;
		BodyHealth -= 1;
		UpdateHealthBarDisplay();
		if (BombModuleHealth <= 0 && BombSegment.RegionRect.Position.X < 100) {
			Rect2 newRegion = new Rect2(new Vector2(BombSegment.RegionRect.Position.X + 128, BombSegment.RegionRect.Position.Y),
										BombSegment.RegionRect.Size);
			BombSegment.RegionRect = newRegion;
		}
	}
}


