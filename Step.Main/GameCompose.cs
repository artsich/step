using Step.Engine;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Main.Gameplay;
using Step.Main.Gameplay.Actors;

namespace Step.Main;

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

// todo:
// shield is visible on after game reload
// do not create second renderer for editor
// use separate camara when in editor mode...
public class GameCompose : IGame
{
	#region CameraSettings
	private const float TargetAspectRatio = 16f / 9f;
	private const float InverseTargetAspectRatio = 1f / TargetAspectRatio;

	private const float GameCameraWidth = 320f;
	private const float GameCameraHeight = GameCameraWidth * InverseTargetAspectRatio;
	#endregion

	private RenderTarget2d _gameRenderTarget;
	private Renderer _renderer;
	private CrtEffect _crtEffect;

	private Engine.Engine _engine;
	private GameStateManager _stateManager;

	public void Load(Engine.Engine engine)
	{
		_engine = engine;
		_renderer = engine.Renderer;

		var screenSize = engine.Window.FramebufferSize;
		_gameRenderTarget = new RenderTarget2d(screenSize.X, screenSize.Y, true);

		_crtEffect = new CrtEffect(
			new Shader(
				"Assets/Shaders/CRT/shader.vert",
				"Assets/Shaders/CRT/shader.frag"
			),
			new RenderTarget2d(screenSize.X, screenSize.Y, true),
			engine.Renderer
		);

		engine.AddEditor(new ParticlesEditor(screenSize, new Camera2d(GameCameraWidth, GameCameraHeight)));
		engine.AddEditor(new EffectsEditor(_crtEffect));

		_stateManager = new GameStateManager(engine, GameCameraWidth, GameCameraHeight);
	}

	public Texture2d Render(float dt)
	{
		_renderer.PushRenderTarget(_gameRenderTarget);
		_gameRenderTarget.Clear(GameColors.Background);
		GameRoot.I.Draw();
		_renderer.Flush();
		_renderer.PopRenderTarget();

		//return _gameRenderTarget.Color;
		return PostProcessing();
	}

	private Texture2d PostProcessing()
	{
		if (_stateManager.GetCurrentState() == GameState.Game)
		{
			var player = GameRoot.I.Scene.GetChildOf<Player>();
			var camera = GameRoot.I.Scene.GetChildOf<Camera2d>();

			if (player != null && camera != null)
			{
				_crtEffect.VignetteTarget = camera.ToClipSpace(player.GlobalPosition);
			}
			else
			{
				_crtEffect.VignetteTarget = new(0.5f);
			}
		}
		else
		{
			_crtEffect.VignetteTarget = new(0.5f);
		}

		_crtEffect.Apply(_gameRenderTarget.Color, out var _finalImage);
		return _finalImage;
	}

	public void Unload()
	{
		_stateManager.Unload();
		_crtEffect.Dispose();
		_gameRenderTarget.Dispose();
		_engine.ClearEditors();
	}
}
