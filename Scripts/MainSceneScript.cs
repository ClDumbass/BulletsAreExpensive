using Godot;
using System;

public partial class MainSceneScript : Node
{
	[Export]
	public PackedScene StageOne { get; set; }
	[Export]
	public PackedScene StageTwo { get; set; }
	[Export]
	public PackedScene StageThree { get; set; }

	private Control MainMenuNode;
	private BaseStageScript activeStage;

	private int stageOneHighscore = 0;
	private int stageTwoHighscore = 0;
	private int stageThreeHighscore = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MainMenuNode = GetNode<Control>("MainMenu");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (activeStage != null && activeStage.StageComplete) {
			if (stageOneHighscore < activeStage.Score) {
				stageOneHighscore = activeStage.Score;
			}

			activeStage.QueueFree();
			activeStage = null;
			MainMenuNode.Visible = true;
			MainMenuNode.SetProcess(true);
		}
	}
	private void OnStageOneButtonClicked() {
		// Replace with function body.
		activeStage = StageOne.Instantiate<BaseStageScript>();
		AddChild(activeStage);
		MainMenuNode.Visible = false;
		MainMenuNode.SetProcess(false);
	}
	private void OnQuitClicked() {
		GetTree().Quit();
	}
}




