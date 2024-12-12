using OpenTK.Mathematics;

namespace Step.Main;

public class Spawner(IReadOnlyList<Vector2> spawnPoints, Vector2 thingSize, float timeInterval)
{
	private static readonly Random Random = new(25512);

	private float timeElapsed = 0f;

	public Thing? Get(float dt)
	{
		timeElapsed += dt;

		if (timeElapsed > timeInterval)
		{
			timeElapsed = 0f;
			var index = Random.Next(spawnPoints.Count);
			var position = spawnPoints[index];
			return new Thing(position, thingSize);
		}

		return null;
	}
}