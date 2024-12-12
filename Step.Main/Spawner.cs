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

		if (timeElapsed > timeInterval)
		{
			timeElapsed = 0f;
			var index = Random.Next(spawnPoints.Count);
			var position = spawnPoints[index];

			var thingId = Random.Next(3);
			var can = false;

			while(thingId != 0 && !can)
			{
				can = thingId != 1 || !gameScene.Player.IsFullHp;

				if (!can)
				{
					thingId = Random.Next(3);
				}
			}

			return thingId switch
			{
				0 => new Thing(position, thingSize),
				1 => new Thing(position, thingSize, new HealEffect(1, gameScene.Player)) { Color = Color4.Red },
				2 => new Thing(position, thingSize, new BombEffect(gameScene)) { Color = Color4.Navy },
				_ => null
			};
		}

		return null;
	}
}