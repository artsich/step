using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Editor;

namespace Step.Main.Gameplay;

public class Spawner(Box2 spawnArea, SpawnRule[] spawnRules) : GameObject(nameof(Spawner))
{
	[EditorProperty]
	public float InitialEntitiesPerSecond { get; set; } = 1f;

	[EditorProperty]
	public float SpawnRateIncreaseInterval { get; set; } = 10f;

	[EditorProperty]
	public float SpawnRateIncreaseFactor { get; set; } = 0.5f;

	private float _timeSinceLastSpawn;
	private float _timeSinceLastIncrease;
	private float _timeSinceStart;
	private float _currentSpawnRate;
	private readonly Random _random = new();

	protected override void OnStart()
	{
		_currentSpawnRate = InitialEntitiesPerSecond;
	}

	protected override void OnUpdate(float deltaTime)
	{
		_timeSinceStart += deltaTime;
		_timeSinceLastSpawn += deltaTime;
		_timeSinceLastIncrease += deltaTime;

		if (_timeSinceLastIncrease >= SpawnRateIncreaseInterval)
		{
			_currentSpawnRate += SpawnRateIncreaseFactor;
			_timeSinceLastIncrease = 0;
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
		var removalBox = new Box2(
			spawnArea.Min - new Vector2(margin),
			spawnArea.Max + new Vector2(margin)
		);

		foreach (var enemy in children)
		{
			if (!removalBox.ContainsInclusive(enemy.GlobalPosition))
			{
				CallDeferred(enemy.QueueFree);
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

		float totalProbability = availableRules.Sum(rule => rule.SpawnProbability);
		float randomValue = (float)_random.NextDouble() * totalProbability;

		float accumulatedProbability = 0f;
		foreach (var rule in availableRules)
		{
			accumulatedProbability += rule.SpawnProbability;
			if (randomValue <= accumulatedProbability)
			{
				var spawnPos = GenerateSpawnPosition(rule);

				var enemy = rule.CreateEntity(spawnPos);

				CallDeferred(() => AddChild(enemy));
				CallDeferred(enemy.Start);
				break;
			}
		}
	}

	private Vector2 GenerateSpawnPosition(SpawnRule rule)
	{
		if (rule.SpawnLocation == SpawnLocationType.Interior)
		{
			return new Vector2(
				(float)_random.NextDouble() * spawnArea.Size.X + spawnArea.Min.X,
				(float)_random.NextDouble() * spawnArea.Size.Y + spawnArea.Min.Y
			);
		}

		float margin = 50f;
		var expandedBox = new Box2(
			spawnArea.Min - new Vector2(margin),
			spawnArea.Max + new Vector2(margin)
		);

		float perimeter = 2 * (expandedBox.Size.X + expandedBox.Size.Y);
		float randomPoint = (float)_random.NextDouble() * perimeter;

		if (randomPoint < expandedBox.Size.X) // Top edge
		{
			return new Vector2(expandedBox.Min.X + randomPoint, expandedBox.Min.Y);
		}
		randomPoint -= expandedBox.Size.X;

		if (randomPoint < expandedBox.Size.Y)
		{
			return new Vector2(expandedBox.Max.X, expandedBox.Min.Y + randomPoint);
		}
		randomPoint -= expandedBox.Size.Y;

		if (randomPoint < expandedBox.Size.X) // Bottom edge
		{
			return new Vector2(expandedBox.Max.X - randomPoint, expandedBox.Max.Y);
		}
		randomPoint -= expandedBox.Size.X;

		// Left edge
		return new Vector2(expandedBox.Min.X, expandedBox.Max.Y - randomPoint);
	}
}