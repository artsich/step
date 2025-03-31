using Silk.NET.Input;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Graphics;
using Step.Main.Gameplay.Actors;
using Step.Main.Gameplay.UI;

namespace Step.Main.Gameplay;

public class GameStateManager
{
	private GameState _currentState;
	private readonly Engine.Engine _engine;
	private readonly Renderer _renderer;

	private MainMenu? _mainMenu;
	private GameLoop? _gameLoop;

	private readonly Camera2d _menuCamera;
	private readonly Camera2d _gameCamera;

	private readonly float _cameraWidth;
	private readonly float _cameraHeight;

	private Texture2d? _gliderTexture;
	private Texture2d? _circleTexture;
	private Texture2d? _playerTexture;
	private Texture2d? _crossTexture;

	public GameStateManager(Engine.Engine engine, float cameraWidth, float cameraHeight)
	{
		_engine = engine;
		_renderer = engine.Renderer;
		_currentState = GameState.Start;

		_cameraWidth = cameraWidth;
		_cameraHeight = cameraHeight;

		_menuCamera = new Camera2d(cameraWidth, cameraHeight);
		_gameCamera = new Camera2d(cameraWidth, cameraHeight);

		InitializeMainMenu();

		_engine.Keyboard.KeyDown += HandleKeyDown;

		GameRoot.I.SetScene(_mainMenu!);
		GameRoot.I.CurrentCamera = _menuCamera;
	}

	private void InitializeMainMenu()
	{
		_mainMenu = new MainMenu(_engine);

		_mainMenu.OnPlayPressed += () =>
		{
			_mainMenu.CallDeferred(() =>
			{
				_mainMenu.SetContinueButtonEnabled(true);
				ToPlayState();
			});
		};

		_mainMenu.OnContinuePressed += () =>
		{
			_mainMenu.CallDeferred(() =>
			{
				ToGameState();
			});
		};

		_mainMenu.OnExitPressed += () =>
		{
			_mainMenu.CallDeferred(() =>
			{
				ToExitState();
			});
		};
	}

	private void LoadAssets()
	{
		AudioManager.Ins.LoadSound("start", "Music/ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("main_theme", "Music/air-ambience-234180.mp3");
		AudioManager.Ins.LoadSound("player_hurt_glider", "Music/hurt.wav");
		AudioManager.Ins.LoadSound("player_hurt_circle", "Music/hurt2.wav");
		AudioManager.Ins.LoadSound("player_pickup", "Music/pickup.wav");
		AudioManager.Ins.LoadSound("wall_collision", "Music/wall_collision.mp3");

		_gliderTexture = Assets.LoadTexture2d("Textures/glider-enemy.png");
		_circleTexture = Assets.LoadTexture2d("Textures/circle-enemy.png");
		_playerTexture = Assets.LoadTexture2d("Textures/player.png");
		_crossTexture = Assets.LoadTexture2d("Textures/cross-enemy.png");
	}

	private void InitializeGameLoop()
	{
		_gameLoop = new GameLoop(_engine);
		_gameLoop.AddChild(_gameCamera);

		var player = CreatePlayer();
		var frame = new Frame(_renderer);
		var spawner = CreateSpawner(player);

		_gameLoop.AddChild(frame);
		_gameLoop.AddChild(player);
		_gameLoop.AddChild(spawner);

		_gameLoop.OnFinish += OnGameFinish;
	}

	private void OnGameFinish()
	{
		_currentState = GameState.Start;
		_mainMenu!.SetContinueButtonEnabled(false);
		ToPausedState();
	}

	private void HandleKeyDown(IKeyboard keyboard, Key key, int arg3)
	{
		if (key == Key.Escape)
		{
			switch (_currentState)
			{
				case GameState.Game:
					ToPausedState();
					break;
				default:
					break;
			}
		}
	}

	private Player CreatePlayer()
	{
		var player = new Player(
			_engine.Input,
			new RectangleShape2d(_renderer)
			{
				Size = new Vector2f(16f),
				CollisionLayers = (int)PhysicLayers.Player,
				CollisionMask = (int)(PhysicLayers.Enemy | PhysicLayers.Frame)
			});

		var playerSprite = new PlayerSprite();
		playerSprite.AddChild(new Sprite2d(_renderer, _playerTexture!)
		{
			Name = "Frame",
			Color = GameColors.Player,
		});

		playerSprite.AddChild(new Sprite2d(_renderer, _renderer.DefaultWhiteTexture)
		{
			Name = "Health",
			Color = Color.LightGreen,
			Pivot = new Vector2f(0f),
			LocalTransform = new Transform()
			{
				Position = new Vector2f(-8f, -8f),
				Scale = new Vector2f(16f),
			}
		});

		player.AddChild(playerSprite);
		player.AddAbility(new SpeedIncreaseAbility(player));
		player.AddAbility(new RegenerationAbility(player));
		player.AddAbility(new SizeChangerAbility(player) { Duration = 3f });
		player.AddAbility(new TimeFreezeAbility() { Duration = 2f });
		player.AddAbility(new MagnetAbility(50f, player, _renderer));
		player.AddAbility(new ShieldAbility(player, new PlayerShield(_engine.Input, _renderer)) { Duration = 3f });

		return player;
	}

	private Spawner CreateSpawner(Player player)
	{
		var enemyFactory = new EnemyFactory(
			_renderer,
			_gliderTexture!,
			_circleTexture!,
			_crossTexture!,
			player);

		return new Spawner(new Box2f(-_cameraWidth / 2f, -_cameraHeight / 2f, _cameraWidth / 2f, _cameraHeight / 2f),
		[
			new SpawnRule
			{
				StartTime = 0f,
				SpawnWeight = 1f,
				CreateEntity = enemyFactory.CreateCircle
			},
			new SpawnRule
			{
				StartTime = 60f,
				SpawnWeight = 0.05f,
				CreateEntity = enemyFactory.CreateGlider
			},
			new SpawnRule
			{
				StartTime = 30f,
				SpawnWeight = 0.1f,
				CreateEntity = enemyFactory.CreateCross,
				SpawnLocation = SpawnLocationType.Interior,
			}
		]);
	}

	public void ToPausedState()
	{
		_currentState = GameState.Paused;
		_mainMenu!.Enabled = true;

		GameRoot.I.SwapScene(_mainMenu);
		GameRoot.I.CurrentCamera = _menuCamera;
	}

	public void ToPlayState()
	{
		LoadAssets();
		InitializeGameLoop();

		_gameLoop!.Enabled = true;
		_mainMenu!.Enabled = false;

		GameRoot.I.SetScene(_gameLoop);
		GameRoot.I.CurrentCamera = _gameCamera;

		_mainMenu.NewGameInstead(true);

		_currentState = GameState.Game;
	}

	public void ToGameState()
	{
		_currentState = GameState.Game;

		_gameLoop!.Enabled = true;
		_mainMenu!.Enabled = false;

		GameRoot.I.SwapScene(_gameLoop);
		GameRoot.I.CurrentCamera = _gameCamera;
	}

	public void ToExitState()
	{
		_currentState = GameState.Exit;
		_engine.Window.Close();
	}

	public GameState GetCurrentState()
	{
		return _currentState;
	}

	public void Unload()
	{
		_engine.Keyboard.KeyDown -= HandleKeyDown;
		AudioManager.Ins.UnloadSounds();
	}
}