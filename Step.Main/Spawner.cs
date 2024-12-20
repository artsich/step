using OpenTK.Mathematics;

namespace Step.Main;

public class Spawner
{
	private static readonly Random Random = new(25512);
	private readonly IReadOnlyList<Vector2> spawnPoints;
	private readonly Vector2 thingSize;
	private readonly IGameScene gameScene;
	private float timeElapsed = 0f;

	public float Speed { get; set; } = 60f;

	public float TimeInterval { get; set; }

	private Texture2d _healthEffect;
	private Texture2d _bombEffect;
	private Texture2d _justThing;

	public Spawner(
		IReadOnlyList<Vector2> spawnPoints,
		Vector2 thingSize,
		float timeInterval,
		IGameScene gameScene)
	{
		this.spawnPoints = spawnPoints;
		this.thingSize = thingSize;
		this.gameScene = gameScene;
		TimeInterval = timeInterval;

		_healthEffect = new Texture2d("Assets/Textures/effect_health.png").Load();
		_bombEffect = new Texture2d("Assets/Textures/effect_bomb.png").Load();
		_justThing  = new Texture2d("Assets/Textures/thing.png").Load();
	}

	public Thing? Get(float dt)
	{
		timeElapsed += dt;
		Thing? result = null;

		if (timeElapsed > TimeInterval)
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
					Speed = Speed,
					Texture = _justThing,
				},
				1 => new Thing(position, thingSize, new HealEffect(1, gameScene.Player))
				{ 
					Color = new Color4<Rgba>(0.45f, 0.29f, 0.27f, 1f),
					Speed = Speed,
					Texture = _healthEffect,
				},
				2 => new Thing(position, thingSize, new KillAllEffect(gameScene))
				{ 
					Color = new Color4<Rgba>(0.21f, 0.24f, 0.26f, 1f),
					Speed = Speed,
					Texture = _bombEffect,
				},
				_ => null
			};
		}

		return result;
	}
}