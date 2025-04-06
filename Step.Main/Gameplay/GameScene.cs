using Silk.NET.Input;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Graphics;
using Step.Main.Gameplay.Builders;
using Step.Main.Gameplay.UI;

namespace Step.Main.Gameplay;

public class GameScene : GameObject
{
	private GameState _currentState = GameState.Start;
	private readonly Engine.Engine _engine;

	private MainMenu? _mainMenu;
	private GameLoop? _gameLoop;
	private Viewport? _uiViewport;
	private Viewport? _gameViewport;

	private readonly Camera2d _menuCamera;
	private readonly Camera2d _gameCamera;

	public Viewport MainViewport => _currentState == GameState.Game ? _gameViewport! : _uiViewport!;

	public GameScene(Engine.Engine engine, float cameraWidth, float cameraHeight) : base("GameRoot")
	{
		_engine = engine;

		_menuCamera = new Camera2d(cameraWidth, cameraHeight);
		_gameCamera = new Camera2d(cameraWidth, cameraHeight);

		_engine.Keyboard.KeyDown += HandleKeyDown;
		InitializeMainMenu();
	}

	private void InitializeMainMenu()
	{
		_mainMenu = new MainMenu(_engine);

		_mainMenu.OnPlayPressed += () =>
		{
			_mainMenu.CallDeferred(() =>
			{
				_mainMenu.SetContinueButtonEnabled(true);
				ToPlayNewState();
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

		_uiViewport = new Viewport(_engine, _menuCamera, _engine.Window.FramebufferSize);
		_uiViewport.AddChild(_mainMenu);

		AddChild(_uiViewport);
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

	public void ToPausedState()
	{
		_currentState = GameState.Paused;

		_gameViewport!.Enabled = false;
		_mainMenu!.Enabled = true;
	}

	public void ToPlayNewState()
	{
		if (_gameViewport != null)
		{
			var gameView = _gameViewport;
			CallDeferred(() =>
			{
				RemoveChild(gameView);
				gameView.End();
				gameView.Dispose();
			});
		}

		_gameLoop = new GameBuilder(_engine).Build();
		_gameLoop.AddChild(_gameCamera);
		_gameLoop.OnFinish += OnGameFinish;

		_gameViewport = new Viewport(_engine, _gameCamera, _engine.Window.FramebufferSize)
		{
			ClearColor = Constants.GameColors.Background
		};
		_gameViewport.AddChild(_gameLoop);

		_gameViewport.Enabled = true;
		_mainMenu!.Enabled = false;

		_mainMenu.NewGameInstead(true);

		_currentState = GameState.Game;

		AddChild(_gameViewport);
		_gameViewport.Start();
	}

	public void ToGameState()
	{
		_currentState = GameState.Game;

		_gameViewport!.Enabled = true;
		_mainMenu!.Enabled = false;
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