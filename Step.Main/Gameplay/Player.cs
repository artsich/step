using OpenTK.Windowing.GraphicsLibraryFramework;
using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Editor;

namespace Step.Main.Gameplay;

public interface IAbility
{
	bool IsActive { get; }

	void Activate();

	void Deactivate();

	void Update(float deltaTime) { }
}

public abstract class PassiveAbility : IAbility
{
	public bool IsActive => false;

	public abstract void Activate();

	public abstract void Deactivate();
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

public class Player :  GameObject
{
	[EditorProperty]
	public float Speed { get; set; } = 30f;

	[EditorProperty]
	public float Hp { get; set; } = 5f;

	[EditorProperty]
	public float HealBonusSpeed { get; set; } = 1f;

	private PlayerAbilities _playerAbilities;
	private readonly Input _input;

	public Player(Input input)
		: base(name: nameof(Player))
	{
		_playerAbilities = new(input, this);
		_input = input;
	}

	public void AddAbility(IAbility ability) => _playerAbilities.Add(ability);

	protected override void OnUpdate(float deltaTime)
	{
		_playerAbilities.Update(deltaTime);
		Move(deltaTime);
	}

	private void Move(float deltaTime)
	{
		var pos = LocalTransform.Position;
		var mouse = _input.MouseScreenPosition;

		var dir = (mouse - pos).Normalized();

		pos += dir * Speed * deltaTime;
		LocalTransform.Position = pos;
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}
