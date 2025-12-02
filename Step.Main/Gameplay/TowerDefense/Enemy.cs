using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Animations;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Enemy : GameObject
{
	private const float DefaultHealth = 2f;
	private const float HitScaleFactor = 1.15f;
	private const float HitDuration = 0.08f;

	private readonly IReadOnlyList<Vector2f> _path;
	private readonly float _moveSpeed;
	private readonly Health _health;
	private readonly Sprite2d _sprite;
	private readonly SpriteHitAnimation _hitAnimation;

	private bool _reachedBase;
	private bool _dead;
	private int _targetIndex;

	public event Action<Enemy>? ReachedBase;
	public event Action<Enemy>? Died;

	public bool IsAlive => !_dead && _health.CurrentHealth > 0f;

	public Enemy(
		Renderer renderer,
		IReadOnlyList<Vector2f> path,
		float moveSpeed = 25f,
		float maxHealth = DefaultHealth,
		Vector4f? color = null)
		: base(nameof(Enemy))
	{
		if (path.Count == 0)
			throw new ArgumentException("Path must contain at least one point.", nameof(path));

		_path = path;
		_moveSpeed = moveSpeed;
		_health = new Health(maxHealth);
		_targetIndex = Math.Min(1, path.Count - 1);

		_sprite = new Sprite2d(renderer, Assets.LoadTexture2d("Textures/spr_goblin.png"))
		{
			Layer = 7,
			Color = color ?? Vector4f.One,
			LocalTransform = new Transform
			{
				Scale = new Vector2f(18f, 18f)
			}
		};

		AddChild(_sprite);
		_hitAnimation = new SpriteHitAnimation(
			_sprite,
			_sprite.LocalTransform.Scale,
			_sprite.Color,
			Tweens,
			HitScaleFactor,
			HitDuration);

		GlobalPosition = _path[0];
	}

	public Enemy(
		Renderer renderer,
		IReadOnlyList<Vector2f> path,
		EnemyTypeConfig config)
		: this(renderer, path, config.MoveSpeed, config.Health, config.Color)
	{
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (_reachedBase || _dead)
			return;

		if (_targetIndex >= _path.Count)
		{
			ReachBase();
			return;
		}

		var target = _path[_targetIndex];
		var toTarget = target - GlobalPosition;
		float distanceSquared = (toTarget.X * toTarget.X) + (toTarget.Y * toTarget.Y);

		if (distanceSquared <= float.Epsilon)
		{
			AdvanceToNextTarget();
			return;
		}

		float distance = MathF.Sqrt(distanceSquared);
		float travelDistance = _moveSpeed * deltaTime;

		if (distance <= travelDistance)
		{
			GlobalPosition = target;
			AdvanceToNextTarget();
			return;
		}

		var direction = toTarget / distance;
		GlobalPosition += direction * travelDistance;
	}

	private void AdvanceToNextTarget()
	{
		if (_targetIndex >= _path.Count - 1)
		{
			ReachBase();
			return;
		}

		_targetIndex++;
	}

	public void ApplyDamage(float amount)
	{
		if (amount <= 0f || _dead || _reachedBase)
			return;

		_health.ApplyDamage(amount);
		_hitAnimation.Play();

		if (_health.CurrentHealth <= 0f)
		{
			Die();
		}
	}

	private void ReachBase()
	{
		if (_reachedBase)
			return;

		_reachedBase = true;
		_dead = true;
		ReachedBase?.Invoke(this);
		QueueFree();
	}

	private void Die()
	{
		if (_dead)
			return;

		_dead = true;
		Died?.Invoke(this);
		QueueFree();
	}

}


