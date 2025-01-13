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

	public int MaxCollisionsPerFrame { get; set; } = 32;

	private int _collisionsThisFrame;

	public abstract bool CheckCollision(CollisionShape other);

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
