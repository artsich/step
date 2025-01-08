using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Serilog;
using StbImageSharp;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Engine.Graphics.Particles;
using Step.Main.Gameplay;
using Step.Main.Gameplay.Spawn;

/*
 *  [Mechanics]
 *    Spawn temporal obstacle
 *    Temporal pistol
 *    Additional automatic obstacles, that can collect things
 *  [Graphics]
 *    Explosion effect
 *  [Progress system]
 *    Difficulty should be higher after some score
 *  [Tech]
 *    Make possible to play on different monitor size.
 *  [BUGS]
 *    When reload happens, not all events are unsubscribed, so memory leak
 *    https://github.com/aybe/DearImGui/blob/develop/DearImGui.OpenTK/Extensions/GameWindowBaseWithDebugContext.cs
 */

namespace Step.Main;

public class GameCompose : GameWindow
{
	private const float TargetAspectRatio = 16f / 9f;
	private const float InverseTargetAspectRatio = 1f / TargetAspectRatio;

	private const float GameCameraWidth = 320f;
	private const float GameCameraHeight = GameCameraWidth * InverseTargetAspectRatio;

	private bool _paused = false;
	private bool _showImGui = true;

	private float _lastUpdateTime;
	private float _audioMasterVolume = 0.1f;

	private Texture2d _healthEffect;
	private Texture2d _bombEffect;
	private Texture2d _justThing;
	private Texture2d _speedEffect;
	private Texture2d _playerTexture;
	private Texture2d _sizeChanger;
	private Texture2d _heroTextureAtlas;
	private Texture2d _heroSwordTexture;
	private Emitter _dashParticleEmitter;
	private Renderer _renderer;
	private ImGuiController _controller;
	private RenderTarget2d _gameRenderTarget;

	private readonly List<IEditorView> _editors = [];

	private Gameplay.Main _root;
	private Camera2d _mainCamera;
	private Emitter _wallCollisionParticleEmitter;

	public GameCompose(
		GameWindowSettings gameWindowSettings,
		NativeWindowSettings nativeWindowSettings)
		: base(gameWindowSettings, nativeWindowSettings)
	{
	}

	protected override void OnLoad()
	{
		base.OnLoad();
		MouseWheel += GameMouseWheel;

		StbImage.stbi_set_flip_vertically_on_load(1);

		_controller = new ImGuiController(
			ClientSize.X, ClientSize.Y,
			"Assets\\ProggyClean.ttf", 13.0f, this.GetDpi());

		_renderer = new Renderer(ClientSize.X, ClientSize.Y);
		_renderer.Load();

		_gameRenderTarget = new RenderTarget2d(ClientSize.X, ClientSize.Y, true);

		_renderer.SetBackground(new Color4<Rgba>(0.737f, 0.718f, 0.647f, 1.0f));
		LoadAssets();
		ReloadGame();

		_editors.Add(new ParticlesEditor(ClientSize, _mainCamera));
	}

	private void LoadAssets()
	{
		_healthEffect = Assets.LoadTexture2d("Textures\\effect_health.png");
		_bombEffect = Assets.LoadTexture2d("Textures\\effect_bomb.png");
		_justThing = Assets.LoadTexture2d("Textures\\thing.png");
		_speedEffect = Assets.LoadTexture2d("Textures\\effect_speed.png");
		_playerTexture = Assets.LoadTexture2d("Textures\\player.png");
		_sizeChanger = Assets.LoadTexture2d("Textures\\effect_size_increase.png");
		_heroTextureAtlas = Assets.LoadTexture2d("Textures\\HeroAtlas.png");
		_heroSwordTexture = Assets.LoadTexture2d("Textures\\Sword.png");

		_dashParticleEmitter = Assets.LoadEmitter("Particles\\player_dash_particle.json");
		_dashParticleEmitter!.Material!.Texture = _playerTexture;

		_wallCollisionParticleEmitter = Assets.LoadEmitter("Particles\\wall_collision.json");

		AudioManager.Ins.LoadSound("start", "Music\\ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("player_heal", "Music\\player_heal.mp3");
		AudioManager.Ins.LoadSound("thing_taken", "Music\\thing_taken.wav");
		AudioManager.Ins.LoadSound("kill_all", "Music\\kill_all.mp3");
		AudioManager.Ins.LoadSound("player_take_damage", "Music\\player_take_damage.mp3");
		AudioManager.Ins.LoadSound("main_theme", "Music\\main_theme.mp3");
		AudioManager.Ins.LoadSound("player_dash", "Music\\dash.wav");
		AudioManager.Ins.LoadSound("wall_collision", "Music\\wall_collision.mp3");

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);
	}

	private void ReloadGame()
	{
		var width = GameCameraWidth;
		var height = GameCameraHeight;
		var camera = new Camera2d(width, height);

		var sword = new GameObject("Sword")
		{
			LocalTransform = new Transform()
			{
				Position = new(-3.6f, 8f)
			}
		};
		sword.AddChild(new Sprite2d(_renderer, _heroSwordTexture)
		{
			LocalTransform = new Transform()
			{
				Scale = new(16f)
			}
		});

		var player = new Player(
			new(0f, -60f),
			//new(40f, 20f),
			new(10f, 32f),
			KeyboardState,
			new Box2(-width / 2f, -height / 2f, width / 2f, height / 2f),
			_playerTexture,
			_renderer);

		player.AddChild(new Particles2d(_dashParticleEmitter!, _renderer)
		{
			Name = "DashParticles",
		});
		player.AddChild(new Particles2d(_wallCollisionParticleEmitter!, _renderer)
		{
			Name = "WallCollisionParticles",
		});
		player.AddChild(sword);

		var idleFrames = new SpriteFrames("idle", true, 5f, _heroTextureAtlas,
		[
			new Rect(0, 64, 32, 32),
			new Rect(32, 64, 32, 32),
		]);

		var walkFrames = new SpriteFrames("walk", true, 5f, _heroTextureAtlas,
		[
			new Rect(0, 128, 32, 32),
			new Rect(32, 128, 32, 32),
			new Rect(64, 128, 32, 32),
			new Rect(96, 128, 32, 32),
		]);

		var dashFrames = new SpriteFrames("dash", false, 5f, _heroTextureAtlas,
		[
			new Rect(0, 320, 32, 32),
			new Rect(32, 320, 32, 32),
			new Rect(64, 320, 32, 32),
		]);

		player.AddChild(new AnimatedSprite2d(_renderer, [walkFrames, idleFrames, dashFrames]) { Name = "Animations" });

		var spawner = new Spawner(
			[
				new(140f, 100f),
				new(110f, 105f),
				new(0f, 90f),
				new(-140f, 95f),
				new(-110f, 110f)
			],
			1f,
			[
				new SpawnSimpleEntity(_justThing, _renderer, true),
				new SpawnSimpleEntity(_justThing, _renderer, false),
				new SpawnSizeChanger(_sizeChanger, _renderer),
				new SpawnHealthEntity(_healthEffect, _renderer),
				new SpawnKillAllEntity(_bombEffect, _renderer),
				new SpawnSpeedEntity(_speedEffect, _renderer),
			])
		{
			Enabled = false
		};

		_root = new Gameplay.Main(spawner, _renderer);
		_root.AddChild(player);
		_root.AddChild(camera);

		_root.OnFinish += () =>
		{
			Console.Clear();
			Log.Logger.Information("Reloading...");
			ReloadGame();
		};

		_root.Start();

		_mainCamera = _root.GetChildOf<Camera2d>();
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		_renderer.PushRenderTarget(_gameRenderTarget);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		_root.Draw();
		_renderer.Flush();
		_renderer.PopRenderTarget();

		if (_showImGui)
		{
			ImGuiRender(e);
		}
		else
		{
			_renderer.DrawScreenRectNow(_gameRenderTarget.Color);
		}

		SwapBuffers();
	}

	private void ImGuiRender(FrameEventArgs e)
	{
		_controller.Update(this, (float)e.Time);

		ImGui.DockSpaceOverViewport();

		if (ImGui.Begin("Some"))
		{
			if (ImGui.Button("Clear console"))
			{
				Console.Clear();
			}

			if (ImGui.Button("Game & Assets reload"))
			{
				UnloadAssets();
				LoadAssets();

				//UnloadGame();
				ReloadGame();
			}

			if (ImGui.Button(_paused ? "Paused" : "Un pause"))
			{
				_paused = !_paused;
			}

			ImGui.End();
		}

		if (ImGui.Begin("Audio Settings"))
		{
			ImGui.SliderFloat("Master volume", ref _audioMasterVolume, 0f, 1f);
			ImGui.End();
		}

		if (ImGui.Begin("Assets"))
		{
			if (ImGui.BeginTabBar("Main Tabs"))
			{
				foreach (var editor in _editors)
				{
					if (ImGui.BeginTabItem(editor.Name))
					{
						editor.Draw();
						ImGui.EndTabItem();
					}
				}
				ImGui.EndTabBar();
			}
			ImGui.End();
		}

		if (ImGui.Begin("Performance"))
		{
			var ms = e.Time * 1000;
			var fps = 1000 / ms;
			ImGui.Text($"Render time: {ms:F2}ms | {fps:F2}fps");
			ImGui.Text($"Update time: {_lastUpdateTime * 1000:F2}ms");

			ImGui.End();
		}

		ImGui.ShowDebugLogWindow();

		if (ImGui.Begin("Game render", ImGuiWindowFlags.NoScrollbar))
		{
			var imgSize = StepMath
				.AdjustToAspect(
					TargetAspectRatio,
					ImGui.GetContentRegionAvail().FromSystem())
				.ToSystem();

			ImGui.Image(_gameRenderTarget.Color.Handle, imgSize, new(0f, 1f), new(1f, 0f));

			ImGui.End();
		}

		if (ImGui.Begin("Scene"))
		{
			_root.DebugDraw();
			ImGui.End();
		}

		_controller.Render();
		ImGuiController.CheckGLError("End of frame");
	}

	protected override void OnUpdateFrame(FrameEventArgs e)
	{
		base.OnUpdateFrame(e);
		float dt = (float)e.Time;
		_lastUpdateTime = dt;

		if (KeyboardState.IsKeyDown(Keys.Escape))
		{
			Close();
		}

		if (KeyboardState.IsKeyPressed(Keys.P))
		{
			_paused = !_paused;
		}

		if (KeyboardState.IsKeyPressed(Keys.GraveAccent))
		{
			_showImGui = !_showImGui;
		}

		CheckWindowStateToggle();

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);

		if (_showImGui)
		{
			foreach (var editor in _editors)
			{
				editor.Update(dt);
			}
		}

		if (!_paused)
		{
			_root.Update(dt);
		}
	}

	private void CheckWindowStateToggle()
	{
		if (KeyboardState.IsKeyDown(Keys.LeftAlt))
		{
			if (KeyboardState.IsKeyPressed(Keys.Enter))
			{
				if (WindowState == WindowState.Fullscreen)
				{
					WindowState = WindowState.Normal;
				}
				else
				{
					WindowState = WindowState.Fullscreen;
				}
			}
		}
	}

	protected override void OnTextInput(TextInputEventArgs e)
	{
		base.OnTextInput(e);

		if (_showImGui)
		{
			_controller.PressChar((char)e.Unicode);
		}
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);
		if (_showImGui)
		{
			_controller.MouseScroll(e.Offset);
		}
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);
		GL.Viewport(0, 0, e.Width, e.Height);
		_controller.WindowResized(e.Width, e.Height);
	}

	private void UnloadAssets()
	{
		_healthEffect.Dispose();
		_bombEffect.Dispose();
		_justThing.Dispose();
		_speedEffect.Dispose();
		_playerTexture.Dispose();
		AudioManager.Ins.UnloadSounds();
	}

	protected override void OnUnload()
	{
		UnloadAssets();

		AudioManager.Ins.Dispose();
		_renderer.Unload();
		base.OnUnload();
	}

	private void GameMouseWheel(MouseWheelEventArgs obj)
	{
		var scale = 0.1f;
		if (obj.OffsetY != 0f)
		{
			scale *= Math.Sign(obj.OffsetY);
			_mainCamera.Zoom(scale);
		}
	}
}
