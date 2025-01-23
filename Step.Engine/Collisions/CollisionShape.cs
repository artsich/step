using Step.Engine.Editor;

namespace Step.Engine.Collisions;

public abstract class CollisionShape(CollisionSystem collisionSystem) : GameObject
{
	[EditorProperty]
	public bool Visible { get; set; }

	public event Action<CollisionShape>? OnCollision;

	[EditorProperty]
	public bool IsStatic { get; set; }

	[EditorProperty]
	public bool IsActive { get; set; } = true;

	public int CollisionMask { get; set; }

	public int CollisionLayers { get; set; }

	public int MaxCollisionsPerFrame { get; set; } = 32;

	private int _collisionsThisFrame;

	public abstract bool CheckCollision(CollisionShape other);

	public bool CollidableWith(CollisionShape other)
	{
		return (CollisionMask & other.CollisionLayers) != 0;
	}

	protected override void OnStart()
	{
		collisionSystem.Register(this);
	}

	protected override void OnEnd()
	{
		collisionSystem.Unregister(this);
	}

	internal void ResetCollisionsCount()
	{
		_collisionsThisFrame = 0;
	}

	internal void RaiseCollision(CollisionShape other)
	{
		if (_collisionsThisFrame < MaxCollisionsPerFrame)
		{
			_collisionsThisFrame++;
			OnCollision?.Invoke(other);
		}
	}
}
