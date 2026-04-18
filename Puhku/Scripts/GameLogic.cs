//koodin opetteluun on käytetty apuna tekoälyä
// sanalistat sisältävät csv-tiedostot on tehty tekoälyllä

using Godot;
using System;
using System.Collections.Generic;

public partial class GameLogic : Node
{
	// Artistien tekemät eri palloskenet raahataan tähän listaan Inspectorissa
	[Export] private PackedScene[] _balloonTemplates;

	private List<string> _wordPool = new List<string>();
	private string _targetWord = "";
	private int _currentIndex = 0;
	private int _lastWordIndex = -1;

	private RichTextLabel _uiDisplay;

	// MUUTETTU: Tyyppi vaihdettu CanvasLayeriksi, jotta koodi löytää solmun
	// Pausemenu ja sen napit.
	private CanvasLayer _pauseMenu;
	private Button _pauseButton;
	private Button _backToGame;
	private Button _backToMainMenu;

	// Pausemenun Musiikki ja SFX näppäimet
	private Button _musicOn;
	private Button _SFX_On;

	// Nappien tekstuurit
	private Texture2D _musicOnIcon;
	private Texture2D _musicOffIcon;
	private Texture2D _sfxOnIcon;
	private Texture2D _sfxOffIcon;

	// Audiostreamplayer (pyöritää musiikin pelin avatessa autoloadin kautta)
	private AudioStreamPlayer _bgm;

	bool paused = false;

	// --- VOITTORUUDUN OSAT ---
	// MUUTETTU: Tyyppi vaihdettu CanvasLayeriksi vastaamaan uutta hierarkiaa
	private CanvasLayer _winContainer;
	private Label _notificationLabel;
	private Button _nextLevelButton;
	private Button _mainMenuButton;

	//laskee montako palloa syntynyt siitä kun viimeisin oikea pallon on poksautettu
	private int _priorityCounter = 0;

	//ruutu, joka tulee näkyviin kun sanalista on käyty läpi
	private CanvasLayer _endOfWordList;
	private Label _noWordsLeft;
	private Button _mainMenu;

	private Timer _hardModeTimer;
	private List<string> _hardModePool = new List<string>();
	public override void _Ready()
	{
		_uiDisplay = GetNodeOrNull<RichTextLabel>("WordDisplay");

		// Haetaan CanvasLayer-tyyppinä, jotta Hide() toimii oikein alussa (Käyttöliittymän perusosat)
		_pauseMenu = GetNodeOrNull<CanvasLayer>("WindowCoverForPause");

		// Haetaan uusi ajastin hardmodelle
		_hardModeTimer = GetNodeOrNull<Timer>("HardModeTimer");
		if (_hardModeTimer != null)
		{
			_hardModeTimer.Timeout += SpawnHardModeBalloon;
		}


		// Piilotetaan pause-näkymä heti alussa
		if (_pauseMenu != null)
		{
			_pauseMenu.Hide();
		}

		// Haetaan ruutu, joka tulee näkyviin, kuns sanalista on käyty läpi
		_endOfWordList = GetNodeOrNull<CanvasLayer>("WinCoverEndWordList");
		//Haetaan palaa takaisin Main Menu -nappi yllä olevalle ruudulle
		_mainMenu = GetNodeOrNull<Button>("WinCoverEndWordList/EndOfWordList/CenterContainer/VBoxContainer/MainMenuList");

		//piilotetaa alussa ruutu, joka tulee esiin kun sanalista on käyty läpi
		if (_endOfWordList != null)
		{
			_endOfWordList.Hide();
		}



		// Etsitään sisältö PauseButtonille ja Pause-nappuloille (Polut tarkistettu hierarkiasta)
		_pauseButton = GetNodeOrNull<Button>("PauseButtonContainer/PauseButton");
		_backToGame = GetNodeOrNull<Button>("WindowCoverForPause/PauseMenu/CenterContainer/VBoxContainer/BackToGame");
		_backToMainMenu = GetNodeOrNull<Button>("WindowCoverForPause/PauseMenu/CenterContainer/VBoxContainer/MainMenu");

		// Haetaan pausemenun audioasetusten napit.
		_musicOn = GetNodeOrNull<Button>("WindowCoverForPause/PauseMenu/CenterContainer/VBoxContainer/MusicAndSound/HBoxContainer/MusicOn");
		_SFX_On = GetNodeOrNull<Button>("WindowCoverForPause/PauseMenu/CenterContainer/VBoxContainer/MusicAndSound/HBoxContainer/SFX_On");

		// Haetaan yleinen musiikkisoitin autoloadista.
		_bgm = GetNodeOrNull<AudioStreamPlayer>("/root/GlobalAudioStreamPlayer/MusicPlayer");

		// Haetaan tekstuurit musiikki- ja SFX-napeille.
		_musicOnIcon = GD.Load<Texture2D>("res://Assets/Icons/General/Music_on_yellow.png");
		_musicOffIcon = GD.Load<Texture2D>("res://Assets/Icons/General/Music_off_yellow.png");
		_sfxOnIcon = GD.Load<Texture2D>("res://Assets/Icons/General/SFX_on_yellow.png");
		_sfxOffIcon = GD.Load<Texture2D>("res://Assets/Icons/General/SFX_off_yellow.png");

		// Nää on vaan debug printit jos napit ei toimi :)
		GD.Print(_musicOn == null ? "MusicOn NOT FOUND" : "MusicOn FOUND");
		GD.Print(_SFX_On == null ? "SFX_On NOT FOUND" : "SFX_On FOUND");

		// Valitaan musiikkinapin ikoni matchaamaan musiikin tilaa ja tehdään toggle-mallinen nappi.
		if (_musicOn != null)
		{
			if (menu.MusicEnabled)
				_musicOn.Icon = _musicOnIcon;
			else
				_musicOn.Icon = _musicOffIcon;

			_musicOn.Pressed += OnPauseMusicTogglePressed;
		}
		// Sama kun ylempi mutta SFX napille.
		if (_SFX_On != null)
		{
			if (menu.SfxEnabled)
				_SFX_On.Icon = _sfxOnIcon;
			else
				_SFX_On.Icon = _sfxOffIcon;

			_SFX_On.Pressed += OnPauseSfxTogglePressed;
		}

		// MUUTETTU: Haetaan WinContainer CanvasLayerina (Eli voittoruutu)
		_winContainer = GetNodeOrNull<CanvasLayer>("WinContainer");

		if (_winContainer != null)
		{
			_winContainer.Hide();

			// PÄIVITETTY: Polut vastaamaan uutta CenterContainer/VBoxContainer -rakennetta
			_notificationLabel = _winContainer.GetNodeOrNull<Label>("CenterContainer/VBoxContainer/NotificationLabel");
			_nextLevelButton = _winContainer.GetNodeOrNull<Button>("CenterContainer/VBoxContainer/NextLevelButton");
			_mainMenuButton = _winContainer.GetNodeOrNull<Button>("CenterContainer/VBoxContainer/MainMenuButton");
		}



		// Sanat CSV:stä ja uuden levelin aloitus
		LoadWordsFromCSV();
		StartNewLevel();
	}

	private void StartNewLevel()
	{
		// Piilotetaan koko voittoruutu heti tason alussa
		if (_winContainer != null)
		{
			_winContainer.Hide();
		}

		// Poistetaan puhkaistut pallot
		ClearBalloons(null);

		Random random = new Random();
		//antaa satunnaisen luvun 0 ja _wordPool.Count - 1 väliltä
		int nextIndex = random.Next(_wordPool.Count);

		_targetWord = _wordPool[nextIndex];

		// poistetaan sana listalta, jottei sitä valita uudelleen
		//valittu sana on jo tallennettu _targetWord muuttujaan
		//joten sana voidaan poistaa
		_wordPool.RemoveAt(nextIndex);

		//nollaa pelaajan edistymisen uuden sanan alussa,
		//aloittaa oikean kirjaimen tarkistuksen uuden sanan 1. kirjaimesta
		_currentIndex = 0;

		// Päivitetään näkyvä sana ja luodaan pallot.
		UpdateUI();

		// --- HARD MODE ERITYISALUSTUS ---
		_priorityCounter = 0;
		_hardModePool.Clear();

		if (menu.IsHardMode)
		{
			// Hard modessa käynnistetään ajastin, joka syöttää palloja alareunasta
			_hardModeTimer?.Start();
		}
		else
		{
			// Easy modessa varmistetaan että ajastin on kiinni ja luodaan kaikki pallot heti kuten ennenkin
			_hardModeTimer?.Stop();
			SetupLevel();
		}
	}

	private void LoadWordsFromCSV()
	{
		// Luetaan sanat CSV-tiedostosta yksi rivi kerrallaan.
		// VAIHDETTU: Valitaan CSV kielivalinnan mukaan
		string csvPath = menu.IsFinnish ? "res://CSV/FinnishWordList.csv" : "res://CSV/EnglishWordList.csv";
		using var file = FileAccess.Open(csvPath, FileAccess.ModeFlags.Read);

		while (!file.EofReached())
		{
			string word = file.GetLine().Trim();

			if (word != "")
			{
				_wordPool.Add(word);
			}
		}
	}

	private void ClearBalloons(Balloon ignoreBalloon)
	{
		foreach (Node child in GetChildren())
		{
			if (child is Balloon balloon)
			{
				if (balloon != ignoreBalloon)
				{
					balloon.QueueFree();
				}
			}
		}
	}


	public bool CheckLetter(string letter, Balloon currentBalloon)
	{
		//Tarkistetaan, onko pelaajan klikkaama kirjain seuraava oikea kirjain.
		if (_currentIndex < _targetWord.Length && letter == _targetWord[_currentIndex].ToString())
		{
			_currentIndex++;
			UpdateUI();
			if (_currentIndex >= _targetWord.Length)
			{
				// Tyhjennetään muut pallot heti kun sana on valmis
				ClearBalloons(currentBalloon);
				HandleWin();
			}
			return true;
		}
		return false;
	}

	private async void HandleWin()
	{
		// Odotetaan sekunti jotta viimeinen poksahdus näkyy
		await ToSignal(GetTree().CreateTimer(1.0), "timeout");

		if (_wordPool.Count == 0)
		{
			_endOfWordList.Show();

		}

		// Näytetään koko voittoruutu + animaatio pyörähtää
		if (_winContainer != null && _wordPool.Count != 0)
		{
			_winContainer.Show();
		}

	}
	// Nappi, mistä pääsee seuraavaan tasoon
	public void _on_next_level_button_pressed()
	{
		StartNewLevel();
	}

	// Tämä on voittoruudun päävalikkonappi
	public void _on_main_menu_button_pressed()
	{
		GetTree().Paused = false;
		// VAIHDETTU: Palataan oikeaan päävalikkoon kielen mukaan
		if (menu.IsFinnish)
			GetTree().ChangeSceneToFile("res://Scenes/finnish.tscn");
		else
			GetTree().ChangeSceneToFile("res://Scenes/start.tscn");
	}

	// Tällä määritellään uuden tason asetukset
	private void SetupLevel()
	{
		if (_balloonTemplates == null || _balloonTemplates.Length == 0) return;

		// Lisätään oikeat kirjaimet
		List<string> lettersToSpawn = new List<string>();
		foreach (char c in _targetWord) lettersToSpawn.Add(c.ToString());

		//Lisätään sattumanvaraiset väärät kirjaimet.
		// VAIHDETTU: Käytetään suomalaista aakkostoa suomimoodissa
		string alphabet = menu.IsFinnish ? "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		Random random = new Random();
		for (int i = 0; i < 5; i++) lettersToSpawn.Add(alphabet[random.Next(alphabet.Length)].ToString());

		Vector2 screenSize = GetViewport().GetVisibleRect().Size;
		List<Vector2> placedPositions = new List<Vector2>();
		float minDistance = 180.0f; // Estetään pallojen päällekkäisyys

		// Pallojen luominen ja randomisointi
		foreach (string letter in lettersToSpawn)
		{
			Vector2 finalPos = Vector2.Zero;
			bool foundSpot = false;
			for (int attempt = 0; attempt < 100; attempt++)
			{
				float x = (float)random.Next(100, (int)screenSize.X - 100);
				float y = (float)random.Next(100, (int)screenSize.Y - 250);
				Vector2 testPos = new Vector2(x, y);
				bool tooClose = false;
				foreach (Vector2 existingPos in placedPositions)
				{
					if (testPos.DistanceTo(existingPos) < minDistance)
					{
						tooClose = true;
						break;
					}
				}
				if (!tooClose)
				{
					finalPos = testPos;
					foundSpot = true;
					break;
				}
			}
			if (foundSpot)
			{
				int randomIndex = random.Next(_balloonTemplates.Length);
				Balloon newBalloon = _balloonTemplates[randomIndex].Instantiate<Balloon>();

				newBalloon.MyLetter = letter;
				newBalloon.Position = finalPos;

				placedPositions.Add(finalPos);
				AddChild(newBalloon);
			}
		}
	}

	private void UpdateUI()
	{
		if (_uiDisplay == null) return;

		// Kasataan sana niin, että jo löydetyt kirjaimet näkyy kirkkaina ja loput haaleina
		string brightOpen = "[color=white]";
		string ghostOpen = "[color=#ffffff66]";
		string closeTag = "[/color]";
		string finalBBCode = "[center]";

		for (int i = 0; i < _targetWord.Length; i++)
		{
			if (i < _currentIndex)
				finalBBCode += brightOpen + _targetWord[i] + closeTag + " ";
			else
				finalBBCode += ghostOpen + _targetWord[i] + closeTag + " ";
		}

		finalBBCode += "[/center]";
		_uiDisplay.Text = finalBBCode;
	}

	public void _on_pause_button_pressed()
	{
		// vaihdetaan peli pause-tilaan
		GetTree().Paused = true;
		if (_pauseMenu != null) _pauseMenu.Show();
	}

	public void _on_back_to_game_pressed()
	{
		GetTree().Paused = false;
		if (_pauseMenu != null) _pauseMenu.Hide();
	}

	// Tämä on pause-valikon päävalikkonappi
	public void _on_main_menu_pressed()
	{
		GetTree().Paused = false;
		// VAIHDETTU: Palataan oikeaan päävalikkoon kielen mukaan
		if (menu.IsFinnish)
			GetTree().ChangeSceneToFile("res://Scenes/finnish.tscn");
		else
			GetTree().ChangeSceneToFile("res://Scenes/start.tscn");
	}

	//Kun sanalista on käyty loppuun, tämä nappi tulee esiin ruudulle
	public void _on_main_menu_list_pressed()
	{
		// VAIHDETTU: Palataan oikeaan päävalikkoon kielen mukaan
		if (menu.IsFinnish)
			GetTree().ChangeSceneToFile("res://Scenes/finnish.tscn");
		else
			GetTree().ChangeSceneToFile("res://Scenes/start.tscn");
	}

	// Audiometodit
	private void OnPauseMusicTogglePressed()
	{
		GD.Print("PAUSE MUSIC BUTTON WORKS");

		menu.MusicEnabled = !menu.MusicEnabled;

		if (menu.MusicEnabled)
		{
			if (_bgm != null)
				_bgm.Play();

			if (_musicOn != null)
				_musicOn.Icon = _musicOnIcon;
		}
		else
		{
			if (_bgm != null)
				_bgm.Stop();

			if (_musicOn != null)
				_musicOn.Icon = _musicOffIcon;
		}
	}

	private void OnPauseSfxTogglePressed()
	{
		GD.Print("PAUSE SFX BUTTON WORKS");

		menu.SfxEnabled = !menu.SfxEnabled;

		if (_SFX_On != null)
		{
			if (menu.SfxEnabled)
				_SFX_On.Icon = _sfxOnIcon;
			else
				_SFX_On.Icon = _sfxOffIcon;
		}
	}

	private void SpawnHardModeBalloon()
	{
		// Jos koko sana on jo löydetty, lopetetaan pallojen syöttö
		if (_currentIndex >= _targetWord.Length)
		{
			_hardModeTimer?.Stop();
			return;
		}

		Random random = new Random();
		string letter;

		// Nostetaan laskuria jokaisella spawnauksella
		_priorityCounter++;

		// TAKUULOGIIKKA: Joka 3. pallo on aina se, mitä pelaaja tarvitsee seuraavaksi
		if (_priorityCounter >= 5)
		{
			letter = _targetWord[_currentIndex].ToString();
			_priorityCounter = 0; // Nollataan takuu, jotta seuraavat kaksi ovat taas satunnaisempia
		}
		else
		{
			// Jos ei ole takuuvuoro, käytetään poolia (kuten SetupLevel-logiikassa)
			if (_hardModePool.Count == 0)
			{
				// Täytetään pooli sanan kirjaimilla
				foreach (char c in _targetWord) _hardModePool.Add(c.ToString());

				// Lisätään muutama extra häiriöksi
				// VAIHDETTU: Käytetään suomalaista aakkostoa suomimoodissa
				string alphabet = menu.IsFinnish ? "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
				// muuttamalla loopin i < (arvo) arvoa, voi muuttaa montako extra palloa peli spawnaa häiriöksi
				for (int i = 0; i < 6; i++) _hardModePool.Add(alphabet[random.Next(alphabet.Length)].ToString());
			}

			// Valitaan satunnainen kirjain poolista ja poistetaan se (estää toiston)
			int idx = random.Next(_hardModePool.Count);
			letter = _hardModePool[idx];
			_hardModePool.RemoveAt(idx);
		}

		Vector2 screenSize = GetViewport().GetVisibleRect().Size;
		float finalX = 0;
		bool foundSpot = false;

		// PÄÄLLEKKÄISYYDEN ESTO (Teidän alkuperäinen 100 yrityksen logiikka)
		for (int attempt = 0; attempt < 100; attempt++)
		{
			float testX = (float)random.Next(100, (int)screenSize.X - 100);
			bool tooClose = false;

			// Tarkistetaan etäisyys muihin ruudulla oleviin palloihin (X-akselilla)
			foreach (Node child in GetChildren())
			{
				if (child is Balloon otherBalloon)
				{
					// Käytetään 100px väliä, jotta se on reilu mutta toimiva
					if (Mathf.Abs(testX - otherBalloon.Position.X) < 100.0f)
					{
						tooClose = true;
						break;
					}
				}
			}

			if (!tooClose)
			{
				finalX = testX;
				foundSpot = true;
				break;
			}
		}

		// Luodaan pallo vain jos sille löytyi paikka
		if (foundSpot)
		{
			int templateIndex = random.Next(_balloonTemplates.Length);
			Balloon newBalloon = _balloonTemplates[templateIndex].Instantiate<Balloon>();

			newBalloon.MyLetter = letter;
			// Pallo spawnaa ruudun alareunaan
			newBalloon.Position = new Vector2(finalX, screenSize.Y + 100);

			AddChild(newBalloon);
		}
	}
}
