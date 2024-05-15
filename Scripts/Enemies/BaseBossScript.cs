using Godot;
using System;

public partial class BaseBossScript : Node2D
{
    /// <summary>
    /// Set to true when boss is fully dead and the level should end.
    /// </summary>
    public bool Dead { get; set; } = false;
    public float EnrageTimer { get; set; } = 90.0f;
    [Export]
    public EnemyMasterScript EnemyMaster { get; set; }
    [Export]
    public Node2D PlayerNode { get; set; }
    public Label EnrageTimerLabel { get; set; }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
