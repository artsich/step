using Step.Engine;
using Step.Engine.Editor;
using Step.Main.Gameplay.Actors;

namespace Step.Main.Gameplay;

public sealed class Spawner(Box2f spawnArea, SpawnRule[] spawnRules) : GameObject(nameof(Spawner))
{
	[EditorProperty]
	public float InitialEntitiesPerSecond { get; set; } = 1f;

	[EditorProperty]
	public float SpawnRateIncreaseInterval { get; set; } = 10f;

	[EditorProperty]
	public float SpawnRateIncreaseFactor { get; set; } = 0.5f;

	[EditorProperty]
	public float InitialEnemySpeed { get; set; } = 30f;

	[EditorProperty]
	public float SpeedIncreaseInterval { get; set; } = 10f;

	[EditorProperty]
	public float SpeedIncreaseFactor { get; set; } = 5f;

	[EditorProperty]
	public bool On { get; set; } = true;

	public event Action<GameObject>? OnSpawn;

	private float _timeSinceLastSpawn;
	private float _timeSinceLastIncrease;
	private float _timeSinceLastSpeedIncrease;
	private float _timeSinceStart;
	private float _currentSpawnRate;
	private float _currentEnemySpeedMultiplier;

	private readonly Random _random = new();

	protected override void OnStart()
	{
		_currentSpawnRate = InitialEntitiesPerSecond;
		_currentEnemySpeedMultiplier = InitialEnemySpeed;
	}

	protected override void OnDebugDraw()
	{
		base.OnDebugDraw();
		EditOf.Render(this);
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (!On)
		{
			return;
		}

		_timeSinceStart += deltaTime;
		_timeSinceLastSpawn += deltaTime;
		_timeSinceLastIncrease += deltaTime;
		_timeSinceLastSpeedIncrease += deltaTime;

		if (_timeSinceLastIncrease >= SpawnRateIncreaseInterval)
		{
			_currentSpawnRate += SpawnRateIncreaseFactor;
			_timeSinceLastIncrease = 0;
		}

		if (_timeSinceLastSpeedIncrease >= SpeedIncreaseInterval)
		{
			_currentEnemySpeedMultiplier += SpeedIncreaseFactor;
			_timeSinceLastSpeedIncrease = 0;
		}

		if (_timeSinceLastSpawn >= 1f / _currentSpawnRate)
		{
			SpawnEnemy();
			_timeSinceLastSpawn = 0;
		}

		TryRemoveChild();
	}

	private void TryRemoveChild()
	{
		float margin = 50f;
		var removalBox = new Box2f(
			spawnArea.Min - new Vector2f(margin),
			spawnArea.Max + new Vector2f(margin)
		);

		foreach (var enemy in children)
		{
			if (!removalBox.Contains(enemy.GlobalPosition))
			{
				enemy.QueueFree();
			}
		}
	}

	private void SpawnEnemy()
	{
		var availableRules = spawnRules
			.Where(rule => _timeSinceStart >= rule.StartTime)
			.ToList();

		if (availableRules.Count == 0)
			return;

		float totalProbability = availableRules.Sum(rule => rule.SpawnWeight);
		float randomValue = (float)_random.NextDouble() * totalProbability;

		float accumulatedProbability = 0f;
		foreach (var rule in availableRules)
		{
			accumulatedProbability += rule.SpawnWeight;
			if (randomValue <= accumulatedProbability)
			{
				var spawnPos = GenerateSpawnPosition(rule);

				var enemy = rule.CreateEntity(new EnemySpawnDetails(spawnPos, _currentEnemySpeedMultiplier));
			
				OnSpawn?.Invoke(enemy);

				CallDeferred(() => {
					AddChild(enemy);
					enemy.Start();
				});
				break;
			}
		}
	}

	private Vector2f GenerateSpawnPosition(SpawnRule rule)
	{
		if (rule.SpawnLocation == SpawnLocationType.Interior)
		{
			return new Vector2f(
				(float)_random.NextDouble() * spawnArea.Size.X + spawnArea.Min.X,
				(float)_random.NextDouble() * spawnArea.Size.Y + spawnArea.Min.Y
			);
		}

		float margin = 50f;
		var expandedBox = new Box2f(
			spawnArea.Min - new Vector2f(margin),
			spawnArea.Max + new Vector2f(margin)
		);

		float perimeter = 2 * (expandedBox.Size.X + expandedBox.Size.Y);
		float randomPoint = (float)_random.NextDouble() * perimeter;

		if (randomPoint < expandedBox.Size.X) // Top edge
		{
			return new Vector2f(expandedBox.Min.X + randomPoint, expandedBox.Min.Y);
		}
		randomPoint -= expandedBox.Size.X;

		if (randomPoint < expandedBox.Size.Y)
		{
			return new Vector2f(expandedBox.Max.X, expandedBox.Min.Y + randomPoint);
		}
		randomPoint -= expandedBox.Size.Y;

		if (randomPoint < expandedBox.Size.X) // Bottom edge
		{
			return new Vector2f(expandedBox.Max.X - randomPoint, expandedBox.Max.Y);
		}
		randomPoint -= expandedBox.Size.X;

		// Left edge
		return new Vector2f(expandedBox.Min.X, expandedBox.Max.Y - randomPoint);
	}
}