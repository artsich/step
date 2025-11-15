using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Enemy : GameObject
{
	private readonly IReadOnlyList<Vector2f> _path;
	private readonly float _moveSpeed;
	private bool _reachedBase;
	private int _targetIndex;

	public event Action<Enemy>? ReachedBase;

	public Enemy(Renderer renderer, IReadOnlyList<Vector2f> path, float moveSpeed = 25f)
		: base(nameof(Enemy))
	{
		if (path.Count == 0)
			throw new ArgumentException("Path must contain at least one point.", nameof(path));

		_path = path;
		_moveSpeed = moveSpeed;
		_targetIndex = Math.Min(1, path.Count - 1);

		var sprite = new Sprite2d(renderer, Assets.LoadTexture2d("Textures\\spr_goblin.png"))
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
		if (_reachedBase)
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

	private void ReachBase()
	{
		if (_reachedBase)
			return;

		_reachedBase = true;
		ReachedBase?.Invoke(this);
		QueueFree();
	}
}


