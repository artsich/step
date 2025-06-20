using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.Actors;

namespace Step.Main.Gameplay;

public class GameLoop(GameInfo gameInfo) : GameObject(nameof(GameLoop))
{
	private Camera2d? _camera;
	private Player? _player;
	private Spawner? _spawner;

	public Action? OnFinish;

	protected override void OnStart()
	{
		_camera = GetChildOf<Camera2d>();
		_player = GetChildOf<Player>();
		_spawner = GetChildOf<Spawner>();

		_spawner.OnSpawn += OnEnemySpawn;

		_player.OnDeath += OnPlayerDeath;
		_player.OnDamage += OnPlayerDamage;

		_player.OnCrossCoinCollected += OnCrossCoinCollected;
	}

	private void OnEnemySpawn(GameObject obj)
	{
		if (obj is GliderEntity)
		{
			gameInfo.AddCoin(Coin.Glider, 1);
		}
		else if (obj is CircleEnemy)
		{
			gameInfo.AddCoin(Coin.Circle, 1);
		}
	}

	private void OnCrossCoinCollected()
	{
		gameInfo.AddCoin(Coin.Cross, 1);
	}

	private void OnPlayerDamage()
	{
		_camera!.Shake(2f, 0.2f);
	}

	protected override void OnEnd()
	{
		_spawner!.OnSpawn -= OnEnemySpawn;

		_player!.OnDeath -= OnPlayerDeath;
		_player.OnCrossCoinCollected -= OnCrossCoinCollected;
		_player.OnDamage -= OnPlayerDamage;
	}

	private void OnPlayerDeath()
	{
		OnFinish?.Invoke();
	}
}
