using Godot;
using System;

public partial class ScoreScript : Node
{
	[Export]
	public Label BulletScoreLabel { get; set; }
	[Export]
	public Label BombScoreLabel { get; set; }
	[Export]
	public Label HealthScoreLabel { get; set; }
	[Export]
	public Label TotalScoreLabel { get; set; }
	[Export]
	public Label TitleLabel { get; set; }

	public BaseStageScript stageScript { get; set; }
	public int Score { get { return score; } }
	private int score;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void TabulateScore(int bullets, int bombs, int health) {
		GD.Print("Tabulate!");
		score = bullets + 10 * bombs;

		BulletScoreLabel.Text = bullets.ToString() + "cr";
		BombScoreLabel.Text = (10 * bombs).ToString() + "cr";
		if (health <= 0) {
			HealthScoreLabel.Text = "0.1x";
			score /= 10;
			TitleLabel.Text = "YOU DIED";
		} else {
			HealthScoreLabel.Text = health.ToString() + "x";
			score *= health;
		}

		TotalScoreLabel.Text = score.ToString() + "cr";
	}

	public void OnReturnClicked() {
		stageScript.Score = score;
		stageScript.StageComplete = true;
		this.QueueFree();
	}
}
