using GameJamBulletHell.Scripts.Attacks;
using Godot;
using System;

public partial class TwinziesScript : BaseBossScript
{
	[Export]
	PackedScene BeamMaster { get; set; }
	[Export]
	PackedScene TwinzBit { get; set; }
	[Export]
	Label TopHealthLabel { get; set; }
	[Export]
	Label BottomHealthLabel { get; set; }
	[Export]
	ColorRect TopHealthBar { get; set; }
	[Export]
	ColorRect BottomHealthBar { get; set; }
	[Export]
	AudioStreamPlayer LaserSound { get; set; }

	private int HealthTop = 300, HealthBottom = 300;

	float timer = 0;
	float enrageAddTimer = 0;
	int enrageCounter = 0;
	/// <summary>
	/// 0 - rush on screen until about 320 with laser on
	/// 1 - move right while spawning vertiboos to the far left
	/// 2 - rush left with laser on to about where vertiboos spawned
	/// 3 - crawl slowly left a bit more. menacingly.
	/// 4 - recenter for next bit, spawn a big spinny at the start
	/// 5 - top moves right, bottom moves left, while channeling beam
	/// 6 - reverse direction
	/// 7 - recenter
	/// 8 - spawn beam-reflector "bits" from boss that move up/down into room a bit, then connect beams between each-other *and* the boss they came from, then move them to corners of the room 64 away from east/west wall
	/// 9 - bosses move to the wall to be opposite the beat they're chained to
	/// 10 - bits return to their source boss, removing beams about halfway back
	/// 11 - reposition bosses to be on the far right while spawning vertiboos, then seq2 again
	/// 
	/// 69 - Early enrage due to one half being dead and the other not; enrage timer gets set to 0 and spam peekaboos/vertiboos
	/// 169 - death animation
	/// </summary>
	int sequence = 0;
	int specialCounter = 0;

	private Node2D pairedMovement;
	private Node2D topHalf;
	private Node2D bottomHalf;

	private Node2D attacksHolder;
	private BeamScript primaryBeam;

	//positional offset from boss center to spawn point for bits is roughly 39,18
	private Node2D[] bits;
	private BeamScript[] bitBeams;

	private Color PrimaryBeamColor = new Color(0.3f, 1f, 0.3f, 0.7f);
	private const int bitzXOffset = 39;
	private const int bitzYOffset = 18;

	private Control HealthBarGroups;
	public override void _Ready() {
		HealthBarGroups = GetNode<Control>("HealthBars");
		HealthBarGroups.Visible = false;

		pairedMovement = GetNode<Node2D>("PairedMovement");
		pairedMovement.Position = new Vector2(-64, 32);
		topHalf = pairedMovement.GetNode<Node2D>("Top");
		bottomHalf = pairedMovement.GetNode<Node2D>("Bottom");
		attacksHolder = GetNode<Node2D>("PairedMovement/AttacksHolder");

		AddPrimaryBeam();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EnrageTimer -= (float)delta;
		timer += (float)delta;
		if (EnrageTimer <= 0) {
			enrageAddTimer += (float)delta;
			EnrageTimerLabel.Text = "0.00s";
		} else {
			EnrageTimerLabel.Text = EnrageTimer.ToString("0.00") + "s";
		}
		float bitSpeed = 64f;


		///Handle spawning adds for enrage
		if (enrageAddTimer >= 1.0f) {
			enrageAddTimer -= 3.0f;

			AddChild(EnemyMaster.MakeLaserEmitter(new Vector2(500, -128), new Vector2(500, 270 + 128)));
		}

		if (HealthTop <= 0 && HealthBottom <= 0 && sequence < 100) {
			//deadge
			PlayerNode.iframes = 1000;
			sequence = 169;
			specialCounter = 0;
			BossDeathSound.Play();
		} else if ((HealthTop <= 0 || HealthBottom <= 0) && sequence < 60) {
			//early enrage, clean stuff up and shift to early enrage sequence
			sequence = 69;
			if (bitBeams != null) {
				foreach (var bitBeam in bitBeams) {
					bitBeam.QueueFree();
				}
			}

			if (bits != null) {
				foreach (var bit in bits) {
					bit.QueueFree();
				}
			}

			ClearAttacksHolder();
			specialCounter = 0;
			timer = (float) delta;
		}

		///Handle standard boss behavior
		switch (sequence) {
			case 0:
				pairedMovement.Position += new Vector2(240f * (float)delta, 0);
				if (pairedMovement.Position.X > 320) {
					pairedMovement.Position = new Vector2(320, pairedMovement.Position.Y);
					sequence = 1;
					specialCounter = 0;
					timer = 0;
					HealthBarGroups.Visible = true;
					ClearAttacksHolder();
				}
				break;
			case 1:
				if (specialCounter == 0) {
					float baseNumber = -100;
					float gap = 20;
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 220, -16),           false, Vector2.Down));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 190, -16 - 1 * gap), false, Vector2.Down));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 160, -16 - 2 * gap), false, Vector2.Down));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 130, -16 - 3 * gap), false, Vector2.Down));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 205, 294 + 0 * gap), true,  Vector2.Up));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 175, 294 + 1 * gap), true,  Vector2.Up));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 145, 294 + 2 * gap), true,  Vector2.Up));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 115, 294 + 3 * gap), true,  Vector2.Up));
					++specialCounter;
				}
				pairedMovement.Position += new Vector2(30f * (float)delta, 0);
				if (pairedMovement.Position.X >= 480 - 64) {
					pairedMovement.Position = new Vector2(480 - 64, pairedMovement.Position.Y);
					sequence = 2;
					specialCounter = 0;
					timer = 0;

					AddPrimaryBeam();
				}
				break;
			case 2:
				pairedMovement.Position -= new Vector2(240f * (float)delta, 0); { 
					int targetPosition = 136;
					if (pairedMovement.Position.X <= targetPosition) {
						pairedMovement.Position = new Vector2(targetPosition, pairedMovement.Position.Y);
						sequence = 3;
						specialCounter = 0;
						timer = 0;
					}
				}
				break;
			case 3:
				pairedMovement.Position -= new Vector2(16f * (float)delta, 0); {
					int targetPosition = 64;
					if (pairedMovement.Position.X <= targetPosition) {
						pairedMovement.Position = new Vector2(targetPosition, pairedMovement.Position.Y);
						sequence = 4;
						specialCounter = 0;
						timer = 0;
						ClearAttacksHolder();
					}
				}
				break;
			case 4:
				if (specialCounter == 0) {
					AddChild(EnemyMaster.MakeGigaspinny(135));
					++specialCounter;
				}
				pairedMovement.Position += new Vector2(64f * (float)delta, 0); {
					int targetPosition = 240;
					if (pairedMovement.Position.X >= targetPosition) {
						pairedMovement.Position = new Vector2(	targetPosition, pairedMovement.Position.Y);
						sequence = 5;
						specialCounter = 0;
						timer = 0;
					}
				}
				break;
			case 5:
				if (specialCounter == 0) {
					AddPrimaryBeam();
					++specialCounter;
				}

				//use timer for adds here

				topHalf.Position += new Vector2(64f * (float)delta, 0);
				bottomHalf.Position += new Vector2(-64f * (float)delta, 0);
				if (topHalf.Position.X >= 240-64) {
					topHalf.Position = new Vector2(240 - 64, topHalf.Position.Y);
					bottomHalf.Position = new Vector2(-240 + 64, bottomHalf.Position.Y);
					sequence = 6;
					specialCounter = 0;
					timer = 0;
				}
				break;
			case 6:

				//user timer for adds here
				topHalf.Position -= new Vector2(64f * (float)delta, 0);
				bottomHalf.Position -= new Vector2(-64f * (float)delta, 0);
				if (topHalf.Position.X <= -240 + 64) {
					topHalf.Position = new Vector2(-240 + 64, topHalf.Position.Y);
					bottomHalf.Position = new Vector2(240 - 64, bottomHalf.Position.Y);
					sequence = 7;
					specialCounter = 0;
					timer = 0;
				}
				break;
			case 7:
				topHalf.Position += new Vector2(64f * (float)delta, 0);
				bottomHalf.Position += new Vector2(-64f * (float)delta, 0);
				if (topHalf.Position.X >= 0) {
					topHalf.Position = new Vector2(0, topHalf.Position.Y);
					bottomHalf.Position = new Vector2(0, bottomHalf.Position.Y);
					sequence = 8;
					specialCounter = 0;
					timer = 0;
					ClearAttacksHolder();
				}
				break;
			case 8:
				///This sequence uses bits[0,1] and bitBeams[0,1,2]
				if (specialCounter==0) {
					++specialCounter;
					bits = new Node2D[] { TwinzBit.Instantiate<Node2D>(), TwinzBit.Instantiate<Node2D>() };

					bits[0].Position = new Vector2(topHalf.GlobalPosition.X + bitzXOffset, topHalf.GlobalPosition.Y + bitzYOffset);
					bits[1].Position = new Vector2(bottomHalf.GlobalPosition.X - bitzXOffset, bottomHalf.GlobalPosition.Y - bitzYOffset);
					AddChild(bits[0]);
					AddChild(bits[1]);

					bitBeams = new BeamScript[] {BeamMaster.Instantiate<BeamScript>(), BeamMaster.Instantiate<BeamScript>(), BeamMaster.Instantiate<BeamScript>()};

					AddChild(EnemyMaster.MakePeekabooEnemy(60, Vector2.Left));
					AddChild(EnemyMaster.MakePeekabooEnemy(210, Vector2.Left));
				}

				if (specialCounter < 3) {
					bits[0].Position += new Vector2(0, bitSpeed * (float)delta);
					bits[1].Position -= new Vector2(0, bitSpeed * (float)delta);
				} else if (specialCounter == 3) {
					Vector2 toLowerRight = (new Vector2(480, 270) - bits[0].Position).Normalized();
					Vector2 toUpperLeft = (-bits[1].Position).Normalized();
					GD.Print(toLowerRight.X.ToString() + "," + toLowerRight.Y.ToString());

					bits[0].Position += bitSpeed * toLowerRight * (float)delta;
					bits[1].Position += bitSpeed * toUpperLeft * (float)delta;
				}

				if (specialCounter == 1 && bits[0].Position.Y >= 64) {
					specialCounter = 2;

					AddChild(bitBeams[0]);
					LaserSound.Play();
					LaserSound.VolumeDb -= 5;
					AddChild(EnemyMaster.MakePeekabooEnemy(90, Vector2.Left));
					AddChild(EnemyMaster.MakePeekabooEnemy(180, Vector2.Left));
				}
				else if (specialCounter == 2 && bits[0].Position.Y >= 120) {
					specialCounter = 3;
					timer = 0;

					bits[0].Position = new Vector2(bits[0].Position.X, 120);
					bits[1].Position = new Vector2(bits[1].Position.X, 270-120);

					bitBeams[1].Color = PrimaryBeamColor;
					bitBeams[1].Width = 7;
					bitBeams[2].Color = PrimaryBeamColor;
					bitBeams[2].Width = 7;

					AddChild(bitBeams[1]);
					AddChild(bitBeams[2]);
					LaserSound.VolumeDb += 5;
					AddChild(EnemyMaster.MakePeekabooEnemy(120, Vector2.Left));
					AddChild(EnemyMaster.MakePeekabooEnemy(150, Vector2.Left));
				}
				else if (specialCounter == 3 && bits[0].Position.X >= 480-64+ bitzXOffset) {
					specialCounter = 0;
					sequence = 9;
					timer = 0;
					bits[0].Position = new Vector2(480 - 64+ bitzXOffset, bits[0].Position.Y);
					bits[1].Position = new Vector2(64-bitzXOffset, bits[1].Position.Y);
				}
				
				break;
			case 9:
				//continues using bits 0,1 and beams 0,1,2 created by sequence 8

				topHalf.Position += new Vector2(64f * (float)delta, 0);
				bottomHalf.Position += new Vector2(-64f * (float)delta, 0);
				if (topHalf.Position.X >= 240-64) {
					topHalf.Position = new Vector2(240-64, topHalf.Position.Y);
					bottomHalf.Position = new Vector2(-240 + 64, bottomHalf.Position.Y);
					sequence = 10;
					specialCounter = 0;
					timer = 0;
					ClearAttacksHolder();
				}
				break;
			case 10:
				//continues using bits 0,1 and beams 0,1,2 created by sequence 8
				bits[0].Position += new Vector2(0, -bitSpeed * (float)delta);
				bits[1].Position += new Vector2(0,  bitSpeed * (float)delta);

				if (specialCounter == 0 && bits[0].Position.Y <= 128 - bitzYOffset) {
					specialCounter = 1;
					timer = 0;
					bitBeams[0].QueueFree();
					bitBeams[1].QueueFree();
					bitBeams[2].QueueFree();
					LaserSound.Stop();
					bitBeams = null;
				}

				if (specialCounter == 1 && bits[0].Position.Y <= 64-bitzYOffset) {
					bits[0].QueueFree();
					bits[1].QueueFree();
					bits = null;
					sequence = 11;
					specialCounter = 0;
					timer = 0;
				}
				break;
			case 11:
				if (specialCounter == 0) {
					float baseNumber = -100;
					float gap = 20;
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 220, -16), false, Vector2.Down));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 190, -16 - 1 * gap), false, Vector2.Down));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 160, -16 - 2 * gap), false, Vector2.Down));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 130, -16 - 3 * gap), false, Vector2.Down));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 205, 294 + 0 * gap), true, Vector2.Up));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 175, 294 + 1 * gap), true, Vector2.Up));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 145, 294 + 2 * gap), true, Vector2.Up));
					AddChild(EnemyMaster.MakeVertiboo(new Vector2(baseNumber + 115, 294 + 3 * gap), true, Vector2.Up));
					++specialCounter;
				}
				bottomHalf.Position += new Vector2(256f * (float)delta, 0);
				if (bottomHalf.Position.X >= 240 - 64) {
					pairedMovement.Position = new Vector2(480 - 64, pairedMovement.Position.Y);
					topHalf.Position = new Vector2(0, topHalf.Position.Y);
					bottomHalf.Position = new Vector2(0, bottomHalf.Position.Y);
					sequence = 2;
					specialCounter = 0;
					timer = 0;
					AddPrimaryBeam();
				}
				break;
			case 69: 
				{
					if (timer > 1.0f) {
						timer -= 10.0f;
						for (int i = 0; i < 5; i += 1) {
							if (HealthTop <= 0) {
								AddChild(EnemyMaster.MakeVertiboo(new Vector2(30 + 40 * i, 294 + 20 * i), true, Vector2.Up));
							} else if (HealthBottom <=0) {
								AddChild(EnemyMaster.MakeVertiboo(new Vector2(20 + 40 * i, -16 - 20 * i), false, Vector2.Down));
							}
							AddChild(EnemyMaster.MakeLaserEmitter(new Vector2(500 + 60 * i, 0 + 90*(i%2)), new Vector2(500 + 60 * i, 180 + 90 * (i % 2))));
						}
						++specialCounter;
					}
				}
				break;
			case 169:
				//allow time for death animation, but make the player invuln so they don't lose
				if (timer >= 4.0f) {
					Dead = true;
				}
				break;
			default:
				break;
		}
		QueueRedraw();
	}

	public override void _PhysicsProcess(double delta) {
		switch (sequence) {
			case 5:
			case 6:
			case 7:
				if (primaryBeam != null) {
					primaryBeam.PositionA = topHalf.Position;
					primaryBeam.PositionB = bottomHalf.Position;
				}
				break;
			case 8:
			case 9:
			case 10:
				if (bitBeams != null) {
					bitBeams[0].PositionA = bits[0].Position;
					bitBeams[0].PositionB = bits[1].Position;
					bitBeams[1].PositionA = topHalf.GlobalPosition;
					bitBeams[1].PositionB = bits[0].Position;
					bitBeams[2].PositionA = bits[1].Position;
					bitBeams[2].PositionB = bottomHalf.GlobalPosition;
				}
				break;
			default:
				break;
		}
	}

	private void AddPrimaryBeam() {
		primaryBeam = BeamMaster.Instantiate<BeamScript>();
		primaryBeam.PositionA = GetNode<Node2D>("PairedMovement/Top").Position;
		primaryBeam.PositionB = GetNode<Node2D>("PairedMovement/Bottom").Position;
		primaryBeam.Width = 10;
		primaryBeam.Color = PrimaryBeamColor;
		attacksHolder.AddChild(primaryBeam);
		LaserSound.Play();
	}

	private void ClearAttacksHolder() {
		primaryBeam = null;
		LaserSound.Stop();
		foreach (Node2D n in attacksHolder.GetChildren()) {
			n.QueueFree();
		}
	}

	private void OnBottomHit(Area2D area) {
		if (HealthBottom <= 0) return;

		HealthBottom -= area.GetParent<AttackBaseScript>().Damage;
		if (HealthBottom <= 0) {
			HealthBottom = 0;
			EnemyExplosionScript explosionScript = DeathExplosion.Instantiate<EnemyExplosionScript>();
			explosionScript.SpawnArea = new Rect2(bottomHalf.GlobalPosition.X - 64, bottomHalf.GlobalPosition.Y, 128, 32);
			AddChild(explosionScript);
			BossDeathSound.Play();
		}
		BottomHealthLabel.Text = HealthBottom.ToString();
		BottomHealthBar.Scale = new Vector2((float)HealthBottom / 300f, 1);

	}
	private void OnTopHit(Area2D area) {
		if (HealthTop <= 0) return;

		HealthTop -= area.GetParent<AttackBaseScript>().Damage;
		if (HealthTop <= 0) {
			HealthTop = 0;
			EnemyExplosionScript explosionScript = DeathExplosion.Instantiate<EnemyExplosionScript>();
			explosionScript.SpawnArea = new Rect2(topHalf.GlobalPosition.X - 64, topHalf.GlobalPosition.Y-32, 128, 32);
			AddChild(explosionScript);
			BossDeathSound.Play();
		}
		TopHealthLabel.Text = HealthTop.ToString();
		TopHealthBar.Scale = new Vector2((float)HealthTop / 300f, 1);
	}
}
