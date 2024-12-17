using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Step.Main.Audio;
using System.Drawing;

/*
 * Goals:
 *  guns - pistol, knife
 */

// StaticDraw: This buffer will rarely, if ever, update after being initially uploaded.
// DynamicDraw: This buffer will change frequently after being initially uploaded.
// StreamDraw: This buffer will change on every frame.

// To do this, we use the GL.VertexAttribPointer function
// This function has two jobs, to tell opengl about the format of the data, but also to associate the current array buffer with the VAO.
// This means that after this call, we have setup this attribute to source data from the current array buffer and interpret it in the way we specified.
// Arguments:
//   Location of the input variable in the shader. the layout(location = 0) line in the vertex shader explicitly sets it to 0.
//   How many elements will be sent to the variable. In this case, 3 floats for every vertex.
//   The data type of the elements set, in this case float.
//   Whether or not the data should be converted to normalized device coordinates. In this case, false, because that's already done.
//   The stride; this is how many bytes are between the last element of one vertex and the first element of the next. 3 * sizeof(float) in this case.
//   The offset; this is how many bytes it should skip to find the first element of the first vertex. 0 as of right now.
// Stride and Offset are just sort of glossed over for now, but when we get into texture coordinates they'll be shown in better detail.

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
		-0.5f, -0.5f, 0.0f,
		0.5f, -0.5f, 0.0f,
		0.5f,  0.5f, 0.0f,
		-0.5f,  0.5f, 0.0f
	};

	private int _vertexBufferObject;
	private int _vertexArrayObject;

	private Shader _shader;

	private Matrix4 _viewProj = Matrix4.CreateOrthographicOffCenter(-180f, 180f, -90f, 90f, -1f, 100f);

	private readonly List<Thing> _fallingThings = [];

	private Spawner _spawner;
	private Player _player;

	public Player Player => _player;

	private Queue<Action> _postActions = [];

	private bool paused = false;

	public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
		: base(gameWindowSettings, nativeWindowSettings)
	{
	}

	protected override void OnLoad()
	{
		base.OnLoad();
		PrintOpenGLInfo();

		GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);

		_vertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

		GL.BufferData(BufferTarget.ArrayBuffer, _rectVertices.Length * sizeof(float), _rectVertices, BufferUsage.StaticDraw);

		_vertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(_vertexArrayObject);

		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
		GL.EnableVertexAttribArray(0);

		_shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
		_player = new Player(new(0f, -75f), new(50f, 20f), this.KeyboardState, new Box2(-180f, -90f, 177f, 90f));

		_spawner = new Spawner(
		[
			new(150f, 100f),
			new(110f, 105f),
			new(-150f, 95f),
			new(-110f, 110f)
		],
		new Vector2(20, 20),
		1f,
		this);

		AudioManager.Ins.LoadSound("start", "Music/ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("player_heal", "Music/player_heal.mp3");
		AudioManager.Ins.LoadSound("thing_taken", "Music/thing_taken.wav");
		AudioManager.Ins.LoadSound("kill_all", "Music/kill_all.mp3");
		AudioManager.Ins.LoadSound("player_take_damage", "Music/player_take_damage.mp3");

		AudioManager.Ins.PlaySound("start");

		Player.OnPlayerHeal += () =>
		{
			AudioManager.Ins.PlaySound("player_heal");
		};

		Player.OnThingTaken += (_) =>
		{
			AudioManager.Ins.PlaySound("thing_taken");
		};

		Player.OnDamage += () =>
		{
			AudioManager.Ins.PlaySound("player_take_damage");
		};

		Player.OnDead += () =>
		{
			Console.WriteLine("Game over...");
			Close();
		};
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		Vector4 hpColor = new(0.9f, 0.4f, 0.35f, 1f);
		float hpScaleFactor = _player.Hp / (float)_player.MaxHp;
		hpColor *= hpScaleFactor;
		DrawObject(_player.Position, _player.Size, (Color4<Rgba>)hpColor);

		foreach(var thing in _fallingThings)
		{
			DrawObject(thing.Position, thing.Size, thing.Color);
		}

		SwapBuffers();
	}

	private void DrawObject(Vector2 position, Vector2 size, Color4<Rgba> color)
	{
		Vector2 shadowOffset = new(1, -1);
		Color4<Rgba> shadowColor = new(0f, 0f, 0f, 0.25f);

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		DrawRect(position + shadowOffset, size, shadowColor);
		GL.Disable(EnableCap.Blend);

		DrawRect(position, size, color);
	}

	private void DrawRect(Vector2 position, Vector2 size, Color4<Rgba> color)
	{
		_shader.Use();
		_shader.SetMatrix4("viewProj", _viewProj);

		var model = Matrix4.CreateScale(size.To3(1f)) * Matrix4.CreateTranslation(position.To3());
		_shader.SetMatrix4("model", model);
		_shader.SetColor("color", color);

		GL.BindVertexArray(_vertexArrayObject);
		GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
	}

	protected override void OnUpdateFrame(FrameEventArgs e)
	{
		base.OnUpdateFrame(e);
		float dt = (float)e.Time;

		var input = KeyboardState;

		if (input.IsKeyDown(Keys.Escape))
		{
			Close();
		}

		if (input.IsKeyPressed(Keys.P))
		{
			paused = !paused;
		}

		CheckWindowStateToggle(input);

		if (!paused)
		{
			GameUpdate(dt);
		}


		while (_postActions.TryDequeue(out var action))
		{
			action();
		}
	}

	private void GameUpdate(float dt)
	{
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
				_postActions.Enqueue(() =>
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

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);
		GL.Viewport(0, 0, Size.X, Size.Y);
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
		_postActions.Enqueue(_fallingThings.Clear);
		AudioManager.Ins.PlaySound("kill_all");
	}

	static void PrintOpenGLInfo()
	{
		string vendor = GL.GetString(StringName.Vendor) ?? "Vendor not found...";
		string renderer = GL.GetString(StringName.Renderer) ?? "Renderer not found...";
		string version = GL.GetString(StringName.Version) ?? "Opengl version is not found...";
		string glslVersion = GL.GetString(StringName.ShadingLanguageVersion) ?? "GLSL version not found...";

		Console.WriteLine("---------------------------------------");
		Console.WriteLine("OpenGL Information:");
		Console.WriteLine($"Vendor: {vendor}");
		Console.WriteLine($"Renderer: {renderer}");
		Console.WriteLine($"OpenGL Version: {version}");
		Console.WriteLine($"GLSL Version: {glslVersion}");
		Console.WriteLine("---------------------------------------");
	}
}