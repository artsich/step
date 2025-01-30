using OpenTK.Mathematics;
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

	public GliderEntity CreateGlider(Vector2 position)
	{
		var glider = new GliderEntity(target)
		{
			Name = $"Glider_{NewId}",
			LocalTransform = new Transform()
			{
				Position = position,
				Scale = new Vector2(0.3f),
			}
		};

		glider.AddChild(
			new RectangleShape2d(renderer)
			{
				Size = new Vector2(16f, 16f),
				CollisionLayers = (int)PhysicLayers.Enemy
			}
		);

		glider.AddChild(
			new Sprite2d(renderer, gliderTexture)
			{
				Color = Colors.Glider,
			}
		);

		return glider;
	}

	public CircleEnemy CreateCircle(Vector2 spawnPosition)
	{
		var targetDir = (target.Position - spawnPosition).Normalized();

		var circle = new CircleEnemy(targetDir)
		{
			Name = $"Circle_{NewId}",
			LocalTransform = new Transform()
			{
				Position = spawnPosition,
				Scale = new Vector2(0.3f),
			}
		};

		circle.AddChild(
			new RectangleShape2d(renderer)
			{
				Size = new Vector2(16f, 16f),
				CollisionLayers = (int)PhysicLayers.Enemy
			}
		);

		circle.AddChild(
			new Sprite2d(renderer, circleTexture)
			{
				GType = GeometryType.Circle,
				Color = Colors.Circle,
			}
		);

		return circle;
	}

	public CrossEnemy CreateCross(Vector2 position)
	{
		var cross = new CrossEnemy()
		{
			Name = $"Cross_{NewId}",
			LocalTransform = new Transform()
			{
				Position = position,
				Scale = new Vector2(0.3f),
			}
		};

		cross.AddChild(
			new RectangleShape2d(renderer)
			{
				Size = new Vector2(16f),
				CollisionLayers = (int)PhysicLayers.Enemy
			}
		);

		cross.AddChild(
			new Sprite2d(renderer, crossTexture)
			{
				Color = Colors.Cross,
			}
		);

		return cross;
	}
}
