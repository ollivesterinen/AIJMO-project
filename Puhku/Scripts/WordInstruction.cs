//koodin opetteluun on käytetty apuna tekoälyä
//Wordinstructions.cs kooditiedosto on nimetty vanhan word moden mukaan, joka on nykyisin easy mode.
using Godot;
using System;

public partial class WordInstruction : Control
{

    public override void _Ready()
    {
        GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/back").Pressed += OnBackButtonPressed;
        GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/skip").Pressed += OnSkipButtonPressed;
    }

    private void OnBackButtonPressed()
    {
        // this section disabled with comments
        //change scene to the main menu
        // VAIHDETTU: Palataan oikeaan päävalikkoon kielen mukaan
        //if (menu.IsFinnish)
            //GetTree().ChangeSceneToFile("res://Scenes/finnish.tscn");
        //else
            //GetTree().ChangeSceneToFile("res://Scenes/start.tscn");
        //return to choosemode.tscn depending on language
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
            GetTree().ChangeSceneToFile("res://Scenes/Main_fi.tscn");
        else
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }
}
