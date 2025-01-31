using Step.Engine.Collisions;
using Step.Engine.Graphics;
using Step.Main.Gameplay.Actors;

namespace Step.Main.Gameplay;

public class MagnetZone : CircleCollisionShape
{
	private readonly List<CrossEnemy> _attractedEnemies = [];
	private readonly List<CrossEnemy> _activeEnemies = [];

	public MagnetZone(Renderer renderer) : base(renderer)
	{
		Name = nameof(MagnetZone);

		CollisionLayers = (int)PhysicLayers.Magnet;
		CollisionMask = (int)PhysicLayers.Enemy;
		IsStatic = true;
	}

	protected override void OnStart()
	{
		base.OnStart();
		OnCollision += OnCollisionWithCross;
	}

	protected override void OnEnd()
	{
		base.OnStart();

		foreach(var cross in _attractedEnemies)
		{
			cross.Unfollow();
		}
		_attractedEnemies.Clear();

		OnCollision -= OnCollisionWithCross;
	}

	protected override void OnUpdate(float deltaTime)
	{
		_activeEnemies.Clear();

		foreach (var cross in _attractedEnemies)
		{
			if (cross.MarkedAsFree)
				continue;

			var distance = (cross.GlobalPosition - GlobalPosition).Length;
			if (distance > Radius)
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
