using Godot;
using System;

public partial class PlayerControl : Sprite2D
{
	[Export]
	public Label textLabel { get; set; }
	[Export]
	public PackedScene bulletMaster { get; set; }
	[Export]
	public PackedScene bombMaster { get; set; }

	const float MOVE_SPEED = 90.0f;
	// Called when the node enters the scene tree for the first time.

	public int Health { get { return health; } }
	private int health = 3;
	public int Bullets { get { return bullets; } }
	private int bullets = 10;
	public int Bombs { get { return bombs; } }
	private int bombs = 1;
	private int mines = 0;

	private float iframes = 0;
	private float bulletCooldown = 0;
	private float bombCooldown = 0;
	private CollisionShape2D hitbox;

	private Sprite2D playerSprite;
	public override void _Ready()
	{
		textLabel.Text = "Health: 3";
		hitbox = GetNode<CollisionShape2D>("Hitbox/CollisionShape2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (iframes > 0) {
			iframes -= (float)delta;
		} else {
			iframes = 0;
		}
		bulletCooldown -= (float)delta;
		bombCooldown -= (float)delta;

		textLabel.Text = "Health: " + health.ToString() + 
						 "\n Bullets: " + bullets.ToString() +
						 "\n Bombs: " + bombs.ToString();

		//simple sprite visibility toggle to animate iframes?
		int iframeAnimationState = (int)(iframes * 20f);
		if (iframeAnimationState % 2 == 0) {
			Visible = true;
		}
		else {
			Visible = false;
		}

		///Movement
		float speed = (float)delta * MOVE_SPEED;
		if (Input.IsActionPressed("move_slowly")) {
			speed *= 0.67f;
		}

		Vector2 positionOffset = new Vector2();
		if (Input.IsActionPressed("move_left")) {
			positionOffset.X -= 1;
		}
		if (Input.IsActionPressed("move_right")) {
			positionOffset.X += 1;
		}
		if (Input.IsActionPressed("move_up")) {
			positionOffset.Y -= 1;
		}
		if (Input.IsActionPressed("move_down")) {
			positionOffset.Y += 1;
		}

		if (positionOffset.Y != 0 && positionOffset.X != 0) {
			positionOffset = new Vector2(positionOffset.X * 0.7f * speed, 
										 positionOffset.Y * 0.7f * speed);
		} else {
			positionOffset = new Vector2(positionOffset.X * speed, 
										 positionOffset.Y * speed);
		}

		Vector2 newPos = this.Position + positionOffset;

		float edgeBuffer = 12;

		this.Position = new Vector2(Math.Clamp(newPos.X, edgeBuffer, 480 - edgeBuffer), Math.Clamp(newPos.Y, edgeBuffer, 270 - edgeBuffer));
		
		///Attacking
		if (Input.IsMouseButtonPressed(MouseButton.Left) && bullets > 0 && bulletCooldown <= 0) {
			bulletCooldown = 0.3f / (int)Math.Sqrt(bullets);
			--bullets;
			BulletScript bulletClone = bulletMaster.Instantiate() as BulletScript;

			bulletClone.InitialPosition = Position;
			bulletClone.Direction = Vector2.Right;
			bulletClone.Speed = 120;

			Area2D bulletHitbox = bulletClone.GetNode<Area2D>("Bullet");
			bulletHitbox.CollisionLayer = 4;
			bulletHitbox.CollisionMask = 2;

			AddSibling(bulletClone);
		}
		if (Input.IsMouseButtonPressed(MouseButton.Right) && bombs > 0 && bombCooldown <= 0) {
			--bombs;
			bombCooldown = 1.0f;

			BombScript bombClone = bombMaster.Instantiate() as BombScript;

			bombClone.InitialPosition = Position;
			bombClone.TargetLocation = Position + new Vector2(200, 0);
			bombClone.Visible = true;
			bombClone.Speed = 200;
			bombClone.Radius = 64;

			Area2D bombHitbox = bombClone.GetNode<Area2D>("Bomb");
			bombHitbox.CollisionLayer = 4;
			bombHitbox.CollisionMask = 2;

			Area2D explosionHitbox = bombClone.GetNode<Area2D>("Explosion");
			explosionHitbox.CollisionLayer = 4;
			explosionHitbox.CollisionMask = 2;

			AddSibling(bombClone);
		}
	}
	private void OnCollision(Area2D area) {
		if (iframes > 0) return;
		if (area.IsQueuedForDeletion()) {
			return;
		}

		--health;
		iframes = 3.0f;
	}
	private void OnBulletExit(Area2D area) {
		if ( area.IsQueuedForDeletion() || area.GetParent().IsQueuedForDeletion() || Health <=0)
		{
			return;
		}
		if (area.GetParent() is BulletScript) {
			++bullets;
			Absorbed absorbedEffect = new Absorbed
			{
				Position = area.Position,
				player = this,
				Radius = 4,
				Visible = true
			};
			AddSibling(absorbedEffect);
		}
		else if (string.Equals(area.Name, "Bomb")) {
			++bombs;
			Absorbed absorbedEffect = new Absorbed
			{
				Position = area.Position,
				player = this,
				Radius = 8,
				Visible = true
			};
			AddSibling(absorbedEffect);
		}
		area.GetParent().QueueFree();
	}
}




