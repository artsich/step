using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Spawns : GameObject
{
	private readonly Renderer _renderer;
	private readonly Level _level;
	private readonly List<Sprite2d> _spawnMarkers = [];
	private readonly List<Enemy> _activeEnemies = [];
	private readonly float _spawnSize = 25f;
	private readonly IReadOnlyList<WaveConfig> _waves;
	private readonly Random _random = new();

	private float _spawnTimer;
	private int _spawnedCount;
	private int _spawnIndex;
	private int _currentWaveIndex;
	private bool _waveActive;

	public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;

	public bool WaveInProgress => _waveActive;

	public int CurrentWaveNumber => _currentWaveIndex + 1;
	public int TotalWaves => _waves.Count;
	public bool AreAllWavesCompleted => _currentWaveIndex >= _waves.Count;

	public event Action<Enemy>? EnemyReachedBase;
	public event Action<Enemy>? EnemyDied;
	public event Action? WaveCompleted;
	public event Action? AllWavesCompleted;

	public Spawns(Renderer renderer, Level level) : base(nameof(Spawns))
	{
		_renderer = renderer;
		_level = level;
		_waves = level.Waves;

		CreateSpawnMarkers();
	}

	protected override void OnUpdate(float deltaTime)
	{
		CleanupInactiveEnemies();
		TryCompleteWave();

		if (!_waveActive)
			return;

		if (_currentWaveIndex >= _waves.Count)
			return;

		var currentWave = _waves[_currentWaveIndex];

		if (_spawnedCount >= currentWave.TotalEnemyCount)
			return;

		_spawnTimer += deltaTime;

		while (_spawnTimer >= currentWave.SpawnIntervalSeconds && _spawnedCount < currentWave.TotalEnemyCount)
		{
			_spawnTimer -= currentWave.SpawnIntervalSeconds;
			SpawnEnemy(currentWave);
		}

		TryCompleteWave();
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

	private void SpawnEnemy(WaveConfig wave)
	{
		if (_level.SpawnPositions.Count == 0)
			return;

		var spawnPos = _level.SpawnPositions[_spawnIndex];
		_spawnIndex = (_spawnIndex + 1) % _level.SpawnPositions.Count;

		var path = _level.GetPathFromSpawn(spawnPos);
		var enemy = CreateEnemyFromWave(wave, path);
		
		_activeEnemies.Add(enemy);
		AddChild(enemy);
		_spawnedCount++;
	}

	private Enemy CreateEnemyFromWave(WaveConfig wave, IReadOnlyList<Vector2f> path)
	{
		var enemyType = wave.SelectRandomEnemyType(_random);
		var enemyConfig = EnemyTypeConfig.GetDefault(enemyType);
		var enemy = new Enemy(_renderer, path, enemyConfig);
		enemy.ReachedBase += HandleEnemyReachedBase;
		enemy.Died += HandleEnemyDied;
		return enemy;
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
		EnemyDied?.Invoke(enemy);

		enemy.ReachedBase -= HandleEnemyReachedBase;
		enemy.Died -= HandleEnemyDied;
		_activeEnemies.Remove(enemy);
	}

	public void StartWave()
	{
		if (_waveActive)
			return;

		if (_activeEnemies.Count > 0)
		{
			Serilog.Log.Warning("Cannot start a new wave while enemies are still active.");
			return;
		}

		if (AreAllWavesCompleted)
		{
			Serilog.Log.Warning("All waves completed. Cannot start a new wave.");
			return;
		}

		ResetWaveState();
		_waveActive = true;
	}

	public void StopWave()
	{
		if (!_waveActive)
			return;

		_waveActive = false;
	}

	private void ResetWaveState()
	{
		_spawnTimer = 0f;
		_spawnedCount = 0;
		_spawnIndex = 0;
	}

	private void TryCompleteWave()
	{
		if (!_waveActive)
			return;

		if (_currentWaveIndex >= _waves.Count)
			return;

		var currentWave = _waves[_currentWaveIndex];

		if (!IsWaveReadyToComplete(currentWave))
			return;

		CompleteCurrentWave();
	}

	private bool IsWaveReadyToComplete(WaveConfig currentWave)
	{
		return _spawnedCount >= currentWave.TotalEnemyCount && _activeEnemies.Count == 0;
	}

	private void CompleteCurrentWave()
	{
		_waveActive = false;
		_currentWaveIndex++;

		if (_currentWaveIndex >= _waves.Count)
		{
			AllWavesCompleted?.Invoke();
		}
		else
		{
			WaveCompleted?.Invoke();
		}
	}
}
