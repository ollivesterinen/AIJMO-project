//koodin opetteluun on käytetty apuna tekoälyä

using Godot;
using System;

public partial class menu : Control
{
	private AudioStreamPlayer _bgm;

	//public static = whatever the value of these variables,
	// the state applies to other parts of the game too
	public static bool SfxEnabled = true;
	public static bool MusicEnabled = true;
	public static bool IsHardMode = false;
	public static bool IsFinnish = false;

	private Button _musicToggle;

	//music is already playing when the scene starts
	private bool _musicOn = true;
	private Texture2D _musicOnIcon;
	private Texture2D _musicOffIcon;

	private Button _sfxToggle;
	//SFX is already on when the scene starts
	private bool _sfxOn = true;
	private Texture2D _sfxOnIcon;
	private Texture2D _sfxOffIcon;

	public override void _Ready()
	{
		// VAIHDETTU: Asetetaan IsFinnish sen mukaan missä scenessä ollaan
		// jotta kielivalinta pysyy oikeana vaikka _Ready() ajetaan uudelleen
		if (GetTree().CurrentScene.SceneFilePath == "res://Scenes/finnish.tscn")
			IsFinnish = true;
		else if (GetTree().CurrentScene.SceneFilePath == "res://Scenes/start.tscn")
			IsFinnish = false;

		//get the content of node
		//"/root/" = looking for the node in Scene Tree root
		//this is autoload node so the state of music on/off and SFX on/off
		//set in this scene carries on to other scenes
		_bgm = GetNode<AudioStreamPlayer>("/root/GlobalAudioStreamPlayer/MusicPlayer");


		//checks if music should be playing
		if (MusicEnabled)
		{
			//checks if music is already playing
			//if not, start playing music
			//prevents the music starting from the beginning when scene changes
			if (!_bgm.Playing)
			{
				_bgm.Play();
			}
		}
		else
		{
			_bgm.Stop();
		}

		_musicToggle = GetNode<Button>("CenterContainer3/HBoxContainer/musicToggle");

		//load icons, so when changing the state of music
		//the icon swaps to the other one immediately
		//Note: the long file path and GD.Load because code checks the whole file system of the game
		//not just current scene because other scenes use the same icon
		_musicOnIcon = GD.Load<Texture2D>("res://Assets/Icons/General/Music_on_yellow.png");
		_musicOffIcon = GD.Load<Texture2D>("res://Assets/Icons/General/Music_off_yellow.png");

		//set starting icon
		if (MusicEnabled)
		{
			_musicOn = true;
			_musicToggle.Icon = _musicOnIcon;
		}
		else
		{
			_musicOn = false;
			_musicToggle.Icon = _musicOffIcon;
		}

		_musicToggle.Pressed += OnMusicTogglePressed;

		_sfxToggle = GetNode<Button>("CenterContainer3/HBoxContainer/SFX_Toggle");

		//load icons, so when changing the state of SFX
		//the icon swaps to the other one immediately
		//Note: the long file path and GD.Load because code checks the whole file system of the game
		//not just current scene because other scenes use the same icon
		_sfxOnIcon = GD.Load<Texture2D>("res://Assets/Icons/General/SFX_on_yellow.png");
		_sfxOffIcon = GD.Load<Texture2D>("res://Assets/Icons/General/SFX_off_yellow.png");

		//set starting icon
		if (SfxEnabled)
		{
			_sfxOn = true;
			_sfxToggle.Icon = _sfxOnIcon;
		}
		else
		{
			_sfxOn = false;
			_sfxToggle.Icon = _sfxOffIcon;
		}

		_sfxToggle.Pressed += OnSFXTogglePressed;

		// VAIHDETTU: Haetaan napit GetNodeOrNull:lla koska suomi- ja enkku-scenessä napit ovat eri nimisiä
		var newGameBtn = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/newGame");
		if (newGameBtn != null) newGameBtn.Pressed += OnNewGameButtonPressed;

		var uusiPeliBtn = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/uusiPeli");
		if (uusiPeliBtn != null) uusiPeliBtn.Pressed += OnNewGameButtonPressed;

		var quitBtn = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/quitGame");
		if (quitBtn != null) quitBtn.Pressed += OnQuitGameButtonPressed;

		var poistuBtn = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/poistuPelistä");
		if (poistuBtn != null) poistuBtn.Pressed += OnQuitGameButtonPressed;

		var finnishBtn = GetNodeOrNull<Button>("CenterContainer2/HBoxContainer/finnish");
		if (finnishBtn != null) finnishBtn.Pressed += OnFinnishButtonPressed;

		var englishBtn = GetNodeOrNull<Button>("CenterContainer2/HBoxContainer/english");
		if (englishBtn != null) englishBtn.Pressed += OnEnglishButtonPressed;
	}

	private void OnNewGameButtonPressed()
	{
		//change scene to choose mode menu
		// VAIHDETTU: Valitaan oikea chooseMode kielen mukaan
		if (IsFinnish)
			GetTree().ChangeSceneToFile("res://Scenes/chooseMode_fi.tscn");
		else
			GetTree().ChangeSceneToFile("res://Scenes/chooseMode.tscn");
	}

	private void OnQuitGameButtonPressed()
	{

		GetTree().Quit();
	}

	private void OnFinnishButtonPressed()
	{
		// VAIHDETTU: Asetetaan suomimoodi päälle ennen scenen vaihtoa
		IsFinnish = true;
		GetTree().ChangeSceneToFile("res://Scenes/finnish.tscn");
	}

	private void OnEnglishButtonPressed()
	{
		// VAIHDETTU: Asetetaan suomimoodi pois päältä ennen scenen vaihtoa
		IsFinnish = false;
		//change into the same scene if the English language button is pressed
		GetTree().ChangeSceneToFile("res://Scenes/start.tscn");
	}


	private void OnMusicTogglePressed()
	{
		//vaihdetaan nappulan tila
		if (_musicOn == true)
		{
			_musicOn = false;
		}
		else
		{
			_musicOn = true;
		}
		//updates the global static variable so the state of music on/off
		//carries on to other scenes
		MusicEnabled = _musicOn;

		if (_musicOn)
		{
			_bgm.Play();
			_musicToggle.Icon = _musicOnIcon;
		}
		else
		{
			_bgm.Stop();
			_musicToggle.Icon = _musicOffIcon;
		}
	}

	private void OnSFXTogglePressed()
	{
		//vaihdetaan nappulan tila
		if (_sfxOn == true)
		{
			_sfxOn = false;
		}
		else
		{
			_sfxOn = true;
		}

		//updates the global static variable so the state of SFX on/off
		//carries on to other scenes
		SfxEnabled = _sfxOn;

		/*no need to start or stop SFX playing with specific code as done above with music
		because SFX sounds are so short that they don't play simultaneously when
		state of SfxEnabled is updated and after the status update the game knows immediately whether play
		or not to play SFX*/

		if (_sfxOn)
		{
			_sfxToggle.Icon = _sfxOnIcon;
		}
		else
		{
			_sfxToggle.Icon = _sfxOffIcon;
		}
	}
}
