using Godot;
using System;

public partial class TwinziesScript : BaseBossScript
{
	float timer = 0;
	/// <summary>
	/// 0 - rush on screen until about 320 with laser on
	/// </summary>
	int sequence = 0;

	private Node2D pairedMovement;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pairedMovement = GetNode<Node2D>("PairedMovement");
		pairedMovement.Position = new Vector2(-64, 32);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EnrageTimer -= (float)delta;
		EnrageTimerLabel.Text = EnrageTimer.ToString("0.00") + "s";

		switch (sequence) {
			case 0:
				pairedMovement.Position += new Vector2(240f * (float)delta, 0);
				if (pairedMovement.Position.X > 320) {
					pairedMovement.Position = new Vector2(320, pairedMovement.Position.Y);
					sequence = 1;
					QueueRedraw();
				}
				break;
			default:
				break;
		}
		QueueRedraw();
	}

	public override void _Draw() {
		if (sequence == 0) {
			DrawLine(GetNode<Node2D>("PairedMovement/Top").GlobalPosition, GetNode<Node2D>("PairedMovement/Bottom").GlobalPosition, new Color(0.3f, 1.0f, 0.3f, 0.7f), 10);
		}
	}
}
