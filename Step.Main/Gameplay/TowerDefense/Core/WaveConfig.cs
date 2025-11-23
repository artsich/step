using System.Linq;

namespace Step.Main.Gameplay.TowerDefense.Core;

public readonly struct EnemyTypeWeight(EnemyType type, float weight)
{
	public EnemyType Type { get; } = type;
	public float Weight { get; } = weight;
}

public readonly struct WaveConfig
{
	private IReadOnlyList<EnemyTypeWeight> EnemyTypes { get; }
	
	public int TotalEnemyCount { get; }
	
	public float SpawnIntervalSeconds { get; }

	public WaveConfig(
		IReadOnlyList<EnemyTypeWeight> enemyTypes,
		int totalEnemyCount,
		float spawnIntervalSeconds)
	{
		if (enemyTypes == null || enemyTypes.Count == 0)
			throw new ArgumentException("Enemy types list cannot be null or empty.", nameof(enemyTypes));

		if (totalEnemyCount <= 0)
			throw new ArgumentOutOfRangeException(nameof(totalEnemyCount), "Total enemy count must be greater than zero.");

		if (spawnIntervalSeconds <= 0f || float.IsNaN(spawnIntervalSeconds) || float.IsInfinity(spawnIntervalSeconds))
			throw new ArgumentOutOfRangeException(nameof(spawnIntervalSeconds), "Spawn interval must be a finite value greater than zero.");

		EnemyTypes = enemyTypes;
		TotalEnemyCount = totalEnemyCount;
		SpawnIntervalSeconds = spawnIntervalSeconds;
	}

	public EnemyType SelectRandomEnemyType(Random random)
	{
		var totalWeight = EnemyTypes.Sum(et => et.Weight);
		var randomValue = random.NextSingle() * totalWeight;

		var currentWeight = 0f;
		foreach (var enemyType in EnemyTypes)
		{
			currentWeight += enemyType.Weight;
			if (randomValue <= currentWeight)
				return enemyType.Type;
		}

		return EnemyTypes[^1].Type;
	}
}

