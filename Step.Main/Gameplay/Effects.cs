using Serilog;

namespace Step.Main.Gameplay;

public interface IEffect
{
	bool IsCompleted { get; }

	bool Use();

	void Update(float dt) { }
}

public class SizeChangeEffect(
	Player player,
	float horizontalSize = 20f,
	float durationInSec = 10f) : IEffect
{
	private bool _started;
	private float _time;

	public bool IsCompleted { get; private set; }

	public bool Use()
	{
		if (!CanApply())
		{
			Log.Logger.Information("Size already changed...");
			return false;
		}

		_started = true;
		player.Resize(new(player.Size.X + horizontalSize, player.Size.Y));

		return true;
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

	private bool CanApply() => !player.HasActiveEffect<SizeChangeEffect>();

	private void End(Player player)
	{
		_started = false;
		player.Resize(new(player.Size.X - horizontalSize, player.Size.Y));
		IsCompleted = true;
	}
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

	public bool Use()
	{
		if (!CanApply())
		{
			Log.Logger.Information("Speed already active...");
			return false;
		}

		_started = true;
		player.SpeedScale = speedScale;

		return true;
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

	private bool CanApply() => !player.HasActiveEffect<SpeedEffect>();
}

public class HealEffect(int hp, Player player) : IEffect
{
	public bool IsCompleted { get; private set; }

	public bool Use()
	{
		if (!CanApply())
		{
			Log.Logger.Information("Health if full...");
			return false;
		}

		player.AddHp(hp);
		Log.Logger.Information("Heal effect used...");
		IsCompleted = true;

		return true;
	}

	private bool CanApply() => !player.IsFullHp;
}

public class KillAllEffect(IGameScene scene) : IEffect
{
	public bool IsCompleted { get; private set; }

	public bool Use()
	{
		scene.KillEnemies();
		Log.Logger.Information("Kill all used...");

		IsCompleted = true;
		return true;
	}
}
