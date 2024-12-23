using ImGuiNET;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;

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

	// todo: not good solution as it hard to scale from different sources.
	public float SpeedScale { get => _speedScale; set
		{
			_speedScale = Math.Clamp(value, 0f, float.MaxValue);
		}
	}
	private static readonly float DefaultSpeedScale = 1f;
	private float _speedScale = DefaultSpeedScale;

	public void ResetSpeedScale()
	{
		_speedScale = DefaultSpeedScale;
	}

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
	private readonly Dictionary<Type, IEffect> _activatedEffects = [];

	public int Hp { get; private set; } = 5;

	public bool IsFullHp => Hp >= MaxHp;

	private bool godMode = false;

	public void Update(float dt)
	{
		UpdateEffects(dt);
		Move(input, dt);
		ResolveWorldCollision();
	}

	public void Resize(Vector2 newSize)
	{
		Size = newSize;
	}

	public int EffectsCount<T>() where T : IEffect
	{
		return _effects.OfType<T>().Count();
	}

	public bool HasActiveEffect<T>() => _activatedEffects.ContainsKey(typeof(T));

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
		Console.WriteLine($"Effect `{effect.GetType().Name}` taken");
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
		foreach (var effect in Enumerable.Reverse(_effects))
		{
			var name = effect.GetType().Name;
			ImGui.BulletText($"{name}");
		}

		ImGui.SeparatorText("Player settings");
		ImGui.Checkbox("God mode", ref godMode);

		var systemVectorSize = _size.ToSystem();
		ImGui.SliderFloat2("Player Size", ref systemVectorSize, 1f, 200f);
		Resize(systemVectorSize.FromSystem());
	}

	private void UpdateEffects(float dt)
	{
		if (input.IsKeyPressed(Keys.Space))
		{
			UseNextEffect();
		}

		foreach (var effect in _effects)
		{
			effect.Update(dt);
		}

		_effects.RemoveAll(effect =>
		{
			bool isCompleted = effect.IsCompleted;

			if (isCompleted)
			{
				_activatedEffects.Remove(effect.GetType());
				Console.WriteLine($"Effect completed {effect.GetType().Name}");
			}

			return isCompleted;
		});
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

		dashCdEllapsed += dt;
		if (input.IsKeyDown(Keys.LeftShift) && dashCdEllapsed > DashCd)
		{
			if (targetSpeed != 0f)
			{
				_velocity *= DashScale;
				dashCdEllapsed = 0f;
			}
		}

		_velocity = MathHelper.Lerp(_velocity, targetSpeed * _speedScale, Acceleration * dt);
		_position.X += _velocity * dt;
	}

	private void AddActivatedEffect(IEffect effect)
	{
		var effectType = effect.GetType();
		_activatedEffects.Add(effectType, effect);
	}

	private void UseNextEffect()
	{
		if (_effects.Count > 0)
		{
			var effect = _effects.Last();

			if (effect.CanApply())
			{
				effect.Use();
				AddActivatedEffect(effect);
				Console.WriteLine($"Effect `{effect.GetType().Name}` used");
			}
			else
			{
				Console.WriteLine($"Effect `{effect.GetType().Name}` can't be used...");
			}
		}
	}
}
