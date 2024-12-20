using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using Step.Main.Audio;

/*
 * Goals:
 *  Add score
 *  Effects
 *    - speed
 *		- buf
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
}

public class Game : GameWindow, IGameScene
{
	private readonly float[] _rectVertices =
	{
		// Position          // Texture Coordinates
		-0.5f, -0.5f, 0.0f,  0.0f, 0.0f,  // Bottom-left corner
		 0.5f, -0.5f, 0.0f,  1.0f, 0.0f,  // Bottom-right corner
		 0.5f,  0.5f, 0.0f,  1.0f, 1.0f,  // Top-right corner
		-0.5f,  0.5f, 0.0f,  0.0f, 1.0f   // Top-left corner
	};

	ImGuiController _controller;

	private int _vertexBufferObject;
	private int _vertexArrayObject;

	private Shader _shader;

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

	private bool _godModeEnabled = false;
	private float _thingsSpeed = 60f;
	private float _spawnTimeInterval = 1f;
	private System.Numerics.Vector2 _playerSize;
	private float _lastUpdateTime;

	private Texture2d _healthEffect;
	private Texture2d _bombEffect;
	private Texture2d _justThing;

	public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
		: base(gameWindowSettings, nativeWindowSettings)
	{
	}

	protected override void OnLoad()
	{
		base.OnLoad();
		StbImage.stbi_set_flip_vertically_on_load(1);

		//CenterWindow();
		Graphics.PrintOpenGLInfo();

		GL.ClearColor(0.737f, 0.718f, 0.647f, 1.0f);

		_vertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

		GL.BufferData(BufferTarget.ArrayBuffer, _rectVertices.Length * sizeof(float), _rectVertices, BufferUsage.StaticDraw);

		_vertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(_vertexArrayObject);

		// Position attribute
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
		GL.EnableVertexAttribArray(0);

		// Texture coordinate attribute
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
		GL.EnableVertexAttribArray(1);

		_healthEffect = new Texture2d("Assets/Textures/effect_health.png").Load();
		_bombEffect = new Texture2d("Assets/Textures/effect_bomb.png").Load();
		_justThing  = new Texture2d("Assets/Textures/thing.png").Load();

		_shader = new Shader("Assets/Shaders/shader.vert", "Assets/Shaders/shader.frag");
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
			new(-150f, 95f),
			new(-110f, 110f)
		],
		this,
		1f,
		[
			new SpawnSimpleEntity(_justThing),
			new SpanwHealthEntity(_healthEffect),
			new SpawnKillAllEntity(_bombEffect),
		]);

		AudioManager.Ins.LoadSound("start", "Assets/Music/ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("player_heal", "Assets/Music/player_heal.mp3");
		AudioManager.Ins.LoadSound("thing_taken", "Assets/Music/thing_taken.wav");
		AudioManager.Ins.LoadSound("kill_all", "Assets/Music/kill_all.mp3");
		AudioManager.Ins.LoadSound("player_take_damage", "Assets/Music/player_take_damage.mp3");
		AudioManager.Ins.LoadSound("main_theme", "Assets/Music/main_theme.mp3");

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

		Vector4 hpColor = new(0.9f, 0.4f, 0.35f, 1f);
		float hpScaleFactor = _player.Hp / (float)_player.MaxHp;
		hpColor *= hpScaleFactor;
		DrawObject(_player.Position, _player.Size, (Color4<Rgba>)hpColor, _playerTexture);

		foreach(var thing in _fallingThings)
		{
			DrawObject(thing.Position, thing.Size, Color4.White, thing.Texture);
		}

		if (_showImGui)
		{
			// Enable Docking
			//ImGui.DockSpaceOverViewport();
			ImGui.Begin("Debug");
			{
				ImGui.SeparatorText("Game info");
				ImGui.Text($"Score: {_score}");
				ImGui.Text($"Health: {_player.Hp}");
				ImGui.Text($"Falling things: {_fallingThings.Count}");

				ImGui.SeparatorText("Player settings");
				ImGui.Checkbox("God mode", ref _godModeEnabled);
				ImGui.SliderFloat2("Player Size ", ref _playerSize, 1f, 200f);

				ImGui.SeparatorText("Spawner settings");
				ImGui.SliderFloat("Things speed", ref _thingsSpeed, 1f, 200f);
				ImGui.SliderFloat("Spawn time", ref _spawnTimeInterval, 0.01f, 1f);

				ImGui.SeparatorText("Performance");
				ImGui.Text($"Render time: {e.Time}");
				ImGui.Text($"Update time: {_lastUpdateTime}");
			}
			ImGui.End();
			_controller.Render();
			ImGuiController.CheckGLError("End of frame");
		}

		SwapBuffers();
	}

	private void DrawObject(Vector2 position, Vector2 size, Color4<Rgba> color, Texture2d? texture = null)
	{
		Vector2 shadowOffset = new(1, -1);
		Color4<Rgba> shadowColor = new(0f, 0f, 0f, 0.25f);

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		DrawRect(position + shadowOffset, size, shadowColor, texture);

		DrawRect(position, size, color, texture);

		GL.Disable(EnableCap.Blend);
	}

	private void DrawRect(Vector2 position, Vector2 size, Color4<Rgba> color, Texture2d? texture = null)
	{
		_shader.Use();
		_shader.SetMatrix4("viewProj", _camera.ViewProj);

		var model = Matrix4.CreateScale(size.To3(1f)) * Matrix4.CreateTranslation(position.To3());
		_shader.SetMatrix4("model", model);
		_shader.SetColor("color", color);

		if (texture != null)
		{
			texture?.BindAsSampler(0);
			_shader.SetInt("diffuseTexture", 0);
		}

		GL.BindVertexArray(_vertexArrayObject);
		GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

		texture?.Unbind();
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
		_camera.Update(dt);

		_spawner.Speed = _thingsSpeed;
		_spawner.TimeInterval = _spawnTimeInterval;

		_player.SetGodMode(_godModeEnabled);
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
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindVertexArray(0);
		GL.UseProgram(0);

		GL.DeleteBuffer(_vertexBufferObject);
		GL.DeleteVertexArray(_vertexArrayObject);

		GL.DeleteProgram(_shader.Handle);

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
}
