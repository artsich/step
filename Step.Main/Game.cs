using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using Step.Main.Audio;
using Step.Main.Spawn;

/*
 * Goals:
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

public interface IGameScene
{
	Player Player { get; }

	void KillThings();

	int EffectsCount<T>() where T : IEffect;
}

public class Game : GameWindow, IGameScene
{
	ImGuiController _controller;

	private readonly Camera2d _camera = new(360, 180);

	private readonly List<Thing> _fallingThings = [];

	private Spawner _spawner;
	private Player _player;

	public Player Player => _player;
	private Texture2d _playerTexture;

	private readonly Queue<Action> _postUpdateActions = [];

	private bool _paused = false;

	private bool _showImGui = false;

	private int _score = 0;

	private float _thingsSpeed = 60f;
	private float _spawnTimeInterval = 1f;
	private System.Numerics.Vector2 _playerSize;
	private float _lastUpdateTime;
	private float _audioMasterVolume = 0.5f;

	private Texture2d _healthEffect;
	private Texture2d _bombEffect;
	private Texture2d _justThing;
	private Texture2d _speedEffect;
	private Renderer _renderer;

	public Game(
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
		//CenterWindow();
		_renderer = new Renderer();
		_renderer.Load();

		_renderer.SetBackground(new Color4<Rgba>(0.737f, 0.718f, 0.647f, 1.0f));

		_healthEffect = new Texture2d("Assets/Textures/effect_health.png").Load();
		_bombEffect = new Texture2d("Assets/Textures/effect_bomb.png").Load();
		_justThing = new Texture2d("Assets/Textures/thing.png").Load();
		_speedEffect = new Texture2d("Assets/Textures/effect_speed.png").Load();

		_player = new Player(
			new(0f, -75f),
			new(40f, 20f),
			KeyboardState,
			new Box2(-180f, -90f, 177f, 90f));
		_playerSize = new System.Numerics.Vector2(_player.Size.X, _player.Size.Y);

		_controller = new ImGuiController(ClientSize.X, ClientSize.Y)
		{
			FontGlobalScale = 2f
		};

		_spawner = new Spawner(
		[
			new(150f, 100f),
			new(110f, 105f),
			new(0f, 90f),
			new(-150f, 95f),
			new(-110f, 110f)
		],
		this,
		1f,
		[
			new SpawnSimpleEntity(_justThing),
			new SpanwHealthEntity(_healthEffect),
			new SpawnKillAllEntity(_bombEffect),
			new SpawnSpeedEntity(_speedEffect),
		]);

		AudioManager.Ins.LoadSound("start", "Assets/Music/ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("player_heal", "Assets/Music/player_heal.mp3");
		AudioManager.Ins.LoadSound("thing_taken", "Assets/Music/thing_taken.wav");
		AudioManager.Ins.LoadSound("kill_all", "Assets/Music/kill_all.mp3");
		AudioManager.Ins.LoadSound("player_take_damage", "Assets/Music/player_take_damage.mp3");
		AudioManager.Ins.LoadSound("main_theme", "Assets/Music/main_theme.mp3");
		AudioManager.Ins.LoadSound("player_dash", "Assets/Music/dash.wav");

		AudioManager.Ins.SetMasterVolume(_audioMasterVolume);

		AudioManager.Ins.PlaySound("start");
		AudioManager.Ins.PlaySound("main_theme", true);

		Player.OnPlayerHeal += () =>
		{
			AudioManager.Ins.PlaySound("player_heal");
		};

		Player.OnThingTaken += (_) =>
		{
			_score++;
			AudioManager.Ins.PlaySound("thing_taken");
		};

		Player.OnDamage += () =>
		{
			_camera.Shake(magnitude: 2f, duration: 1f);
			AudioManager.Ins.PlaySound("player_take_damage");
		};

		Player.OnDead += () =>
		{
			Console.WriteLine("Game over...");
			Close();
		};

		_playerTexture = new Texture2d("Assets/Textures/player.png").Load();
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);
		_controller.Update(this, (float)e.Time);

		GL.Clear(ClearBufferMask.ColorBufferBit);
		_renderer.SetCamera(_camera);

		Vector4 hpColor = new(0.9f, 0.4f, 0.35f, 1f);
		float hpScaleFactor = _player.Hp / (float)_player.MaxHp;
		hpColor *= hpScaleFactor;
		_renderer.DrawObject(_player.Position, _player.Size, (Color4<Rgba>)hpColor, _playerTexture);

		foreach(var thing in _fallingThings)
		{
			_renderer.DrawObject(thing.Position, thing.Size, Color4.White, thing.Texture);
		}

		if (_showImGui)
		{
			ImguiRender(e);
		}

		SwapBuffers();
	}

	private void ImguiRender(FrameEventArgs e)
	{
		if (ImGui.Begin("Main Window"))
		{
			if (ImGui.BeginTabBar("Main Tabs"))
			{
				if (ImGui.BeginTabItem("Game"))
				{
					ImGui.SeparatorText("Game info");
					ImGui.Text($"Score: {_score}");
					ImGui.Text($"Falling things: {_fallingThings.Count}");

					_player.DrawDebug();

					ImGui.SeparatorText("Spawner settings");
					ImGui.SliderFloat("Things speed", ref _thingsSpeed, 1f, 200f);
					ImGui.SliderFloat("Spawn time", ref _spawnTimeInterval, 0.01f, 1f);

					ImGui.SeparatorText("Performance");
					ImGui.Text($"Render time: {e.Time * 1000:F2}");
					ImGui.Text($"Update time: {_lastUpdateTime * 1000:F2}");

					if (ImGui.Button("Clear console"))
					{
						Console.Clear();
					}

					if (ImGui.Button("Damage"))
					{
						_player.Damage(1);
					}

					ImGui.EndTabItem();
				}

				if (ImGui.BeginTabItem("Audio Settings"))
				{
					ImGui.SliderFloat("Master volume", ref _audioMasterVolume, 0f, 1f);
					ImGui.EndTabItem();
				}

				ImGui.EndTabBar();
			}

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

		_camera.Update(dt);

		if (!_paused)
		{
			GameUpdate(dt);
		}

		while (_postUpdateActions.TryDequeue(out var action))
		{
			action();
		}
	}

	private void GameUpdate(float dt)
	{
		_spawner.Speed = _thingsSpeed;
		_spawner.TimeInterval = _spawnTimeInterval;

		_player.Resize(new (_playerSize.X, _playerSize.Y));
		_player.Update(dt);

		var spawnedThing = _spawner.Get(dt);
		if (spawnedThing is not null)
		{
			_fallingThings.Add(spawnedThing);
		}

		foreach (var thing in _fallingThings)
		{
			thing.Update(dt);
		}

		List<Thing> toRemove = [];
		var playerBox = _player.Box;
		foreach (var thing in _fallingThings)
		{
			if (thing.BoundingBox.Contains(playerBox))
			{
				_player.Take(thing);
				toRemove.Add(thing);
			}
			else if (thing.BoundingBox.Max.Y < -90f)
			{
				toRemove.Add(thing);
				Player.Damage(1);
			}
		}

		foreach (var thing in toRemove)
		{
			_fallingThings.Remove(thing);
		}
	}

	private void CheckWindowStateToggle(KeyboardState input)
	{
		if (input.IsKeyDown(Keys.LeftAlt))
		{
			if (input.IsKeyPressed(Keys.Enter))
			{
				_postUpdateActions.Enqueue(() =>
				{
					if (WindowState == WindowState.Fullscreen)
					{
						WindowState = WindowState.Normal;
					}
					else
					{
						WindowState = WindowState.Fullscreen;
					}
				});
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
		GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
		_controller.WindowResized(ClientSize.X, ClientSize.Y);
	}

	protected override void OnUnload()
	{
		AudioManager.Ins.Dispose();
		_renderer.Unload();
		base.OnUnload();
	}

	public void KillThings()
	{
		_postUpdateActions.Enqueue(() =>
		{
			_score += _fallingThings.Count;
			_fallingThings.Clear();
		});

		AudioManager.Ins.PlaySound("kill_all");
		_camera.Shake(magnitude: 5f, duration: 2f);
	}

	public int EffectsCount<T>() where T : IEffect
	{
		var fallingEffects = _fallingThings.Where(x => x.HasEffect<T>()).Count();
		return _player.EffectsCount<T>() + fallingEffects;
	}

	private void GameMouseWheel(MouseWheelEventArgs obj)
	{
		var scale = 0.1f;
		if (obj.OffsetY != 0f)
		{
			scale *= Math.Sign(obj.OffsetY);
			_camera.Zoom(scale);
		}
	}
}
