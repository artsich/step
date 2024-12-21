using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Step.Main;

public class Player(
	Vector2 position,
	Vector2 size,
	KeyboardState input,
	Box2 worldBb)
{
	private float _velocity;
	private Vector2 _position = position;
	private Vector2 _size = size;

	public event Action? OnPlayerHeal;
	public event Action<Thing>? OnThingTaken;
	public event Action? OnDead;
	public event Action? OnDamage;

	public float MaxSpeed { get; } = 200f;

	public float DashScale { get; } = 5f;
	public float DashCd = 2f;
	private float dashCdEllapsed = 0f;

	public float Acceleration { get; } = 10f;

	public Vector2 Position => _position;

	public Vector2 Size
	{ 
		get => _size;
		private set
		{
			_size = value;
		}
	}

	public Box2 Box => new(Position - (Size / 2f), Position + (Size / 2f));

	public int MaxHp { get; set; } = 5;
	private readonly List<IEffect> _effects = [];

	public int Hp { get; private set; } = 5;

	public bool IsFullHp => Hp >= MaxHp;

	private bool godMode = false;

	public void Update(float dt)
	{
		Move(input, dt);
		ResolveWorldCollision();
	}

	public void Resize(Vector2 newSize)
	{
		Size = newSize;
	}

	public void AddHp(int hp)
	{
		if (hp < 0)
		{
			throw new ArithmeticException("Can't add negative HP...");
		}

		if (Hp + hp <= MaxHp)
		{
			Hp += hp;
			OnPlayerHeal?.Invoke();
		}
	}

	public void Damage(int damage)
	{
		if (godMode)
		{
			OnDamage?.Invoke();
			return;
		}

		if (Hp <= 0)
		{
			throw new InvalidOperationException("Player already Dead...");
		}

		Hp -= damage;

		if (Hp <= 0)
		{
			OnDead?.Invoke();
		}
		else
		{
			OnDamage?.Invoke();
		}
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
		OnThingTaken?.Invoke(thing);
	}

	public void DrawDebug()
	{
		ImGui.SeparatorText("Player stats");

		ImGui.TextColored(new(1f, 0f, 0f, 1f), $"Health: {Hp}");
		foreach (var effect in _effects.GroupBy(x => x.GetType().Name))
		{
			ImGui.BulletText($"{effect.Key}: {effect.Count()}");
		}

		ImGui.SeparatorText("Player settings");
		ImGui.Checkbox("God mode", ref godMode);

		var systemVectorSize = _size.ToSystem();
		ImGui.SliderFloat2("Player Size", ref systemVectorSize, 1f, 200f);
		Resize(systemVectorSize.FromSystem());
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
