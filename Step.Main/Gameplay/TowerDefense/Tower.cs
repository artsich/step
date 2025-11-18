using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Tower : GameObject
{
	private const float SpriteScaleFactor = 0.9f;
	private const float RangeMultiplier = 3.25f;

	private readonly Renderer _renderer;
	private readonly Func<IReadOnlyList<Enemy>> _enemyProvider;
	private readonly float _damage;
	private readonly float _shotsPerSecond;
	private readonly float _range;
	private readonly float _projectileSpeed;

	private float _cooldownTimer;

	public Tower(
		Renderer renderer,
		Vector2f position,
		float cellSize,
		Func<IReadOnlyList<Enemy>> enemyProvider,
		float damage = 1f,
		float shotsPerSecond = 1.5f,
		float projectileSpeed = 120f)
		: base(nameof(Tower))
	{
		_renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
		_enemyProvider = enemyProvider ?? throw new ArgumentNullException(nameof(enemyProvider));

		_damage = MathF.Max(0.01f, damage);
		_shotsPerSecond = MathF.Max(0.1f, shotsPerSecond);
		_projectileSpeed = MathF.Max(10f, projectileSpeed);
		_range = MathF.Max(cellSize * RangeMultiplier, cellSize);

		LocalTransform.Position = position;

		var sprite = new Sprite2d(_renderer, Assets.LoadTexture2d("Textures/spr_tower_lightning_tower.png"))
		{
			Layer = 8
		};
		var scaledSize = cellSize * SpriteScaleFactor;
		sprite.LocalTransform.Scale = new Vector2f(scaledSize, scaledSize);

		AddChild(sprite);
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (deltaTime <= 0f)
			return;

		if (_cooldownTimer > 0f)
		{
			_cooldownTimer -= deltaTime;
			return;
		}

		var target = AcquireTarget();
		if (target == null)
			return;

		Fire(target);
		_cooldownTimer = 1f / _shotsPerSecond;
	}

	private Enemy? AcquireTarget()
	{
		var enemies = _enemyProvider.Invoke();
		if (enemies == null || enemies.Count == 0)
			return null;

		Enemy? closest = null;
		float bestDistance = float.MaxValue;
		var towerPos = GlobalPosition;
		float rangeSquared = _range * _range;

		foreach (var enemy in enemies)
		{
			if (enemy == null || !enemy.IsAlive || enemy.MarkedAsFree)
				continue;

			float distanceSquared = DistanceSquared(towerPos, enemy.GlobalPosition);
			if (distanceSquared > rangeSquared || distanceSquared >= bestDistance)
				continue;

			bestDistance = distanceSquared;
			closest = enemy;
		}

		return closest;
	}

	private void Fire(Enemy target)
	{
		var projectile = new Projectile(
			_renderer,
			target,
			_damage,
			_projectileSpeed);

		AddChild(projectile);
		projectile.GlobalPosition = GlobalPosition;
	}

	private static float DistanceSquared(Vector2f a, Vector2f b)
	{
		float dx = a.X - b.X;
		float dy = a.Y - b.Y;
		return (dx * dx) + (dy * dy);
	}
}

