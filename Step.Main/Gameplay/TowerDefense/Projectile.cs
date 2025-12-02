using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Projectile : GameObject
{
	private const float DefaultSize = 6f;

	private readonly Enemy _target;
	private readonly float _damage;
	private readonly float _speed;

	public Projectile(
		Renderer renderer,
		Enemy target,
		float damage,
		float speed) : base(nameof(Projectile))
	{
		ArgumentNullException.ThrowIfNull(renderer);
		_target = target ?? throw new ArgumentNullException(nameof(target));
		_damage = damage;
		_speed = speed;

		var sprite = new Sprite2d(renderer, Assets.LoadTexture2d("Textures/tower_fire.png"))
		{
			Layer = 9
		};
		sprite.LocalTransform.Scale = new Vector2f(DefaultSize, DefaultSize);

		AddChild(sprite);
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (deltaTime <= 0f)
			return;

		if (!IsTargetValid())
		{
			QueueFree();
			return;
		}

		var targetPos = _target.GlobalPosition;
		var toTarget = targetPos - GlobalPosition;
		float distanceSquared = (toTarget.X * toTarget.X) + (toTarget.Y * toTarget.Y);

		if (distanceSquared <= float.Epsilon)
		{
			HitTarget();
			return;
		}

		float distance = MathF.Sqrt(distanceSquared);
		float travelDistance = _speed * deltaTime;
		var direction = new Vector2f(toTarget.X / distance, toTarget.Y / distance);
		LocalTransform.Rotation = MathF.Atan2(direction.Y, direction.X);

		if (travelDistance >= distance)
		{
			GlobalPosition = targetPos;
			HitTarget();
			return;
		}

		GlobalPosition += direction * travelDistance;
	}

	private bool IsTargetValid()
	{
		return !_target.MarkedAsFree && _target.IsAlive;
	}

	private void HitTarget()
	{
		_target.ApplyDamage(_damage);
		QueueFree();
	}
}

