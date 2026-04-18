//koodin opetteluun on käytetty apuna tekoälyä

using Godot;
using System;

public partial class Balloon : Area2D
{
	//public = GameLogic voi määrittää muuttujalle kirjaimen
	public string MyLetter = "";

	//Huom. tässä taulukko äänille
	[Export] private AudioStream[] _popSounds;
	[Export] private AudioStream _failSound;
	//2D = ääni tulee sen puolen kaiuttimesta, jota lähempänä poksautettu pallo oli
	private AudioStreamPlayer2D _audioPlayer;

	// nopeus jolla ilmapallot liikkuvat ylöspäin
	private float _riseSpeed = 150.0f;
	private AnimatedSprite2D _anim;
	//paikka pallon päälle tulevalle kirjaimelle
	private Label _label;
	//tila vaihtuu heti kun pallo on poksautettu -> 1 pallo ei poksahda 2 kertaa
	private bool _hasPopped = false;

	//tieto, että onko pallo jo poksahtanut, tämä on GameLogicia varten
	//kun GameLogic kysyy IsPopping:in tilaa, ohjaa sen katsomaan _hasPopped:in tilan
	//public = GameLogic voi lukea muuttujan, ei vaihtaa sen tilaa
	//siksi GameLogicia ei päästetä käsiksi privaattiin _hasPopped  muuttujaan
	public bool IsPopping => _hasPopped;

	private bool _animFinished = false;
	private bool _soundFinished = false;

	//laskee, missä kohtaa pallo on sen liikeradalla (sini-aalto)
	//kasvaa joka framella _Process-metodissa
	private float _timer = 0.0f;
	//anchor point pallolle jotta se ei liiku pois näytöltä
	private Vector2 _initialPosition;
	private float _hoverSpeed;
	private float _hoverHeight;

	//laskee kuinka kauan pallo pysyy punaisena
	private Timer _redColorTimer;

	public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_label = GetNode<Label>("Label");
		_audioPlayer = GetNode<AudioStreamPlayer2D>("PopPlayer");
		_redColorTimer = GetNode<Timer>("RedColorTimer");

		_redColorTimer.Timeout += OnRedColorTimeout;

		//GameLogic antaa pallolle kirjaimen
		_label.Text = MyLetter;

		//pallon aloituspaikka
		//Position = Godotin property joka sisältää objektin X ja Y koordinaatit
		_initialPosition = Position;

		_anim.Play("idle");

		if (_audioPlayer != null)
		{

			_audioPlayer.Finished += OnSoundFinished;
		}


		Random random = new Random();
		//satunainen aloitusaika pallon liikkeelle, välillä 0.0 ja 10.0
		//jotta pallot liikkuvat eri tahdissa
		//muutos floatiksi, koska Godotin koordinaatisto käyttää floatteja
		_timer = (float)random.NextDouble() * 10.0f;
		//satunnainen vauhti pallon liikkeelle, välillä 2.0 ja 4.0
		_hoverSpeed = 2.0f + (float)random.NextDouble() * 2.0f;
		//satunnainen liikkeen pituus, välillä 10.0 ja 20.0
		_hoverHeight = 10.0f + (float)random.NextDouble() * 10.0f;
	}

	public override void _Process(double delta)
	{
		if (_hasPopped) return;

		if (menu.IsHardMode)
		{
			// HARD MODE: Liikutetaan palloa tasaisesti ylöspäin
			Position += new Vector2(0, -_riseSpeed * (float)delta);

			// Poistetaan pallo, jos se menee ruudun yläreunan yli
			if (GlobalPosition.Y < -100)
			{
				QueueFree();
			}
		}
		else
		{

			_timer += (float)delta * _hoverSpeed;

			//_initialPosition.Y = pallon anchor point ja aloituspiste
			//laskee kuinka kauas pallo liikkuu aloituspisteestä
			float currentY = _initialPosition.Y + (MathF.Sin(_timer) * _hoverHeight);

			//X = alkuperäinen X arvo, koska pallo ei liiku sivulle
			//Y = laskettiin äsken
			//nextPos = pallon uusi paikka
			Vector2 nextPos = new Vector2(_initialPosition.X, currentY);

			//Position on Godotin property
			//kun Positionille annetaan uusi arvo, vasta tällöin pallon liikkuminen tapahtuu
			Position = nextPos;
		}
	}

	//annetaan 3 parametriä
	//Node viewport = ikkuna, jossa toiminto tapahtui
	//@event = minkälainen InputEvent tapahtui
	//long = long integer
	//shape_idx = mihin objektin collision shapeen osuttiin
	public void _on_input_event(Node viewport, InputEvent @event, long shape_idx)
	{
		if (_hasPopped) return;

		//tarkistaa, että @event oli hiiiren klikkaus (ja
		// varastoi tiedon väliaikaiseen muuttujaan mouseEvent) sekä että
		//tapahtuma oli hiiren napin alaspäin painallus
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			//etsii GameLogic noden ja laittaa sen muuttujaan
			//"true" = Godot etsii GameLogicia koko pelin rakenteesta
			//"false" = etsii kaikki solmuja riippumatta siitä, kuka ne teknisesti omistaa
			//(esim. luotu koodilla ilman parentia tai kuuluu aliskeneen)
			//"as GameLogic" = kohdellaan kuten luokkaa joten sen funktioita voidaan käyttää täällä
			var logic = GetTree().Root.FindChild("GameLogic", true, false) as GameLogic;
			if (logic != null)
			{
				//kutsuu GameLogicista (joka on nyt muuttuja logic) metodia CheckLetter
				//tarkistetaan, onko valittu kirjain oikea
				//this = kertoo GameLogicille, että juuri tätä palloa klikattiin
				if (logic.CheckLetter(MyLetter, this))
				{
					_hasPopped = true;
					//piilottaa kirjaimen
					_label.Visible = false;

					//Näytä tehosteeksi kirkas välähdys kun oikea pallo puhkaistaan
					Modulate = new Color(1.5f, 1.5f, 1.5f);

					PlayRandomPopSound();
					_anim.Play("pop");

					_anim.AnimationFinished += OnAnimationFinished;
				}
				else
				{
					// Jos pelaaja tekee virheen värjätään pallo punaiseksi
					Modulate = new Color(1.0f, 0.3f, 0.3f);
					//pallo pysyy punaisena kunnes ajastin on nollassa
					_redColorTimer.Start();

					PlayFailSound();
				}
			}
		}
	}

	private void OnAnimationFinished()
	{
		_animFinished = true;
		//piilotetaan pallo
		Visible = false;
		CheckIfCanDelete();
	}

	private void OnSoundFinished()
	{
		//tarkistaa ehdon, että tuliko ääni poksautuksesta (arvo = true)
		// vai väärästä kosketuksesta (arvo = false)
		if (_hasPopped)
		{
			_soundFinished = true;
			CheckIfCanDelete();
		}
	}

	private void CheckIfCanDelete()
	{
		if (_animFinished && _soundFinished)
		{   //deletoi pallon
			QueueFree();
		}
	}


	private void PlayRandomPopSound()
	{
		if (!menu.SfxEnabled)
			return;

		if (_popSounds != null && _popSounds.Length > 0 && _audioPlayer != null)
		{
			Random random = new Random();
			//luku 0 ja taulukon pituus -1 väliltä
			int randomIndex = random.Next(_popSounds.Length);
			_audioPlayer.Stream = _popSounds[randomIndex];
			_audioPlayer.Play();
		}
	}

	private void PlayFailSound()
	{
		if (!menu.SfxEnabled)
			return;

		if (_failSound != null && _audioPlayer != null)
		{
			_audioPlayer.Stream = _failSound;
			_audioPlayer.Play();
		}
	}

	private void OnRedColorTimeout()
	{
		// Vaihda pallon väri takaisin valkoiseksi
		Modulate = new Color(1, 1, 1, 1);
	}
}
