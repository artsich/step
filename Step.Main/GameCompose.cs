using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Engine.Graphics.Particles;
using Step.Main.Gameplay;
using Step.Main.Gameplay.Spawn;

/*
 * Goals:
 * Enemies, friend
 *  Effects
 *    - speed
 *		- debuff
 *    - split platform on two but smaller size and move it simultaneously
 *  Render player Stats
 *  - inventory contains available effects of the user.
 *  - current health
 *
 *  Additional automatic platforms, that can collect things
 *  Guns - pistol, knife - additional effects...
 *  
 *  [BUGS]
 *  When reload happens, not all events are unsubscribed, so memory leak
 */

namespace Step.Main;

public class GameCompose : GameWindow
{
	private readonly float TargetAspectRatio = 16f / 9f;

	private bool _paused = false;
	private bool _showImGui = true;

	private float _lastUpdateTime;
	private float _audioMasterVolume = 0.1f;

	private Texture2d _healthEffect;
	private Texture2d _bombEffect;
	private Texture2d _justThing;
	private Texture2d _speedEffect;
	private Texture2d _playerTexture;
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
		_renderer = new Renderer(ClientSize.X, ClientSize.Y);
		_renderer.Load();

		_gameRenderTarget = new RenderTarget2d(ClientSize.X, ClientSize.Y, true);

		_editors.Add(new ParticlesEditor(_renderer));

		_renderer.SetBackground(new Color4<Rgba>(0.737f, 0.718f, 0.647f, 1.0f));

		LoadAssets();

		_controller = new ImGuiController(ClientSize.X, ClientSize.Y)
		{
			FontGlobalScale = 2f
		};

		ReloadGame();
	}

	private void LoadAssets()
	{
		_healthEffect = Assets.LoadTexture2d("Textures\\effect_health.png");
		_bombEffect = Assets.LoadTexture2d("Textures\\effect_bomb.png");
		_justThing = Assets.LoadTexture2d("Textures\\thing.png");
		_speedEffect = Assets.LoadTexture2d("Textures\\effect_speed.png");
		_playerTexture = Assets.LoadTexture2d("Textures\\player.png");

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
		var width = 320f;
		var height = (width * 9f) / 16f;
		var camera = new Camera2d(width, height);

		var player = new Player(
			new(0f, -75f),
			new(40f, 20f),
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
			Name = "WallCollision",
		});

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
			Console.WriteLine("Reloading...");
			ReloadGame();
		};

		_root.Start();

		_mainCamera = _root.GetChildOf<Camera2d>();
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);
		_controller.Update(this, (float)e.Time);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		if (_showImGui)
		{
			ImGuiRender(e);
		}
		else
		{
			_root.Draw();
		}

		SwapBuffers();
	}

	private void ImGuiRender(FrameEventArgs e)
	{
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
			ImGui.Text($"Render time: {e.Time * 1000:F2}ms");
			ImGui.Text($"Update time: {_lastUpdateTime * 1000:F2}ms");

			ImGui.End();
		}

		if (ImGui.Begin("Game render", ImGuiWindowFlags.NoScrollbar))
		{
			_renderer.PushRenderTarget(_gameRenderTarget);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_root.Draw();
			_renderer.PopRenderTarget();

			var imgSize = StepMath
				.AdjustToAspect(
					TargetAspectRatio,
					ImGui.GetContentRegionAvail().FromSystem())
				.ToSystem();

			ImGui.Image(_gameRenderTarget.Color, imgSize, new(0f, 1f), new(1f, 0f));

			ImGui.End();
		}

		if (ImGui.Begin("Gameplay"))
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
		AudioManager.Ins.Dispose();
	}

	protected override void OnUnload()
	{
		UnloadAssets();

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
