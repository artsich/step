using Silk.NET.Windowing;
using Step.Engine;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Main.Editor;
using Step.Main.Gameplay;
using Step.Main.Gameplay.Builders;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

const int TargetFps = 144;

var windowOptions = WindowOptions.Default with
{
	Title = "Frame",
	VSync = false,
	FramesPerSecond = TargetFps,
	UpdatesPerSecond = TargetFps,
	WindowBorder = WindowBorder.Fixed,
	WindowState = WindowState.Maximized,
	//WindowState = WindowState.Fullscreen,
	API = new GraphicsAPI(
		ContextAPI.OpenGL,
		ContextProfile.Core,
		ContextFlags.ForwardCompatible | ContextFlags.Debug,
		new APIVersion(4, 1))
	//PreferredDepthBufferBits = 8;
};

// do not create second renderer for editor
// use separate camara when in editor mode...

const float TargetAspectRatio = 16f / 9f;
const float InverseTargetAspectRatio = 1f / TargetAspectRatio;
const float GameCameraWidth = 320f;
const float GameCameraHeight = GameCameraWidth * InverseTargetAspectRatio;

//engine.AddEditor(new EffectsEditor(_crtEffect));
new Engine(windowOptions)
	.AddEditor((engine) => 
		new AssetsWindow(
			new ParticlesEditor(
				engine.Window.FramebufferSize, 
				new Camera2d(GameCameraWidth, GameCameraHeight)),
			new MapEditor(
				engine.Window.FramebufferSize, 
				new Camera2d(GameCameraWidth, GameCameraHeight))))
	.AddEditor((engine) => new GameControlsWindow(engine))
	.AddEditor((engine) => new AudioSettingsWindow(engine, 0.15f))
	.AddEditor((engine) => new EngineControlsWindow(engine))
	.AddEditor((engine) => new PerformanceWindow(engine))
	.AddEditor((engine) => new SceneWindow())
	.AddEditor((engine) => new GameRenderWindow(engine))
	.AddEditor((engine) => new DebugLogWindow())
	.Run((engine) =>
		new GameScene(
			engine,
			new GameBuilder(engine),
			GameCameraWidth, GameCameraHeight));
