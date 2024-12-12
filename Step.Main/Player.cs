using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;


/*
 * Goals:
 * 1) smooth platform moving, dash, screen bounding box
 * 2) falling spheres
 * 
 * 
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

public class Player(
	Vector2 position,
	Vector2 size,
	KeyboardState input,
	Box2 worldBb)
{
	private float _velocity;
	private Vector2 _position = position;

	public float MaxSpeed { get; } = 200f;

	public float DashScale { get; } = 5f;
	public float DashCd = 2f;
	private float dashCdEllapsed = 0f;

	public float Acceleration { get; } = 10f;

	public Vector2 Position => _position;

	public Vector2 Size { get; } = size;

	public Box2 Box => new(Position - (Size / 2f), Position + (Size / 2f));


	public int MaxHp = 5;
	private List<IEffect> _effects = [];

	public int Hp { get; private set; } = 1;

	public bool IsFullHp => Hp == MaxHp;

	public void Update(float dt)
	{
		Move(input, dt);
		ResolveWorldCollision();
	}

	public void AddHp(int hp)
	{
		Hp += hp;
		Hp = Math.Min(Hp, MaxHp);
	}

	public void AddEffect(IEffect effect)
	{
		Console.WriteLine($"Effect added {effect.GetType().Name}");
		_effects.Add(effect);
	}

	public void Take(Thing thing)
	{
		Console.WriteLine("Collected thing...");
		thing.ApplyEffect(this);
	}

	private void ResolveWorldCollision()
	{
		var box = Box;

		var halfSizeX = Size.X / 2f;

		if (!worldBb.ContainsInclusive(box.Min))
		{
			_position.X = worldBb.Min.X + halfSizeX;
		}

		if (!worldBb.ContainsInclusive(box.Max))
		{
			_position.X = worldBb.Max.X - halfSizeX;
		}
	}

	private void Move(KeyboardState input, float dt)
	{
		float targetSpeed = 0f;

		if (input.IsKeyDown(Keys.Left))
		{
			targetSpeed = -MaxSpeed;
		}
		else if (input.IsKeyDown(Keys.Right))
		{
			targetSpeed = MaxSpeed;
		}

		if (input.IsKeyPressed(Keys.Up))
		{
			UseNextEffect();
		}

		dashCdEllapsed += dt;
		if (input.IsKeyDown(Keys.LeftShift) && dashCdEllapsed > DashCd)
		{
			if (targetSpeed != 0f)
			{
				_velocity *= DashScale;
				dashCdEllapsed = 0f;
			}
		}

		_velocity = MathHelper.Lerp(_velocity, targetSpeed, Acceleration * dt);

		_position.X += _velocity * dt;
	}

	private void UseNextEffect()
	{
		if (_effects.Count > 0)
		{
			_effects.Last().Use();
			_effects.RemoveAt(_effects.Count - 1);
		}
	}
}
