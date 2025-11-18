using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Enemy : GameObject
{
	private const float DefaultHealth = 2f;

	private readonly IReadOnlyList<Vector2f> _path;
	private readonly float _moveSpeed;
	private readonly Health _health;

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
		float maxHealth = DefaultHealth)
		: base(nameof(Enemy))
	{
		if (path.Count == 0)
			throw new ArgumentException("Path must contain at least one point.", nameof(path));

		_path = path;
		_moveSpeed = moveSpeed;
		_health = new Health(maxHealth);
		_targetIndex = Math.Min(1, path.Count - 1);

		var sprite = new Sprite2d(renderer, Assets.LoadTexture2d("Textures/spr_goblin.png"))
		{
			Layer = 7,
			LocalTransform = new Transform
			{
				Scale = new Vector2f(18f, 18f)
			}
		};

		AddChild(sprite);

		GlobalPosition = _path[0];
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


