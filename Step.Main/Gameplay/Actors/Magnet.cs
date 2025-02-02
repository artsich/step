using Step.Engine;
using Step.Engine.Collisions;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Actors;

public sealed class MagnetZone : GameObject
{
	private readonly List<CrossEnemy> _attractedEnemies = [];
	private readonly List<CrossEnemy> _activeEnemies = [];
	private readonly CircleCollisionShape _circleCollisionShape;

	public float Radius
	{
		get => _circleCollisionShape.Radius;
		set => _circleCollisionShape.Radius = value;
	}

	public MagnetZone(Renderer renderer)
		: base(nameof(MagnetZone))
	{
		_circleCollisionShape = new CircleCollisionShape(renderer)
		{
			CollisionLayers = (int)PhysicLayers.Magnet,
			CollisionMask = (int)PhysicLayers.Enemy,
			IsStatic = true
		};
		AddChild(_circleCollisionShape);
	}

	protected override void OnStart()
	{
		_circleCollisionShape.OnCollision += OnCollisionWithCross;
	}

	protected override void OnEnd()
	{
		foreach (var cross in _attractedEnemies)
		{
			cross.Unfollow();
		}
		_attractedEnemies.Clear();

		_circleCollisionShape.OnCollision -= OnCollisionWithCross;
	}

	protected override void OnUpdate(float deltaTime)
	{
		_activeEnemies.Clear();

		foreach (var cross in _attractedEnemies)
		{
			if (cross.MarkedAsFree)
				continue;

			var distance = (cross.GlobalPosition - GlobalPosition).Length;
			if (distance > _circleCollisionShape.Radius)
			{
				cross.Unfollow();
				continue;
			}

			_activeEnemies.Add(cross);
		}

		_attractedEnemies.Clear();
		_attractedEnemies.AddRange(_activeEnemies);
	}

	private void OnCollisionWithCross(CollisionShape shape, CollisionInfo _)
	{
		if (shape.Parent is CrossEnemy cross && !_attractedEnemies.Contains(cross))
		{
			var player = Parent!;
			cross.Follow(player);

			_attractedEnemies.Add(cross);
		}
	}
}
