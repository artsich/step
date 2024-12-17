using OpenTK.Mathematics;

namespace Step.Main;

public class Spawner(
	IReadOnlyList<Vector2> spawnPoints,
	Vector2 thingSize,
	float timeInterval,
	IGameScene gameScene)
{
	private static readonly Random Random = new(25512);

	private float timeElapsed = 0f;

	public Thing? Get(float dt)
	{
		timeElapsed += dt;
		Thing? result = null;

		if (timeElapsed > timeInterval)
		{
			timeElapsed = 0f;
			var index = Random.Next(spawnPoints.Count);
			var position = spawnPoints[index];

			var thingId = Random.Next(3);
			var can = false;

			while (thingId != 0 && !can)
			{
				can = thingId != 1 || !gameScene.Player.IsFullHp;

				if (!can)
				{
					thingId = Random.Next(3);
				}
			}

			result = thingId switch
			{
				0 => new Thing(position, thingSize)
				{
					Color = new Color4<Rgba>(0.34f, 0.42f, 0.27f, 1f),
				},
				1 => new Thing(position, thingSize, new HealEffect(1, gameScene.Player))
				{ 
					Color = new Color4<Rgba>(0.45f, 0.29f, 0.27f, 1f) 
				},
				2 => new Thing(position, thingSize, new KillAllEffect(gameScene))
				{ 
					Color = new Color4<Rgba>(0.21f, 0.24f, 0.26f, 1f) 
				},
				_ => null
			};
		}

		return result;
	}
}