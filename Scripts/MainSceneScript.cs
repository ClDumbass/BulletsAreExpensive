using Godot;
using Godot.Collections;
using System;

public partial class MainSceneScript : Node
{
	[Export]
	public PackedScene StageOne { get; set; }
	[Export]
	public PackedScene StageTwo { get; set; }
	[Export]
	public PackedScene StageThree { get; set; }
	[Export]
	public AudioStreamPlayer MenuBoopPlayer { get; set; }

	private Control MainMenuNode;
	private Control MainSubmenuNode;
	private Control PerkSubmenuNode;
	private BaseStageScript activeStage;

	private int stageOneHighscore = 0;
	private int stageTwoHighscore = 0;
	private int stageThreeHighscore = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MainMenuNode = GetNode<Control>("MainMenu");
		MainSubmenuNode = GetNode<Control>("MainMenu/MainSubmenu");
		MainSubmenuNode.Visible = true;
		PerkSubmenuNode = GetNode<Control>("MainMenu/PerkSubmenu");
		PerkSubmenuNode.Visible = false;
		LoadData();
		UpdateScoreboard();
		GetNode<Control>("MainMenu/MainSubmenu/Stage1").GrabFocus();
	}

	private void UpdateScoreboard() {
		Label s1ScoreLabel = GetNode<Label>("MainMenu/MainSubmenu/Stage1/Score");
		Label s2ScoreLabel = GetNode<Label>("MainMenu/MainSubmenu/Stage2/Score");
		Label s3ScoreLabel = GetNode<Label>("MainMenu/MainSubmenu/Stage3/Score");

		s1ScoreLabel.Text = stageOneHighscore.ToString() + "cr";
		s2ScoreLabel.Text = stageTwoHighscore.ToString() + "cr";
		s3ScoreLabel.Text = stageThreeHighscore.ToString() + "cr";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (activeStage != null && activeStage.StageComplete) {
			if (stageOneHighscore < activeStage.Score) {
				stageOneHighscore = activeStage.Score;
				UpdateScoreboard();
			}

			activeStage.QueueFree();
			activeStage = null;
			MainMenuNode.Visible = true;
			MainMenuNode.SetProcess(true);
			GetNode<Control>("MainMenu/MainSubmenu/Stage1").GrabFocus();
		}
	}
	private void OnStageOneButtonClicked() {
		// Replace with function body.
		activeStage = StageOne.Instantiate<BaseStageScript>();
		AddChild(activeStage);
		MainMenuNode.Visible = false;
		MainMenuNode.SetProcess(false);
	}

	public void OnStageTwoButtonClicked() {

	}
	public void OnStageThreeButtonClicked() {

	}

	private void OnUpgradeButtonPressed() {
		MainSubmenuNode.Visible = false;
		PerkSubmenuNode.Visible = true;
		GetNode<Control>("MainMenu/PerkSubmenu/Back").GrabFocus();
	}
	private void OnPerksBackButtonPressed() {
		PerkSubmenuNode.Visible = false;
		MainSubmenuNode.Visible = true;
		GetNode<Control>("MainMenu/MainSubmenu/Upgrade").GrabFocus();
	}

	private void OnQuitClicked() {
		GetTree().Quit();
	}

	public void OnQuit() {
		GD.Print("SaveAttempt");
		SaveData();
	}

	public void OnUIFocusMoved() {
		MenuBoopPlayer.PitchScale = (float)(0.95 + Random.Shared.NextDouble() * 0.1);
		MenuBoopPlayer.Play();

	}

	private void SaveData() {
		using FileAccess fa = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);

		Dictionary<string, Variant> data = new Dictionary<string, Variant>();
		data.Add("stage1score", stageOneHighscore);
		data.Add("stage2score", stageTwoHighscore);
		data.Add("stage3score", stageThreeHighscore);

		string stringData = Json.Stringify(data);
		fa.StoreLine(stringData);
	}

	private void LoadData() {
		using FileAccess fa = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);
		
		string jsonString = fa.GetLine();

		Json js = new Json();
		Error resultState = js.Parse(jsonString);

		if (resultState != Error.Ok) {
			GD.Print($"JSON Parse Error: {js.GetErrorMessage()} in {jsonString} at line {js.GetErrorLine()}");
			return;
		}
		Dictionary<string, Variant> keyValuePairs = new Dictionary<string, Variant>((Godot.Collections.Dictionary)js.Data);

		stageOneHighscore = (int)keyValuePairs["stage1score"];
		stageTwoHighscore = (int)keyValuePairs["stage2score"];
		stageThreeHighscore = (int)keyValuePairs["stage3score"];
	}
}
