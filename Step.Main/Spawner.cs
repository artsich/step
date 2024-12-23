using OpenTK.Mathematics;

namespace Step.Main;

public abstract class SpawnEntity(
	float probability,
	Func<IGameScene, bool> condition,
	Func<Vector2, IGameScene, Thing> createEntity)
{
	public float Probability { get; } = probability;

	public Func<IGameScene, bool> Condition { get; } = condition;

	public Func<Vector2, IGameScene, Thing> CreateEntity { get; } = createEntity;
}

public sealed class SpawnSpeedEntity(Texture2d texture) : SpawnEntity(
		0.5f,
		(gs) => true,
		(pos, gs) => new Thing(pos, new Vector2(20), new SpeedEffect(gs.Player))
		{
			Texture = texture
		}
	)
{
}

public sealed class SpawnSimpleEntity(Texture2d texture) : SpawnEntity(
		0.9f,
		(gs) => true,
		(pos, gs) => new Thing(pos, new Vector2(20, 20))
		{
			Texture = texture
		}
	)
{
}

public sealed class SpanwHealthEntity(Texture2d texture) : SpawnEntity(
		0.2f,
		(gs) => !gs.Player.IsFullHp && gs.Player.EffectsCount<HealEffect>() <= gs.Player.MaxHp,
		(pos, gs) => new Thing(pos, new Vector2(20, 20), new HealEffect(1, gs.Player))
		{
			Texture = texture
		}
	)
{
}

public sealed class SpawnKillAllEntity(Texture2d texture) : SpawnEntity(
	0.1f,
	(_) => true,
	(pos, gs) => new Thing(pos, new Vector2(20, 20), new KillAllEffect(gs))
	{
		Texture = texture
	}
)
{
}

public class Spawner(
	IReadOnlyList<Vector2> spawnPoints,
	IGameScene gameScene,
	float timeInterval,
	params SpawnEntity[] spawnEntities
) {
	private static readonly Random Random = new(25512);
	private readonly List<SpawnEntity> spawnEntities = [.. spawnEntities.OrderBy(x => x.Probability)];
	private float timeElapsed = 0f;

	public float Speed { get; set; } = 60f;

	public float TimeInterval { get; set; } = timeInterval;

	public Thing? Get(float dt)
	{
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
					return entity.CreateEntity(position, gameScene);
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
