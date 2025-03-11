using ImGuiNET;
using Serilog;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Graphics;
using Step.Main.Gameplay;
using Step.Main.Gameplay.Actors;
using Silk.NET.Maths;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Step.Main;

public interface IGameWindow
{
	Vector2f Size { get; }
}

public enum PhysicLayers : int
{
	Player = 1 << 0,
	Enemy = 1 << 1,
	Magnet = 1 << 2,
	Frame = 1 << 3,
	Shield = 1 << 4,
}

// todo: shield is visible on after game reload.....
public class GameCompose : IGameWindow, IGame
{
	#region CameraSettings
	private const float TargetAspectRatio = 16f / 9f;
	private const float InverseTargetAspectRatio = 1f / TargetAspectRatio;

	private const float GameCameraWidth = 320f;
	private const float GameCameraHeight = GameCameraWidth * InverseTargetAspectRatio;
	#endregion

	#region Assets
	private Texture2d _gliderTexture;
	private Texture2d _circleTexture;
	private Texture2d _playerTexture;
	private Texture2d _crossTexture;
	private CrtEffect _crtEffect;
	#endregion

	private RenderTarget2d _gameRenderTarget;
	private Renderer _renderer;
	private Texture2d _finalImage;
	private IWindow _window;

	Vector2f IGameWindow.Size => (Vector2f)_window.FramebufferSize;

	Engine.Engine _engine;

	public void Load(Engine.Engine engine)
	{
		_engine = engine;
		_renderer = engine.Renderer;
		_window = engine.Window;
		engine.Mouse.Scroll += GameMouseWheel;

		var screenSize = engine.Window.FramebufferSize;
		_gameRenderTarget = new RenderTarget2d(screenSize.X, screenSize.Y, true);

		_crtEffect = new CrtEffect(
			new Shader(
				".\\Assets\\Shaders\\CRT\\shader.vert",
				".\\Assets\\Shaders\\CRT\\shader.frag"
			),
			new RenderTarget2d(screenSize.X, screenSize.Y, true),
			engine.Renderer
		);

		LoadAssets();
		ReloadGame();

		engine.AddEditor(new ParticlesEditor(screenSize, GameRoot.I.Scene.GetChildOf<Camera2d>()));
		engine.AddEditor(new EffectsEditor(_crtEffect));
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
	}

	private void ReloadGame()
	{
		var width = GameCameraWidth;
		var height = GameCameraHeight;
		var camera = new Camera2d(width, height);

		var root = new Gameplay.Main(_renderer);
		root.AddChild(camera);

		var player = new Player(
			_engine.Input,
			new RectangleShape2d(_renderer)
			{
				Size = new Vector2f(16f),
				CollisionLayers = (int)PhysicLayers.Player,
				CollisionMask = (int)(PhysicLayers.Enemy | PhysicLayers.Frame)
			});

		var playerSprite = new PlayerSprite();
		playerSprite.AddChild(new Sprite2d(_renderer, _playerTexture)
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

		var enemyFactory = new EnemyFactory(
			_renderer,
			_gliderTexture,
			_circleTexture,
			_crossTexture,
			player);

		var spawner = new Spawner(new Box2f(-width / 2f, -height / 2f, width / 2f, height / 2f),
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

		var frame = new Frame(_renderer);

		root.AddChild(frame);
		root.AddChild(player);
		root.AddChild(spawner);

		root.OnFinish += () =>
		{
			Console.Clear();
			Log.Logger.Information("Reloading...");
			ReloadGame();
		};

		GameRoot.I.SetScene(root);
		GameRoot.I.CurrentCamera = camera;
	}

	public void Render(float dt)
	{
		_renderer.PushRenderTarget(_gameRenderTarget);
		_gameRenderTarget.Clear(GameColors.Background);
		GameRoot.I.Draw();
		_renderer.Flush();
		_renderer.PopRenderTarget();

		PostProcessing();

		_renderer.DrawScreenRectNow(_finalImage);
	}

	private void PostProcessing()
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

		_crtEffect.Apply(_gameRenderTarget.Color, out _finalImage);
	}

	public void ImGuiRender(float dt)
	{
		if (ImGui.Begin("Game controls"))
		{
			if (ImGui.Button("Game & Assets reload"))
			{
				UnloadAssets();
				LoadAssets();

				//UnloadGame();
				ReloadGame();
			}

			ImGui.End();
		}

		if (ImGui.Begin("Game render", ImGuiWindowFlags.NoScrollbar))
		{
			var availRegion = ImGui.GetContentRegionAvail().FromSystem();
			var imgSize = StepMath
				.AdjustToAspect(
					TargetAspectRatio,
					availRegion)
				.ToSystem();

			var headerOffset = new Vector2f(
				(ImGui.GetWindowSize().X - availRegion.X) / 2f,
				ImGui.GetWindowSize().Y - availRegion.Y);

			ImGui.Image((nint)_finalImage.Handle, imgSize, new(0f, 1f), new(1f, 0f));

			var windowPos = ImGui.GetWindowPos().FromSystem();
			_engine.Input.SetMouseOffset(windowPos + headerOffset);
			_engine.Input.SetWindowSize(imgSize.FromSystem());
			ImGui.End();
		}

		if (ImGui.Begin("Scene"))
		{
			GameRoot.I.DebugDraw();
			ImGui.End();
		}
	}

	public void Update(float dt)
	{
		GameRoot.I.Update(dt);
	}

	private void UnloadAssets()
	{
		AudioManager.Ins.UnloadSounds();
	}

	public void Unload()
	{
		UnloadAssets();

		AudioManager.Ins.Dispose();
		_renderer.Unload();
	}

	private void GameMouseWheel(IMouse _, ScrollWheel scroll)
	{
		var scale = 0.1f;
		if (scroll.Y != 0f)
		{
			scale *= Math.Sign(scroll.Y);
			GameRoot.I.Scene.GetChildOf<Camera2d>().Zoom(scale);
		}
	}
}
