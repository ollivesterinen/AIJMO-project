//koodin opetteluun on käytetty apuna tekoälyä
// entinen picture mode on nimetty hardmodeksi josta picinstructions.cs tiedoston nimi tulee
using Godot;
using System;

public partial class picInstruction : Control
{

	public override void _Ready()
	{
		GetNode<Button>("CenterContainer1/VBoxContainer/HBoxContainer/back").Pressed += OnBackButtonPressed;
    	GetNode<Button>("CenterContainer1/VBoxContainer/HBoxContainer/skip").Pressed += OnSkipButtonPressed;
	}

	private void OnBackButtonPressed()
	{
		//koodi joka palauttaa käyttäjän suoraan main menuun asti on kommentoitu pois
		//change scene to the main menu
		// VAIHDETTU: Palataan oikeaan päävalikkoon kielen mukaan
		//if (menu.IsFinnish)
			//GetTree().ChangeSceneToFile("res://Scenes/finnish.tscn");
		//else
			//GetTree().ChangeSceneToFile("res://Scenes/start.tscn");
		// käyttäjä voi palata ohjeista takaisin vaikeustason valintaan
		if (menu.IsFinnish)
            GetTree().ChangeSceneToFile("res://Scenes/chooseMode_fi.tscn");
        else
            GetTree().ChangeSceneToFile("res://Scenes/chooseMode.tscn");
	}

	private void OnSkipButtonPressed()
	{
		//change scene to the English Gameplay
		// VAIHDETTU: Valitaan oikea peliscene kielen mukaan
		if (menu.IsFinnish)
			GetTree().ChangeSceneToFile("res://Scenes/HardMode_fi.tscn");
		else
			GetTree().ChangeSceneToFile("res://Scenes/HardMode.tscn");
	}
}
