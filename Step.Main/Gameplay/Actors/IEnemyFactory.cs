using OpenTK.Mathematics;

namespace Step.Main.Gameplay.Actors;

public interface IEnemyFactory
{
	GliderEntity CreateGlider(Vector2 position);

	CircleEnemy CreateCircle(Vector2 position);

	CrossEnemy CreateCross(Vector2 position);
}
