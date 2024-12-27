using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Step.Main.Audio;
using Step.Main.ParticleSystem;

namespace Step.Main.Gameplay;

public class Player : GameObject
{
	private float _velocity;
	private Box2 _worldBb;
	private readonly KeyboardState _input;

	public event Action? OnPlayerHeal;
	public event Action<Thing>? OnThingTaken;
	public event Action? OnDead;
	public event Action? OnDamage;

	public float MaxSpeed { get; } = 200f;

	public float DashScale { get; } = 5f;
	public float DashCd = 2f;
	private float dashCdEllapsed = 0f;

	public float Acceleration { get; } = 10f;

	private static readonly float DefaultSpeedScale = 1f;
	private float _speedScale = DefaultSpeedScale;

	private readonly List<IEffect> _effects = [];
	private int _selectedEffectId = 0;
	private readonly Dictionary<Type, IEffect> _activatedEffects = [];

	public int MaxHp { get; set; } = 5;

	public int Hp { get; private set; } = 5;

	private bool godMode = false;

	public bool IsFullHp => Hp >= MaxHp;

	public Vector2 Position => localTransform.Position;

	private Vector2 _size;

	public Vector2 Size => _size;

	public Box2 Box => new(Position - Size / 2f, Position + Size / 2f);

	public float SpeedScale
	{
		get => _speedScale;
		set => _speedScale = Math.Clamp(value, 0f, float.MaxValue);
	}

	private Texture2d _playerTexture;
	private Renderer _renderer;

	private Particles2d _particles;

	public Player(
		Vector2 position,
		Vector2 size,
		KeyboardState input,
		Box2 worldBb,
		Texture2d playerTexture,
		Renderer renderer,
		string name = "Player") : base(name)
	{
		_input = input;
		_worldBb = worldBb;
		_size = size;
		localTransform.Position = position;
		localTransform.Scale = Vector2.One;
		_playerTexture = playerTexture;
		_renderer = renderer;
	}

	public override void OnStart()
	{
		_particles = GetChildOf<Particles2d>();
	}

	public void ResetSpeedScale() => _speedScale = DefaultSpeedScale;

	public void Resize(Vector2 newSize)
	{
		_size = newSize;
	}

	public int EffectsCount<T>() where T : IEffect
	{
		return _effects.OfType<T>().Count();
	}

	public bool HasActiveEffect<T>() => _activatedEffects.ContainsKey(typeof(T));

	public void AddHp(int hp)
	{
		if (hp < 0) throw new ArithmeticException("Can't add negative HP...");
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

	protected override void OnUpdate(float deltaTime)
	{
		UpdateEffects(deltaTime);
		Move(_input, deltaTime);
		ResolveWorldCollision();
	}

	protected override void OnRender()
	{
		_renderer.DrawObject(Position, Size, Color4.White, _playerTexture);
	}

	public override void DebugRender()
	{
		ImGui.SeparatorText("Player stats");
		ImGui.TextColored(new(1f, 0f, 0f, 1f), $"Health: {Hp}");

		ImGui.SeparatorText("Player settings");
		ImGui.Checkbox("God mode", ref godMode);

		var systemVectorSize = _size.ToSystem();
		ImGui.SliderFloat2("Player Size", ref systemVectorSize, 1f, 200f);
		Resize(systemVectorSize.FromSystem());

		if (_activatedEffects.Count > 0)
		{
			ImGui.SeparatorText("Active effects");
			ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.0f, 0.7f, 0.0f, 1.0f));
			foreach (var effectType in _activatedEffects.Keys)
				ImGui.BulletText($"{effectType.Name}");
			ImGui.PopStyleColor();
		}

		if (_effects.Count > 0)
		{
			ImGui.SeparatorText("Effects");
			for (int i = 0; i < _effects.Count; i++)
			{
				var effect = _effects[i];
				var name = effect.GetType().Name;
				if (i == _selectedEffectId)
				{
					ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f));
					ImGui.BulletText($"{name}");
					ImGui.PopStyleColor();
				}
				else
				{
					ImGui.BulletText($"{name}");
				}
			}
		}
	}

	private void UpdateEffects(float dt)
	{
		if (_input.IsKeyPressed(Keys.Space))
		{
			UseNextEffect();
		}

		_selectedEffectId = Math.Clamp(_selectedEffectId, 0, Math.Max(0, _effects.Count - 1));

		if (_input.IsKeyPressed(Keys.Up))
		{
			RotateEffects(-1);
		}

		if (_input.IsKeyPressed(Keys.Down))
		{
			RotateEffects(1);
		}

		if (_input.IsKeyPressed(Keys.D))
		{
			DropEffect();
		}

		foreach (var effect in _activatedEffects) effect.Value.Update(dt);

		var completed = _activatedEffects
			.Where(e => e.Value.IsCompleted)
			.Select(e => e.Key)
			.ToList();

		foreach (var key in completed)
		{
			Console.WriteLine($"Effect completed {key.Name}");
			_activatedEffects.Remove(key);
		}
	}

	private void DropEffect()
	{
		if (_effects.Count > 0)
		{
			var name = _effects[_selectedEffectId].GetType().Name;
			_effects.RemoveAt(_selectedEffectId);
			Console.WriteLine($"{name} dropped...");
		}
	}

	private void RotateEffects(int dir)
	{
		if (_effects.Count == 0) return;
		_selectedEffectId = (_selectedEffectId + dir) % _effects.Count;
	}

	private void ResolveWorldCollision()
	{
		var box = Box;
		var halfSizeX = _size.X / 2f;

		if (!_worldBb.ContainsInclusive(box.Min))
		{
			localTransform.Position = new Vector2(_worldBb.Min.X + halfSizeX, localTransform.Position.Y);
		}

		if (!_worldBb.ContainsInclusive(box.Max))
		{
			localTransform.Position = new Vector2(_worldBb.Max.X - halfSizeX, localTransform.Position.Y);
		}
	}

	private void Move(KeyboardState input, float dt)
	{
		float targetSpeed = 0f;
		if (input.IsKeyDown(Keys.Left)) targetSpeed = -MaxSpeed;
		else if (input.IsKeyDown(Keys.Right)) targetSpeed = MaxSpeed;

		dashCdEllapsed += dt;
		if (input.IsKeyDown(Keys.LeftShift) && dashCdEllapsed > DashCd)
		{
			if (targetSpeed != 0f)
			{
				_velocity *= DashScale;
				dashCdEllapsed = 0f;

				float dir = Math.Sign(_velocity);
				if (dir == -1)
				{
					dir = 0f;
				}
				else
				{
					dir = 3.14f;
				}
				_particles.Emitter.DirectionAngle = dir;

				_particles.Emitting = true;

				AudioManager.Ins.PlaySound("player_dash");
			}
		}

		_velocity = MathHelper.Lerp(_velocity, targetSpeed * _speedScale, Acceleration * dt);
		localTransform.Position = new Vector2(localTransform.Position.X + _velocity * dt, localTransform.Position.Y);
	}

	private void AddActivatedEffect(IEffect effect)
	{
		var effectType = effect.GetType();
		_activatedEffects.Add(effectType, effect);
	}

	private void UseNextEffect()
	{
		if (_effects.Count == 0)
			return;

		var effect = _effects[_selectedEffectId];
		if (effect.CanApply())
		{
			effect.Use();
			_effects.RemoveAt(_selectedEffectId);
			AddActivatedEffect(effect);
			Console.WriteLine($"Effect `{effect.GetType().Name}` used");
		}
		else
		{
			Console.WriteLine($"Effect `{effect.GetType().Name}` can't be used...");
		}
	}
}
