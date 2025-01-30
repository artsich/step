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
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Main.Gameplay;
using Step.Main.Gameplay.Actors;

namespace Step.Main;

public interface IGameWindow
{
	Vector2 Size { get; }
}

public enum PhysicLayers : int
{
	Player	= 1 << 0,
	Enemy	= 1 << 1,
	Magnet  = 1 << 2,
}

public class GameCompose : GameWindow, IGameWindow
{
	private const float TargetAspectRatio = 16f / 9f;
	private const float InverseTargetAspectRatio = 1f / TargetAspectRatio;

	private const float GameCameraWidth = 320f;
	private const float GameCameraHeight = GameCameraWidth * InverseTargetAspectRatio;

	private bool _paused = false;
	private bool _showImGui = true;

	private float _lastUpdateTime;
	private float _audioMasterVolume = 0.15f;

	private Texture2d _gliderTexture;
	private Texture2d _circleTexture;
	private Texture2d _playerTexture;
	private Texture2d _crossTexture;
	private Renderer _renderer;
	private ImGuiController _controller;
	private RenderTarget2d _gameRenderTarget;

	private Texture2d _finalImage;

	private readonly List<IEditorView> _editors = [];

	private Camera2d _mainCamera;

	private Vector2 _currentWindowSize;

	private Input _input;
	private CrtEffect _crtEffect;

	Vector2 IGameWindow.Size => _currentWindowSize;

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

		_crtEffect = new CrtEffect(
			new Shader(
				".\\Assets\\Shaders\\CRT\\shader.vert",
				".\\Assets\\Shaders\\CRT\\shader.frag"
			),
			new RenderTarget2d(ClientSize.X, ClientSize.Y, true),
			_renderer
		);

		_renderer.SetBackground(new Color4<Rgba>(0.737f, 0.718f, 0.647f, 1.0f));
		LoadAssets();
		ReloadGame();

		_editors.Add(new ParticlesEditor(ClientSize, _mainCamera));
		_currentWindowSize = ClientSize;
	}

	private void LoadAssets()
	{
		AudioManager.Ins.LoadSound("start", "Music\\ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("main_theme", "Music\\air-ambience-234180.mp3");
		AudioManager.Ins.LoadSound("player_hurt_glider", "Music\\hurt.wav");
		AudioManager.Ins.LoadSound("player_hurt_circle", "Music\\hurt2.wav");
		AudioManager.Ins.LoadSound("player_pickup", "Music\\pickup.wav");

		AudioManager.Ins.LoadSound("wall_collision", "Music\\wall_collision.mp3");

		_gliderTexture = Assets.LoadTexture2d("Textures\\glider-enemy.png");
		_circleTexture = Assets.LoadTexture2d("Textures\\circle-enemy.png");
		_playerTexture = Assets.LoadTexture2d("Textures\\player.png");
		_crossTexture = Assets.LoadTexture2d("Textures\\cross-enemy.png");

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);
	}

	private void ReloadGame()
	{
		var width = GameCameraWidth;
		var height = GameCameraHeight;
		var camera = new Camera2d(width, height, this);

		_input = new Input(MouseState, camera);

		var root = new Gameplay.Main(_renderer);
		root.AddChild(camera);

		var player = new Player(_input);
		player.AddChild(new RectangleShape2d(_renderer)
		{
			Size = new Vector2(16f),
			CollisionLayers = (int)PhysicLayers.Player,
			CollisionMask = (int)PhysicLayers.Enemy
		});
		var playerSprite = new PlayerSprite();
		playerSprite.AddChild(new Sprite2d(_renderer, _playerTexture)
		{
			Name = "Frame",
			Color = Colors.Player,
		});

		playerSprite.AddChild(new Sprite2d(_renderer, _renderer.DefaultWhiteTexture)
		{
			Name = "Health",
			Color = Color4.Lightgreen,
			Pivot = new Vector2(0f),
			LocalTransform = new Transform()
			{
				Position = new Vector2(-8f, -8f),
				Scale = new Vector2(16f),
			}
		});

		player.AddChild(playerSprite);

		player.AddAbility(new SpeedIncreaseAbility(player));
		player.AddAbility(new RegenerationAbility(player));
		player.AddAbility(new SizeChangerAbility(player) { Duration = 3f });
		player.AddAbility(new TimeFreezeAbility() { Duration = 2f });
		player.AddAbility(new MagnetAbility(50f, player, _renderer));

		var enemyFactory = new EnemyFactory(
			_renderer,
			_gliderTexture,
			_circleTexture,
			_crossTexture,
			player);

		var spawner = new Spawner(new Box2(-width / 2f, -height / 2f, width / 2f, height / 2f),
			[
				new SpawnRule
				{
					StartTime = 0f,
					SpawnProbability = 1f,
					CreateEntity = enemyFactory.CreateCircle
				},
				new SpawnRule
				{
					StartTime = 60f,
					SpawnProbability = 0.05f,
					CreateEntity = enemyFactory.CreateGlider
				},
				new SpawnRule
				{
					StartTime = 30f,
					SpawnProbability = 0.1f,
					CreateEntity = enemyFactory.CreateCross,
					SpawnLocation = SpawnLocationType.Interior,
				}
			]);

		root.AddChild(player);
		root.AddChild(spawner);

		root.OnFinish += () =>
		{
			Console.Clear();
			Log.Logger.Information("Reloading...");
			ReloadGame();
		};

		GameRoot.I.SetScene(root);
		_mainCamera = root.GetChildOf<Camera2d>();
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		_renderer.PushRenderTarget(_gameRenderTarget);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		GameRoot.I.Draw();

		_renderer.Flush();
		_renderer.PopRenderTarget();

		var player = GameRoot.I.Scene.GetChildOf<Player>();
		var camera = GameRoot.I.Scene.GetChildOf<Camera2d>();

		if (player != null && camera != null)
		{
			Vector2 playerWorldPos = player.GlobalPosition;

			Vector4 clipSpace = new Vector4(playerWorldPos.X, playerWorldPos.Y, 0, 1) * camera.ViewProj;
			var pos = new Vector2(
				(clipSpace.X / clipSpace.W + 1.0f) * 0.5f,
				(clipSpace.Y / clipSpace.W + 1.0f) * 0.5f);

			_crtEffect.VignetteTarget = pos;
		}
		else
		{
			_crtEffect.VignetteTarget = new Vector2(0.5f);
		}

		_crtEffect.Apply(_gameRenderTarget.Color, out _finalImage);

		if (_showImGui)
		{
			ImGuiRender(e);
		}
		else
		{
			_renderer.DrawScreenRectNow(_finalImage);
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
				

				if (ImGui.BeginTabItem("Shaders"))
				{
					_crtEffect.DebugDraw();
					ImGui.EndTabItem();
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

			ImGui.Separator();
			ImGui.Text($"Collision shapes: {CollisionSystem.Ins.Count}");

			ImGui.Separator();
			ImGui.Text($"Draw time: {_renderer.Stats.GpuTimeMs:F5} ms");

			ImGui.End();
		}

		ImGui.ShowDebugLogWindow();

		if (ImGui.Begin("Game render", ImGuiWindowFlags.NoScrollbar))
		{
			var availRegion = ImGui.GetContentRegionAvail().FromSystem();
			var imgSize = StepMath
				.AdjustToAspect(
					TargetAspectRatio,
					availRegion)
				.ToSystem();

			var headerOffset = new Vector2(
				(ImGui.GetWindowSize().X - availRegion.X) / 2f,
				ImGui.GetWindowSize().Y - availRegion.Y);
			var windowPos = ImGui.GetWindowPos().FromSystem();
			_input.SetMouseOffset(windowPos + headerOffset);

			ImGui.Image(_finalImage.Handle, imgSize, new(0f, 1f), new(1f, 0f));
			
			_currentWindowSize = imgSize.FromSystem();
			ImGui.End();
		}

		if (ImGui.Begin("Scene"))
		{
			GameRoot.I.DebugDraw();
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
		else
		{
			_currentWindowSize = ClientSize;
			_input.SetMouseOffset(Vector2.Zero);
		}

		if (!_paused)
		{
			_input.Update(dt);
			GameRoot.I.Update(dt);
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
