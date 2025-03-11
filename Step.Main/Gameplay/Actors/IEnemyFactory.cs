namespace Step.Main.Gameplay.Actors;

public interface IEnemyFactory
{
	GliderEntity CreateGlider(Vector2f position);

	CircleEnemy CreateCircle(Vector2f position);

	CrossEnemy CreateCross(Vector2f position);
}
