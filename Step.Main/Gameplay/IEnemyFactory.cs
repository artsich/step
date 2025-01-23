using OpenTK.Mathematics;

namespace Step.Main.Gameplay;

public interface IEnemyFactory
{
	GliderEntity CreateGlider(Vector2 position);

	CircleEnemy CreateCircle(Vector2 position);
}
