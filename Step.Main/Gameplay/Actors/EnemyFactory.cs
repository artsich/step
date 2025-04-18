using Step.Engine;
using Step.Engine.Collisions;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Actors;

public class EnemyFactory(
	Renderer renderer,
	Texture2d gliderTexture,
	Texture2d circleTexture,
	Texture2d crossTexture,
	ITarget target
) : IEnemyFactory
{
	private int _enemyId = 0;

	private int NewId => _enemyId++;

	public GliderEntity CreateGlider(EnemySpawnDetails spawnDetails)
	{
		var glider = new GliderEntity(target)
		{
			Name = $"Glider_{NewId}",
			LocalTransform = new Transform()
			{
				Position = spawnDetails.Position,
				Scale = new Vector2f(0.3f),
			},
			Speed = spawnDetails.Speed
		};

		glider.AddChild(
			new RectangleShape2d(renderer)
			{
				Size = new Vector2f(16f, 16f),
				CollisionLayers = (int)PhysicLayers.Enemy
			}
		);

		glider.AddChild(
			new Sprite2d(renderer, gliderTexture)
			{
				Color = Constants.GameColors.Glider,
			}
		);

		return glider;
	}

	public CircleEnemy CreateCircle(EnemySpawnDetails spawnDetails)
	{
		var targetDir = spawnDetails.Position.DirectionTo(target.Position);

		var circle = new CircleEnemy(targetDir)
		{
			Name = $"Circle_{NewId}",
			LocalTransform = new Transform()
			{
				Position = spawnDetails.Position,
				Scale = new Vector2f(0.3f),
			},
			Speed = spawnDetails.Speed
		};

		circle.AddChild(
			new RectangleShape2d(renderer)
			{
				Size = new Vector2f(16f, 16f),
				CollisionLayers = (int)PhysicLayers.Enemy
			}
		);

		circle.AddChild(
			new Sprite2d(renderer, circleTexture)
			{
				GType = GeometryType.Circle,
				Color = Constants.GameColors.Circle,
			}
		);

		return circle;
	}

	public CrossEnemy CreateCross(EnemySpawnDetails spawnDetails)
	{
		var cross = new CrossEnemy()
		{
			Name = $"Cross_{NewId}",
			LocalTransform = new Transform()
			{
				Position = spawnDetails.Position,
				Scale = new Vector2f(0.3f),
			},
		};

		cross.AddChild(
			new RectangleShape2d(renderer)
			{
				Size = new Vector2f(16f),
				CollisionLayers = (int)PhysicLayers.Enemy
			}
		);

		cross.AddChild(
			new Sprite2d(renderer, crossTexture)
			{
				Color = Constants.GameColors.Cross,
			}
		);

		return cross;
	}
}
