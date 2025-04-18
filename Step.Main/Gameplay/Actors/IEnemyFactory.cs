namespace Step.Main.Gameplay.Actors;

public record struct EnemySpawnDetails(Vector2f Position, float Speed);

public interface IEnemyFactory
{
	GliderEntity CreateGlider(EnemySpawnDetails spawnDetails);

	CircleEnemy CreateCircle(EnemySpawnDetails spawnDetails);

	CrossEnemy CreateCross(EnemySpawnDetails spawnDetails);
}
