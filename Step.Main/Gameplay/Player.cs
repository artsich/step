using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay;

public interface IAbility
{
	bool IsActive { get; }

	void Activate() { }

	void Deactivate() { }

	void Update(float deltaTime) { }
}

public abstract class PassiveAbility : IAbility
{
	public bool IsActive => false;

	public virtual void Activate() { }

	public virtual void Deactivate() { }

	public virtual void Update(float dt) { }
}

public abstract class ActiveAbility : IAbility
{
	public bool IsActive => true;

	public float Duration { get; init; } = 1f;

	private float _elapsedTime = 0f;

	protected bool Activated = false;

	public void Activate()
	{
		if (Activated)
		{
			return;
		}

		Activated = true;
		OnActivated();
	}

	public void Deactivate()
	{
		if (!Activated)
		{
			return;
		}

		Activated = false;
		OnDeactivated();
	}

	protected abstract void OnActivated();

	protected abstract void OnDeactivated();

	public void Update(float dt)
	{
		if (Activated)
		{
			_elapsedTime += GameRoot.I.RealDt;
			if (_elapsedTime > Duration)
			{
				Deactivate();
			}
		}
		else
		{
			_elapsedTime = Math.Max(_elapsedTime - dt, 0f);
		}
	}
}

public class RegenerationAbility(Player player) : PassiveAbility
{
	public float HealFactor { get; init; } = 0.1f;

	public float HealPerSecond { get; init; } = 1f;

	private float _timeElapsed;

	public override void Update(float deltaTime) 
	{
		if (player.Hp < player.MaxHp)
		{
			_timeElapsed += deltaTime;

			if (_timeElapsed > HealPerSecond)
			{
				player.Heal(HealFactor);
				_timeElapsed = 0f;
			}
		}
	}
}

public class SpeedIncreaseAbility(Player player) : PassiveAbility
{
	private readonly float speedMultiplier = 2f;

	public override void Activate()
	{
		player.Speed *= speedMultiplier;
	}

	public override void Deactivate()
	{
		player.Speed /= speedMultiplier;
	}
}

public class MagnetAbility(float radius, GameObject magnetOwner, Renderer renderer) : PassiveAbility
{
	public override void Activate()
	{
		if (magnetOwner.Contains<MagnetZone>())
		{
			throw new InvalidOperationException("Magnet already active...");
		}

		magnetOwner.CallDeferred(() =>
		{
			var magnet = new MagnetZone(renderer)
			{
				Radius = radius,
			};
			magnetOwner.AddChild(magnet);
			magnet.Start();
		});
	}

	public override void Deactivate()
	{
		magnetOwner
			.GetChildOf<MagnetZone>()
			.QueueFree();
	}
}

public class SizeChangerAbility(Player player) : ActiveAbility
{
	private readonly float _scaleMultiplier = 0.3f;

	protected override void OnActivated()
	{
		player.LocalTransform.Scale *= _scaleMultiplier;
	}

	protected override void OnDeactivated()
	{
		player.LocalTransform.Scale /= _scaleMultiplier;
	}
}

public class TimeFreezeAbility : ActiveAbility
{
	private readonly float _freezeScale = 0.5f;
	private readonly float _initialScale = 1f;

	protected override void OnActivated()
	{
		AudioManager.Ins.SlowDown(_freezeScale);
		GameRoot.I.TimeScale = _freezeScale;
	}

	protected override void OnDeactivated()
	{
		AudioManager.Ins.SlowDown(_initialScale);
		GameRoot.I.TimeScale = _initialScale;
	}
}

public class PlayerAbilities(Input input, Player player)
{
	private readonly IList<IAbility> _abilities = [];

	public void Add(IAbility ability)
	{
		_abilities.Add(ability);

		if (!ability.IsActive)
		{
			player.CallDeferred(ability.Activate);
		}
	}

	public void Update(float dt)
	{
		ProcessInput();

		foreach (var a in _abilities)
		{
			a.Update(dt);
		}
	}

	private void ProcessInput()
	{
		TryAbility<SizeChangerAbility>(MouseButton.Left);
		TryAbility<TimeFreezeAbility>(MouseButton.Right);
	}

	private void TryAbility<T>(MouseButton btn) where T : IAbility
	{
		var ability = _abilities
				.Where(a => a.IsActive)
				.OfType<T>()
				.FirstOrDefault();

		if (ability is null)
		{
			return;
		}

		if (input.MouseState.IsButtonPressed(btn))
		{
			ability.Activate();
		}
		else if (input.MouseState.IsButtonReleased(btn))
		{
			ability.Deactivate();
		}
	}
}

public class Player : GameObject, ITarget
{
	[EditorProperty]
	public float Speed { get; set; } = 30f;

	public float MaxHp { get; private set; } = 5f;

	[EditorProperty]
	public float Hp { get; private set; } = 5f;

	public event Action OnDeath;

	public event Action OnDamage;

	public Vector2 Position => GlobalPosition;

	private PlayerAbilities _playerAbilities;
	private RectangleShape2d _collisionShape;
	private readonly Input _input;

	public Player(Input input)
		: base(name: nameof(Player))
	{
		_playerAbilities = new(input, this);
		_input = input;
	}

	public void Heal(float healFactor)
	{
		Hp = Math.Min(Hp + healFactor, MaxHp);
	}

	public void AddAbility(IAbility ability) => _playerAbilities.Add(ability);

	protected override void OnStart()
	{
		_collisionShape = GetChildOf<RectangleShape2d>();
		_collisionShape.OnCollision += OnCollision;
	}

	private void TakeDamage(float amount)
	{
		Hp -= amount;
		if (Hp <= 0)
		{
			OnDeath?.Invoke();
			QueueFree();
		}
		else
		{
			OnDamage?.Invoke();
		}
	}

	private void OnCollision(CollisionShape shape)
	{
		if (shape.Parent is GliderEntity glider)
		{
			AudioManager.Ins.PlaySound("player_hurt_glider");
			TakeDamage(1);
			glider.QueueFree();
		}
		else if (shape.Parent is CircleEnemy circle)
		{
			AudioManager.Ins.PlaySound("player_hurt_circle");
			TakeDamage(1);
			circle.QueueFree();
		}
		else if (shape.Parent is CrossEnemy cross)
		{
			AudioManager.Ins.PlaySound("player_pickup");
			cross.QueueFree();
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		_playerAbilities.Update(deltaTime);
		Move(deltaTime);
	}

	private void Move(float deltaTime)
	{
		var pos = LocalTransform.Position;
		var mouse = _input.MouseScreenPosition;
		var diff = mouse - pos;

		if (diff.LengthSquared > 0.01f)
		{
			var dir = diff.Normalized();
			pos += dir * Speed * deltaTime;
			LocalTransform.Position = pos;
		}
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}
