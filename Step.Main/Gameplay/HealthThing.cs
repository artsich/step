namespace Step.Main.Gameplay;

public interface IEffect
{
	bool IsCompleted { get; }

	void Use();

	bool CanApply() => true;

	void Update(float dt) { }
}

public class SpeedEffect(
	Player player,
	float speedScale = 2f,
	float durationInSec = 5f
) : IEffect
{
	private bool _started;
	private float _time;

	public bool IsCompleted { get; private set; }

	public void Use()
	{
		if (!CanApply())
		{
			throw new InvalidOperationException("Speed already active...");
		}

		_started = true;
		player.SpeedScale = speedScale;
	}

	public void Update(float dt)
	{
		if (_started)
		{
			_time += dt;

			if (_time > durationInSec)
			{
				End(player);
			}
		}
	}

	private void End(Player player)
	{
		_started = false;
		player.ResetSpeedScale();
		IsCompleted = true;
	}

	public bool CanApply()
	{
		var hasActiveSpeed = player.HasActiveEffect<SpeedEffect>();
		return !hasActiveSpeed;
	}
}

public class HealEffect(int hp, Player player) : IEffect
{
	public bool IsCompleted { get; private set; }

	public bool CanApply() => !player.IsFullHp;

	public void Use()
	{
		if (!CanApply())
		{
			throw new InvalidOperationException("Health if full...");
		}

		player.AddHp(hp);
		Console.WriteLine("Heal effect used...");
		IsCompleted = true;
	}
}

public class KillAllEffect(IGameScene scene) : IEffect
{
	public bool IsCompleted { get; private set; }

	public void Use()
	{
		scene.KillThings();
		Console.WriteLine("Kill all used...");
		IsCompleted = true;
	}
}
