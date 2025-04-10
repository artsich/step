using Serilog;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Engine.Graphics.PostProcessing;
using Step.Main.Gameplay.Builders;
using Step.Main.Gameplay.UI;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Step.Main.Gameplay;

public enum PhysicLayers : int
{
	Player = 1 << 0,
	Enemy = 1 << 1,
	Magnet = 1 << 2,
	Frame = 1 << 3,
	Shield = 1 << 4,
}

public enum GameState
{
	Game,
	Start,
	Paused,
	Exit,
}

public class GameScene : RenderResult
{
	private GameState _currentState = GameState.Start;
	private readonly Engine.Engine _engine;

	private MainMenu? _mainMenu;
	private GameLoop? _gameLoop;
	private Viewport? _uiViewport;
	private Viewport? _gameViewport;

	private readonly float _cameraWidth;
	private readonly float _cameraHeight;
	private readonly Camera2d _menuCamera;
	private readonly Camera2d _gameCamera;
	private readonly BlendPostEffect _blendPostEffect;
	private readonly CrtEffect _crtEffect;
	private readonly BlurEffect _blurEffect;

	public override Texture2d ResultTexture
	{
		get
		{
			Texture2d? finalImage;
			if (_currentState == GameState.Game)
			{
				Debug.Assert(_gameViewport != null);
				_crtEffect.Apply(_gameViewport!.ColorTexture, out finalImage);
			}
			else
			{
				Debug.Assert(_uiViewport != null);
				finalImage = _uiViewport!.ColorTexture;

				if (_gameViewport != null)
				{
					_blurEffect.Apply(_gameViewport!.ColorTexture, out var blurredGame);
					_blendPostEffect.Blend(
						blurredGame,
						_uiViewport!.ColorTexture, out finalImage);
				}

				_crtEffect.Apply(finalImage, out finalImage);
			}

			return finalImage;
		}
	}

	public GameScene(Engine.Engine engine, float cameraWidth, float cameraHeight)
		: base("GameRoot")
	{
		_engine = engine;

		_cameraWidth = cameraWidth;
		_cameraHeight = cameraHeight;

		_menuCamera = new Camera2d(cameraWidth, cameraHeight);
		_gameCamera = new Camera2d(cameraWidth, cameraHeight);

		_engine.Keyboard.KeyDown += HandleKeyDown;

		_crtEffect = new CrtEffect(
			engine.Window.FramebufferSize,
			engine.Renderer);
		_blurEffect = new BlurEffect();
		_blendPostEffect = new BlendPostEffect();

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
				PlayNewGame();
			});
		};

		_mainMenu.OnContinuePressed += () =>
		{
			_mainMenu.CallDeferred(() =>
			{
				ResumeGame();
			});
		};

		_mainMenu.OnExitPressed += () =>
		{
			_mainMenu.CallDeferred(() =>
			{
				ToExitState();
			});
		};

		_uiViewport = new Viewport(_engine, _menuCamera, _engine.Window.FramebufferSize)
		{
			Name = "UI viewport"
		};
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
				case GameState.Paused:
					ResumeGame();
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
		_uiViewport!.Enabled = true;
	}

	public void PlayNewGame()
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
			Log.Logger.Information("Reload game");
		}

		_gameLoop = new GameBuilder(_engine, _cameraWidth, _cameraHeight).Build();
		_gameLoop.AddChild(_gameCamera);
		_gameLoop.OnFinish += OnGameFinish;

		_gameViewport = new Viewport(_engine, _gameCamera, _engine.Window.FramebufferSize)
		{
			ClearColor = Constants.GameColors.Background,
			Name = "Game viewport"
		};
		_gameViewport.AddChild(_gameLoop);

		_uiViewport!.Enabled = false;

		_mainMenu!.NewGameInstead(true);

		_currentState = GameState.Game;

		AddChild(_gameViewport);
		_gameViewport.Start();
	}

	public void ResumeGame()
	{
		_currentState = GameState.Game;

		_gameViewport!.Enabled = true;
		_uiViewport!.Enabled = false;
	}

	public void ToExitState()
	{
		_currentState = GameState.Exit;
		_engine.Window.Close();
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_engine.Keyboard.KeyDown -= HandleKeyDown;
		AudioManager.Ins.UnloadSounds();

		_blurEffect.Dispose();
		_crtEffect.Dispose();
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}