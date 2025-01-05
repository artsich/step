using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Serilog;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Graphics;
using Step.Engine.Graphics.Particles;

namespace Step.Main.Gameplay;

public class Player : GameObject
{
	private const float WallCollisionVelocityThreshold = 5f;

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

	public Vector2 Position => LocalTransform.Position;

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

	private Particles2d _dashParticles;
	private Particles2d _wallCollisionParticles;
	private bool _wallCollisionEffectTriggered = false;

	public Player(
		Vector2 position,
		Vector2 size,
		KeyboardState input,
		Box2 worldBb,
		Texture2d playerTexture,
		Renderer renderer) : base(nameof(Player))
	{
		_input = input;
		_worldBb = worldBb;
		_size = size;
		LocalTransform.Position = position;
		LocalTransform.Scale = Vector2.One;
		_playerTexture = playerTexture;
		_renderer = renderer;
	}

	protected override void OnStart()
	{
		_dashParticles = GetChildOf<Particles2d>("DashParticles");
		_wallCollisionParticles = GetChildOf<Particles2d>("WallCollisionParticles");
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
		Log.Logger.Information($"Effect `{effect.GetType().Name}` taken");
		_effects.Add(effect);
	}

	public void Take(Thing thing)
	{
		Log.Logger.Information("Collected thing...");
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

	protected override void OnDebugDraw()
	{
		ImGui.TextColored(new(1f, 0f, 0f, 1f), $"Health: {Hp}");

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

		foreach (var effect in _activatedEffects)
		{
			effect.Value.Update(dt);
		}

		var completed = _activatedEffects
			.Where(e => e.Value.IsCompleted)
			.Select(e => e.Key)
			.ToArray();

		foreach (var key in completed)
		{
			Log.Logger.Information($"Effect completed {key.Name}");
			_activatedEffects.Remove(key);
		}
	}

	private void DropEffect()
	{
		if (_effects.Count > 0)
		{
			var name = _effects[_selectedEffectId].GetType().Name;
			_effects.RemoveAt(_selectedEffectId);
			Log.Logger.Information($"{name} dropped...");
		}
	}

	private void RotateEffects(int dir)
	{
		if (_effects.Count == 0) return;
		_selectedEffectId = (_selectedEffectId + dir) % _effects.Count;
	}

	private void WallCollisionEffect(Vector2 pos, float angle)
	{
		if (!_wallCollisionEffectTriggered && 
			MathF.Abs(_velocity) > WallCollisionVelocityThreshold)
		{
			_wallCollisionEffectTriggered = true;

			_wallCollisionParticles.LocalTransform.Position = pos;

			_wallCollisionParticles.Emitter.DirectionAngle = angle;
			_wallCollisionParticles.Emitting = true;

			AudioManager.Ins.PlaySound("wall_collision");
		}
	}

	private void ResolveWorldCollision()
	{
		var box = Box;
		var halfSizeX = _size.X / 2f;

		if (!_worldBb.ContainsInclusive(box.Min))
		{
			WallCollisionEffect(new(-halfSizeX, 0f), MathF.PI);

			LocalTransform.Position = new(
				_worldBb.Min.X + halfSizeX,
				LocalTransform.Position.Y);
			_velocity = 0f;
		}
		else if (!_worldBb.ContainsInclusive(box.Max))
		{
			WallCollisionEffect(new(halfSizeX, 0f), 0f);

			LocalTransform.Position = new(
				_worldBb.Max.X - halfSizeX,
				LocalTransform.Position.Y);
			_velocity = 0f;
		}
		else
		{
			_wallCollisionEffectTriggered = false;
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
		if (input.IsKeyDown(Keys.LeftControl) && dashCdEllapsed > DashCd)
		{
			if (targetSpeed != 0f)
			{
				_velocity *= DashScale;
				dashCdEllapsed = 0f;

				_dashParticles.Emitting = true;
				AudioManager.Ins.PlaySound("player_dash");
			}
		}

		float dir = -Math.Sign(targetSpeed);
		_dashParticles.Emitter.DirectionSign = new Vector2(dir, _dashParticles.Emitter.DirectionSign.Y);

		_velocity = MathHelper.Lerp(_velocity, targetSpeed * _speedScale, Acceleration * dt);
		LocalTransform.Position = new Vector2(LocalTransform.Position.X + _velocity * dt, LocalTransform.Position.Y);
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
			Log.Logger.Information($"Effect `{effect.GetType().Name}` used");
		}
		else
		{
			Log.Logger.Information($"Effect `{effect.GetType().Name}` can't be used...");
		}
	}
}
