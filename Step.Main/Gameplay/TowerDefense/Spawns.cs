using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Spawns : GameObject
{
	private readonly Renderer _renderer;
	private readonly Level _level;
	private readonly SpawnSettings _spawnSettings;
	private readonly List<Sprite2d> _spawnMarkers = [];
	private readonly List<Enemy> _activeEnemies = [];
	private readonly float _spawnSize = 25f;
	private readonly float _spawnIntervalSeconds;

	private float _spawnTimer;
	private int _spawnedCount;
	private int _spawnIndex;

	public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;

	public event Action<Enemy>? EnemyReachedBase;

	public Spawns(Renderer renderer, Level level) : base(nameof(Spawns))
	{
		_renderer = renderer;
		_level = level;
		_spawnSettings = level.Spawn;
		_spawnIntervalSeconds = MathF.Max(_spawnSettings.SpawnIntervalSeconds, 0.001f);

		CreateSpawnMarkers();
	}

	protected override void OnUpdate(float deltaTime)
	{
		CleanupInactiveEnemies();

		if (_spawnedCount >= _spawnSettings.EnemyCount)
			return;

		_spawnTimer += deltaTime;

		while (_spawnTimer >= _spawnIntervalSeconds && _spawnedCount < _spawnSettings.EnemyCount)
		{
			_spawnTimer -= _spawnIntervalSeconds;
			SpawnEnemy();
		}
	}

	private void CreateSpawnMarkers()
	{
		foreach (var spawnPos in _level.SpawnPositions)
		{
			var spawnSprite = new Sprite2d(_renderer, Assets.LoadTexture2d("Textures/enemy_spawn1.png"))
			{
				Layer = 5,
				LocalTransform = new Transform
				{
					Position = spawnPos,
					Scale = new Vector2f(_spawnSize, _spawnSize)
				}
			};

			_spawnMarkers.Add(spawnSprite);
			AddChild(spawnSprite);
		}
	}

	private void SpawnEnemy()
	{
		if (_level.SpawnPositions.Count == 0)
			return;

		var spawnPos = _level.SpawnPositions[_spawnIndex];
		_spawnIndex = (_spawnIndex + 1) % _level.SpawnPositions.Count;

		var path = _level.GetPathFromSpawn(spawnPos);
		var enemy = new Enemy(_renderer, path);
		enemy.ReachedBase += HandleEnemyReachedBase;
		enemy.Died += HandleEnemyDied;

		_activeEnemies.Add(enemy);
		AddChild(enemy);
		_spawnedCount++;
	}

	private void CleanupInactiveEnemies()
	{
		for (int i = _activeEnemies.Count - 1; i >= 0; i--)
		{
			if (!_activeEnemies[i].MarkedAsFree)
				continue;

			var enemy = _activeEnemies[i];
			enemy.ReachedBase -= HandleEnemyReachedBase;
			enemy.Died -= HandleEnemyDied;
			_activeEnemies.RemoveAt(i);
		}
	}

	private void HandleEnemyReachedBase(Enemy enemy)
	{
		EnemyReachedBase?.Invoke(enemy);

		enemy.ReachedBase -= HandleEnemyReachedBase;
		enemy.Died -= HandleEnemyDied;
		_activeEnemies.Remove(enemy);
	}

	private void HandleEnemyDied(Enemy enemy)
	{
		enemy.ReachedBase -= HandleEnemyReachedBase;
		enemy.Died -= HandleEnemyDied;
		_activeEnemies.Remove(enemy);
	}
}
