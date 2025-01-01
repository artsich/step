using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using Step.Main.Audio;
using Step.Main.Converters;
using Step.Main.Editor;
using Step.Main.Gameplay;
using Step.Main.Gameplay.Spawn;
using Step.Main.Graphics;
using Step.Main.Graphics.Particles;
using System.Text.Json;

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
 */

namespace Step.Main;

public class GameCompose : GameWindow
{
	ImGuiController _controller;

	private readonly float TargetAspectRatio = 16f / 9f;

	private bool _paused = false;
	private bool _showImGui = false;

	private float _lastUpdateTime;
	private float _audioMasterVolume = 0.1f;

	private Texture2d _healthEffect;
	private Texture2d _bombEffect;
	private Texture2d _justThing;
	private Texture2d _speedEffect;
	private Texture2d _playerTexture;
	private Renderer _renderer;

	private GameObject _root;
	private Camera2d _mainCamera;

	private RenderTarget2d _gameRenderTarget;

	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true,
		Converters =
		{
			new Vector2JsonConverter(),
			new Vector4JsonConverter()
		}
	};

	private readonly List<IEditorView> _editors = [];

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

		_healthEffect = new Texture2d(".\\Assets\\Textures\\effect_health.png").Load();
		_bombEffect = new Texture2d(".\\Assets\\Textures\\effect_bomb.png").Load();
		_justThing = new Texture2d(".\\Assets\\Textures\\thing.png").Load();
		_speedEffect = new Texture2d(".\\Assets\\Textures\\effect_speed.png").Load();
		_playerTexture = new Texture2d(".\\Assets\\Textures\\player.png").Load();


		var width = 320f;
		var height = (width * 9f) / 16f;
		var camera = new Camera2d(width, height);

		_controller = new ImGuiController(ClientSize.X, ClientSize.Y)
		{
			FontGlobalScale = 2f
		};

		AudioManager.Ins.LoadSound("start", ".\\Assets.\\Music\\ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("player_heal", ".\\Assets\\Music\\player_heal.mp3");
		AudioManager.Ins.LoadSound("thing_taken", ".\\Assets\\Music\\thing_taken.wav");
		AudioManager.Ins.LoadSound("kill_all", ".\\Assets\\Music\\kill_all.mp3");
		AudioManager.Ins.LoadSound("player_take_damage", ".\\Assets\\Music\\player_take_damage.mp3");
		AudioManager.Ins.LoadSound("main_theme", ".\\Assets\\Music\\main_theme.mp3");
		AudioManager.Ins.LoadSound("player_dash", ".\\Assets\\Music\\dash.wav");

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);

		AudioManager.Ins.PlaySound("start");
		AudioManager.Ins.PlaySound("main_theme", true);

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
				new SpawnSimpleEntity(_justThing, _renderer),
				new SpawnHealthEntity(_healthEffect, _renderer),
				new SpawnKillAllEntity(_bombEffect, _renderer),
				new SpawnSpeedEntity(_speedEffect, _renderer),
			]);

		var loadedEmitter = JsonSerializer.Deserialize<Emitter>(
			File.ReadAllText(".\\Assets\\Particles\\player_dash_particle.json"),
			_jsonOptions);

		loadedEmitter!.Material!.Texture = _playerTexture;
		var playerParticles = new Particles2d(loadedEmitter!, _renderer);

		var player = new Player(
			new(0f, -75f),
			new(40f, 20f),
			KeyboardState,
			new Box2(-width/2f, -height/2f, width/2f, height/2f),
			_playerTexture,
			_renderer);

		player.OnPlayerHeal += () =>
		{
			AudioManager.Ins.PlaySound("player_heal");
		};

		player.OnThingTaken += (_) =>
		{
			AudioManager.Ins.PlaySound("thing_taken");
		};

		player.OnDamage += () =>
		{
			camera.Shake(magnitude: 2f, duration: 0.5f);
			AudioManager.Ins.PlaySound("player_take_damage");
		};

		player.OnDead += () =>
		{
			Console.WriteLine("Game over...");
			Close();
		};

		player.AddChild(playerParticles);
		player.Start();

		_root = new Gameplay.Main(spawner, _renderer);
		_root.AddChild(player);
		_root.AddChild(camera);
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
			_renderer.SetCamera(_mainCamera);
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

			var imgSize = StepMath.AdjustToAspect(
				TargetAspectRatio,
				ImGui.GetContentRegionAvail().FromSystem())
			.ToSystem();

			ImGui.Image(_gameRenderTarget.Color, imgSize, new (0f, 1f), new (1f, 0f));

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

		var input = KeyboardState;

		if (input.IsKeyDown(Keys.Escape))
		{
			Close();
		}

		if (input.IsKeyPressed(Keys.P))
		{
			_paused = !_paused;
		}

		if (input.IsKeyPressed(Keys.GraveAccent))
		{
			_showImGui = !_showImGui;
		}

		CheckWindowStateToggle(input);

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

	private void CheckWindowStateToggle(KeyboardState input)
	{
		if (input.IsKeyDown(Keys.LeftAlt))
		{
			if (input.IsKeyPressed(Keys.Enter))
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

	protected override void OnUnload()
	{
		_healthEffect.Dispose();
		_bombEffect.Dispose();
		_justThing.Dispose();
		_speedEffect.Dispose();
		_playerTexture.Dispose();

		_renderer.Unload();

		AudioManager.Ins.Dispose();
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
