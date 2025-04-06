using Step.Engine;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Engine.Graphics.PostProcessing;
using Step.Main.Gameplay;

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

	private Engine.Engine _engine;
	private GameScene _gameScene;

	private CrtEffect _crtEffect;
	private BlurEffect _blurEffect;

	public void Load(Engine.Engine engine)
	{
		_engine = engine;
		_blurEffect = new();

		var screenSize = engine.Window.FramebufferSize;

		_crtEffect = new CrtEffect(
			new RenderTarget2d(screenSize.X, screenSize.Y, true),
			engine.Renderer);

#if DEBUG
		engine.AddEditor(new ParticlesEditor(screenSize, new Camera2d(GameCameraWidth, GameCameraHeight)));
		engine.AddEditor(new EffectsEditor(_crtEffect));
#endif
		_gameScene = new GameScene(engine, GameCameraWidth, GameCameraHeight);
		GameRoot.I.SetScene(_gameScene);
	}

	public Texture2d Render(float dt)
	{
		_gameScene.MainViewport.Draw();
		return PostProcessing(_gameScene.MainViewport.ColorTexture);
	}

	private Texture2d PostProcessing(Texture2d renderResult)
	{
		//if (_stateManager.GetCurrentState() == GameState.Game)
		//{
		//	var player = GameRoot.I.Scene.GetChildOf<Player>();
		//	var camera = GameRoot.I.Scene.GetChildOf<Camera2d>();

		//	if (player != null && camera != null)
		//	{
		//		_crtEffect.VignetteTarget = camera.ToClipSpace(player.GlobalPosition);
		//	}
		//	else
		//	{
		//		_crtEffect.VignetteTarget = new(0.5f);
		//	}
		//}
		//else
		{
			_crtEffect.VignetteTarget = new(0.5f);
		}

		_crtEffect.Apply(renderResult, out var finalImage);
		_blurEffect.Apply(finalImage, out var blurred);
		return blurred;
	}

	public void Unload()
	{
		_gameScene.Unload();
		_crtEffect.Dispose();
		_engine.ClearEditors();
	}
}
