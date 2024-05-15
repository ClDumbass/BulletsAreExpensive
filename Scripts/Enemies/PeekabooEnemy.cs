using Godot;
using System;

/// <summary>
/// Peekaboo enemy will just barely come on screen, fire a long line of bullets in the desired direction, then duck back off screen and disappear.
/// </summary>
public partial class PeekabooEnemy : Node2D
{
    [Export]
    PackedScene bulletMaster { get; set; }
    [Export]
    AudioStreamPlayer BulletSound { get; set; }
    [Export]
    float Speed { get; set; } = 49;

    public Vector2 FireDirection { get; set; } = Vector2.Left;

    /// <summary>
    /// 0 = coming on screen
    /// 1 = firing bullets
    /// 2 = leaving screen
    /// </summary>
    private int sequence = 0;
    private float timer = 0;
    private float fireTimer = 0;
    private float targetRotation = 3 * MathF.PI / 4;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        timer += (float)delta;

        switch (sequence) {
            case 0:
                Position = new Vector2(Position.X - Speed * (float)delta, Position.Y);
                if (Position.X <= 480 - 16) {
                    Position = new Vector2(480 - 16, Position.Y);
                    sequence = 1;
                    timer = 0;
                }
                break;
            case 1:
                fireTimer += (float)delta;
                if (fireTimer >= 0.2f) {
                    fireTimer -= 0.2f;
                    FireBullet(FireDirection);
                }
                if (timer > 8f) {
                    timer = 0;
                    sequence = 2;
                }
                break;
            case 2:
                Position = new Vector2(Position.X + Speed * (float)delta, Position.Y);
                if (Position.X >= 480 + 16) {
                    QueueFree();
                }
                break;
            default:
                sequence = 0;
                timer = 0;
                break;
        }

        if (Position.X <= -100) {
            QueueFree();
        }
    }
    private void FireBullet(Vector2 direction) {
        BulletScript bulletClone = bulletMaster.Instantiate() as BulletScript;
        bulletClone.InitialPosition = Position;
        bulletClone.Direction = direction;
        bulletClone.Visible = true;
        GetParent().AddChild(bulletClone);
    }
    private void OnCollision(Area2D area) {
        QueueFree();
    }
}
