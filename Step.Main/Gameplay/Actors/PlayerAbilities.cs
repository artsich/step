using OpenTK.Windowing.GraphicsLibraryFramework;
using Step.Engine.Audio;
using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Actors;

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

public class ShieldAbility : ActiveAbility
{
	private readonly Player _player;

	public ShieldAbility(Player player, PlayerShield shield)
	{
		_player = player;
		_player.AddChild(shield);
	}

	protected override void OnActivated()
	{
		_player
			.GetChildOf<PlayerShield>()
			.Enable();
	}

	protected override void OnDeactivated()
	{
		_player
			.GetChildOf<PlayerShield>()
			.Disable();
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
	private readonly List<IAbility> _abilities = [];

	public void Add(IAbility ability)
	{
		_abilities.Add(ability);

		if (!ability.IsActive)
		{
			player.CallDeferred(ability.Activate);
		}
	}

	public void Clear()
	{
		foreach (var ability in _abilities)
		{
			ability.Deactivate();
		}
		_abilities.Clear();
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
		TryAbility<ShieldAbility>(Keys.Space);
		TryAbility<SizeChangerAbility>(MouseButton.Left);
		TryAbility<TimeFreezeAbility>(MouseButton.Right);
	}

	private T? FindActiveAbility<T>() 
		where T : IAbility
	{
		foreach (var ability in _abilities)
		{
			if (ability.IsActive && ability is T typedAbility)
				return typedAbility;
		}

		return default;
	}

	private void TryAbility<T>(MouseButton btn) where T : IAbility
	{
		var ability = FindActiveAbility<T>();
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

	private void TryAbility<T>(Keys key) where T : IAbility
	{
		var ability = FindActiveAbility<T>();
		if (ability is null)
		{
			return;
		}

		if (input.KeyboardState.IsKeyPressed(key))
		{
			ability.Activate();
		}
		else if (input.KeyboardState.IsKeyReleased(key))
		{
			ability.Deactivate();
		}
	}
}
