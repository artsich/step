using OpenTK.Mathematics;

namespace Step.Main.Gameplay.Spawn;

public class Spawner(
	IReadOnlyList<Vector2> spawnPoints,
	float timeInterval,
	params SpawnEntity[] spawnEntities)
{
	private static readonly Random Random = new(25512);
	private readonly List<SpawnEntity> spawnEntities = [.. spawnEntities.OrderBy(x => x.Probability)];
	private float timeElapsed = 0f;

	public float Speed { get; set; } = 60f;

	public float TimeInterval { get; set; } = timeInterval;
	public bool Enabled { get; set; }

	public Thing? Get(float dt, IGameScene gameScene)
	{
		if (!Enabled)
		{
			return null;
		}

		timeElapsed += dt;

		if (timeElapsed > TimeInterval)
		{
			timeElapsed = 0f;
			Vector2 position = GetSpawnPoint();

			var validEntities = spawnEntities
				.Where(e => e.Condition(gameScene))
				.ToList();

			if (validEntities.Count == 0) return null;

			var totalProbability = validEntities.Sum(e => e.Probability);
			var roll = Random.NextDouble() * totalProbability;

			float cumulative = 0f;
			foreach (var entity in validEntities)
			{
				cumulative += entity.Probability;
				if (roll < cumulative)
				{
					var instance = entity.CreateEntity(position, gameScene);
					return instance;
				}
			}
		}

		return null;
	}

	private Vector2 GetSpawnPoint()
	{
		var index = Random.Next(spawnPoints.Count);
		var position = spawnPoints[index];
		return position;
	}
}
