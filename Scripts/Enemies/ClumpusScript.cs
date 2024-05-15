using GameJamBulletHell.Scripts.Attacks;
using Godot;
using System;

public partial class ClumpusScript : BaseBossScript
{
	[Export]
	public PackedScene BulletMaster { get; set; }
	[Export]
	public PackedScene BombMaster { get; set; }
	[Export]
	public EnemyMasterScript EnemyMaster { get; set; }
	[Export]
	public Node2D PlayerNode { get; set; }
	[Export]
	public int Health { get; set; } = 300;
	[Export]
	public AudioStreamPlayer BombFireSound { get; set; }
	[Export]
	public AudioStreamPlayer BulletFireSound { get; set; }
	public Label EnrageTimerLabel { get; set; }

	private int RArmHealth=100, LArmHealth=100, ClusterLHealth=100, ClusterRHealth = 100, ClusterMHealth = 100;
	bool clustersDead = false;

	float timer = 0;
	float addsTimer = 0;
	int addsSequence = 0;

	private Vector2 RArmStartPos, RArmEndPos, LArmStartPos, LArmEndPos, ClusterLPos, ClusterRPos, ClusterMPos;
	private Node2D ClumpusBody;

	private Control HealthBarGroups;
	private Label ClusterLHealthLabel, ClusterMHealthLabel, ClusterRHealthLabel, CoreHealthLabel;
	private ColorRect ClusterLHealthBar, ClusterMHealthBar, ClusterRHealthBar, CoreHealthBar;
	/// <summary>
	/// 0 - Coming on screen slowly.
	/// 1 - Phase 1a, bullet lines and random bombs.
	/// 2 - Phase 1b, bomb arrays and bullet cones.
	/// 11 - Boss has only the core left and thus begins summoning reinforcements
	/// 12 - Enrage has begun
	/// 13 - Death sequence?
	/// </summary>
	int sequence = 0;
	/// <summary>
	/// Used by different phases to count how many shots they've fired in the same pattern.
	/// </summary>
	int specialCounter = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		ClumpusBody = GetNode<Node2D>("ClumpusBody");
		ClumpusBody.Position = new Vector2(480 + 64, 270 / 2);


		HealthBarGroups = GetNode<Control>("HealthBars");
		HealthBarGroups.Visible = false;
		ClusterLHealthLabel = GetNode<Label>("HealthBars/ClusterL/Label");
		ClusterLHealthBar = GetNode<ColorRect>("HealthBars/ClusterL/Foreground");
		ClusterMHealthLabel = GetNode<Label>("HealthBars/ClusterM/Label");
		ClusterMHealthBar = GetNode<ColorRect>("HealthBars/ClusterM/Foreground");
		ClusterRHealthLabel = GetNode<Label>("HealthBars/ClusterR/Label");
		ClusterRHealthBar = GetNode<ColorRect>("HealthBars/ClusterR/Foreground");
		CoreHealthLabel = GetNode<Label>("HealthBars/MainBody/Label");
		CoreHealthBar = GetNode<ColorRect>("HealthBars/MainBody/Foreground");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += (float)delta;
		EnrageTimer -= (float)delta;
		EnrageTimerLabel.Text = EnrageTimer.ToString("0.00") + "s";

		//check for add spawning conditions related to breaking the arms
		if ((LArmHealth <=0 || RArmHealth <= 0) && sequence < 12) {
			addsTimer += (float)delta;

			if (addsTimer > 3.0f) { 
				if (LArmHealth <=0 && addsSequence % 2 == 0) {
					if ((addsSequence >> 1)%2 == 0) {
						AddChild(EnemyMaster.MakeFlybyBulletEnemy(true));
					} else {
						AddChild(EnemyMaster.MakeFlybyBombEnemy(true));
					}
				}
				if (RArmHealth <= 0 && addsSequence % 2 == 1) {
					if ((addsSequence >> 1) % 2 == 0) {
						AddChild(EnemyMaster.MakeFlybyBulletEnemy(false));
					}
					else {
						AddChild(EnemyMaster.MakeFlybyBombEnemy(false));
					}
				}
				addsTimer -= 2.0f;
				addsSequence++;
			}
		}

		//Check for add spawn condition for the cores being broken
		if (clustersDead && sequence < 11) {
			sequence = 11;
			specialCounter = 0;
		}

		//Check for enrage
		if (Health > 0 && sequence < 12 && EnrageTimer <= 0f) {
			sequence = 12;
			specialCounter = 0;
		}

		//check for death sequence start (not fully implemented tho)
		if (Health <= 0 && sequence < 13) {
			sequence = 13;
			timer = 0;
			(PlayerNode as PlayerControl).iframes = 1000;
		}

		//Handle basic attack pattern sequences
		switch (sequence) {
			case 0:
				ClumpusBody.Position -= new Vector2((float)(32 * delta), 0);
				if (ClumpusBody.Position.X <= 480-16) {
					sequence = 1;
					timer = 0;
					RArmStartPos = GetNode<Node2D>("ClumpusBody/RArmStart").GlobalPosition;
					RArmEndPos = GetNode<Node2D>("ClumpusBody/RArmEnd").GlobalPosition;
					LArmStartPos = GetNode<Node2D>("ClumpusBody/LArmStart").GlobalPosition;
					LArmEndPos = GetNode<Node2D>("ClumpusBody/LArmEnd").GlobalPosition;
					ClusterLPos = GetNode<Node2D>("ClumpusBody/ClusterL").GlobalPosition;
					ClusterRPos = GetNode<Node2D>("ClumpusBody/ClusterR").GlobalPosition;
					ClusterMPos = GetNode<Node2D>("ClumpusBody/ClusterM").GlobalPosition;

					HealthBarGroups.Visible = true;
				}
				break;
			case 1:
				if (timer > 0.3f) {
					timer -= 0.3f;
					FireBulletRArm(Vector2.Left);
					FireBulletLArm(Vector2.Left);
					if (ClusterLHealth > 0) { 
						FireBullet(ClusterLPos, Vector2.Left.Rotated(-MathF.PI/16.0f));
					}
					if (ClusterMHealth > 0) {
						FireBullet(ClusterMPos, Vector2.Left);
					}
					if (ClusterRHealth > 0) {
						FireBullet(ClusterRPos, Vector2.Left.Rotated(MathF.PI/16f));
					}
					if (LArmHealth > 0 && specialCounter %4==0) {
						FireBomb(LArmStartPos, new Vector2(PlayerNode.Position.X + Random.Shared.Next() % 65 - 32, PlayerNode.Position.Y + Random.Shared.Next() % 65 - 32));
					}
					if (RArmHealth > 0 && specialCounter % 4 == 0) {
						FireBomb(RArmStartPos, new Vector2(PlayerNode.Position.X + Random.Shared.Next() % 65 - 32, PlayerNode.Position.Y + Random.Shared.Next() % 65 - 32));
					}
					++specialCounter;
				}
				if (specialCounter > 20) {
					sequence = 2; //switch to 2 later
					specialCounter = 0;
				}
				break;
			case 2:
				if (timer > 0.15f) {
					timer -= 0.15f;
					

					//bullets occasionally
					if (specialCounter%2 == 0) {
						if (RArmHealth > 0) {
							FireBulletRArm(Vector2.Left.Rotated(-MathF.PI / (8f + (float)specialCounter)));
						}
						if (LArmHealth > 0) {
							FireBulletLArm(Vector2.Left.Rotated(+MathF.PI / (8f + (float)specialCounter)));
						}
						if (ClusterLHealth > 0) {
							FireBullet(ClusterLPos, Vector2.Left);
						}
						if (ClusterRHealth > 0) {
							FireBullet(ClusterRPos, Vector2.Left);
						}
						if (ClusterMHealth > 0) {

						}
					}

					//... but mostly bombs
					if (RArmHealth > 0) {
						FireBomb(RArmStartPos, new Vector2(15 + 30 * (float)(specialCounter / 5), 15 + 26 * (float)(specialCounter % 5)));
					}
					if (LArmHealth > 0) {
						FireBomb(LArmStartPos, new Vector2(15 + 30 * (float)(specialCounter / 5), 255 - 26 * (float)(specialCounter % 5)));
					}
					++specialCounter;
				}
				if (specialCounter > 60) {
					sequence = 1;
					specialCounter = 0;
				}
				break;
			case 11:
				if (timer > 3.0f) {
					++specialCounter;
					timer -= 1.0f;
					AddChild(EnemyMaster.MakeSpikeballEnemy(135));
				}
				break;
			case 12:
				if (timer > 0.5f) {
					timer -= 0.5f;
					AddChild(EnemyMaster.MakeSpikeballEnemy(15));
					AddChild(EnemyMaster.MakeSpikeballEnemy(45));
					AddChild(EnemyMaster.MakeSpikeballEnemy(75));
					AddChild(EnemyMaster.MakeSpikeballEnemy(105));
					AddChild(EnemyMaster.MakeSpikeballEnemy(135));
					AddChild(EnemyMaster.MakeSpikeballEnemy(165));
					AddChild(EnemyMaster.MakeSpikeballEnemy(195));
					AddChild(EnemyMaster.MakeSpikeballEnemy(225));
					AddChild(EnemyMaster.MakeSpikeballEnemy(255));
				}
				break;
			case 13:
				//allow time for death animation, but make the player invuln so they don't lose
				if (timer >= 2.0f) {
					Dead = true;
				}
				break;
			default:
				break;
		}
	}

	private void FireBulletRArm(Vector2 direction) {
		if (RArmHealth > 0) {
			FireBullet(RArmEndPos, direction);
		}
	}

	private void FireBulletLArm(Vector2 direction) {
		if (LArmHealth > 0) {
			FireBullet(LArmEndPos, direction);
		}
	}

	private void FireBullet(Vector2 sourcePos, Vector2 direction) {
		BulletScript bulletClone = BulletMaster.Instantiate() as BulletScript;
		bulletClone.InitialPosition = sourcePos;
		bulletClone.Direction = direction;
		GetParent().AddChild(bulletClone);
		BulletFireSound.Play();
	}

	private void FireBomb(Vector2 sourcePos, Vector2 target) {
		BombScript bombClone = BombMaster.Instantiate() as BombScript;
		bombClone.InitialPosition = sourcePos;
		bombClone.TargetLocation = target;
		bombClone.Radius = 19;
		bombClone.Speed = MathF.Abs(0.2f * (sourcePos.X - target.X)) + 30f;
		GetParent().AddChild(bombClone);
		BombFireSound.Play();
	}

	private void OnCoreHit(Area2D area) {
		if (clustersDead) {
			Health -= area.GetParent<AttackBaseScript>().Damage;
			if (Health <= 0) {
				Health = 0;
			}
			CoreHealthLabel.Text = Health.ToString();
			CoreHealthBar.Scale = new Vector2((float)Health / 500f, 1);
			if (Health <= 0) {
				ClumpusBody.QueueFree();
			}
		}
		
	}
	private void OnRArmHit(Area2D area) {
		--RArmHealth;
		if (string.Equals(area.Name, "Explosion")) {
			RArmHealth -= 9;
		}
		if (RArmHealth <= 0) {
			ClumpusBody.GetNode<Node2D>("RArmStart").QueueFree();
			ClumpusBody.GetNode<Node2D>("RArmEnd").QueueFree();
		}
	}
	private void OnLArmHit(Area2D area) {
		--LArmHealth;
		if (string.Equals(area.Name, "Explosion")) {
			LArmHealth -= 9;
		}
		if (LArmHealth <= 0) {
			ClumpusBody.GetNode<Node2D>("LArmStart").QueueFree();
			ClumpusBody.GetNode<Node2D>("LArmEnd").QueueFree();
		}
	}
	private void OnClusterLHit(Area2D area) {
		--ClusterLHealth;
		if (string.Equals(area.Name, "Explosion")) {
			ClusterLHealth -= 9;
		}
		if (ClusterLHealth <= 0) {
			ClusterLHealth = 0;
		}
		ClusterLHealthLabel.Text = ClusterLHealth.ToString();
		ClusterLHealthBar.Scale = new Vector2((float)ClusterLHealth / 100f, 1);
		if (ClusterLHealth <=0) {
			GD.Print("ClusterLDead");
			ClumpusBody.GetNode<Node2D>("ClusterL").QueueFree();
			CheckClusterDead();
		}
	}
	private void OnClusterRHit(Area2D area) {
		--ClusterRHealth;
		if (string.Equals(area.Name, "Explosion")) {
			ClusterRHealth -= 9;
		}
		if (ClusterRHealth <= 0) {
			ClusterRHealth = 0;
		}
		ClusterRHealthLabel.Text = ClusterRHealth.ToString();
		ClusterRHealthBar.Scale = new Vector2((float)ClusterRHealth / 100f, 1);
		if (ClusterRHealth <= 0) {
			GD.Print("ClusterRDead");
			ClumpusBody.GetNode<Node2D>("ClusterR").QueueFree();
			CheckClusterDead();
		}
	}
	private void OnClusterMHit(Area2D area) {
		--ClusterMHealth;
		if (string.Equals(area.Name, "Explosion")) {
			ClusterMHealth -= 9;
		}
		if (ClusterMHealth <= 0) {
			ClusterMHealth = 0;
		}
		ClusterMHealthLabel.Text = ClusterMHealth.ToString();
		ClusterMHealthBar.Scale = new Vector2((float)ClusterMHealth / 100f, 1);
		if (ClusterMHealth <= 0) {
			GD.Print("ClusterMDead");
			ClumpusBody.GetNode<Node2D>("ClusterM").QueueFree();
			CheckClusterDead();
		}
	}

	private void CheckClusterDead() {
		if (ClusterLHealth <= 0 && ClusterRHealth <= 0 && ClusterMHealth <= 0) {
			clustersDead = true;
		}
	}
}
