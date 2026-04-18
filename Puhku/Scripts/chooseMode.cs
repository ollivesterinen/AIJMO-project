//koodin opetteluun on käytetty apuna tekoälyä

using Godot;
using System;

public partial class chooseMode : Control
{
	public override void _Ready()
	{
		GetNode<Button>("CenterContainer1/VBoxContainer1/word").Pressed += OnWordButtonPressed;

		GetNode<Button>("CenterContainer1/VBoxContainer1/picture").Pressed += OnPictureButtonPressed;

		GetNode<Button>("CenterContainer2/back").Pressed += OnBackButtonPressed;
	}

	//create method
	private void OnWordButtonPressed()
	{
		// menu.isHardmode = false means easy mode is on.
		menu.IsHardMode = false;
		// VAIHDETTU: Valitaan oikea ohjeruutu kielen mukaan
		if (menu.IsFinnish)
			GetTree().ChangeSceneToFile("res://Scenes/wordInstruct_fi.tscn");
		else
			GetTree().ChangeSceneToFile("res://Scenes/wordInstruct.tscn");
	}

	private void OnPictureButtonPressed()
	{
		// menu.IsHardMode = true means balloons will act differently and gamemode is hard.
		menu.IsHardMode = true;
		// VAIHDETTU: Valitaan oikea ohjeruutu kielen mukaan
		if (menu.IsFinnish)
			GetTree().ChangeSceneToFile("res://Scenes/picInstruct_fi.tscn");
		else
			GetTree().ChangeSceneToFile("res://Scenes/picInstruct.tscn");
	}

	private void OnBackButtonPressed()
	{
		// change scene to the main menu
		// VAIHDETTU: Palataan oikeaan päävalikkoon kielen mukaan
		if (menu.IsFinnish)
			GetTree().ChangeSceneToFile("res://Scenes/finnish.tscn");
		else
			GetTree().ChangeSceneToFile("res://Scenes/start.tscn");
	}
}
