using Step.Engine.Editor;

namespace Step.Engine.Collisions;

public abstract class CollisionShape : GameObject
{
	[EditorProperty]
	public bool Visible { get; set; }

	[EditorProperty]
	public bool IsStatic { get; set; }

	[EditorProperty]
	public bool IsActive { get; set; } = true;

	public int CollisionMask { get; set; }

	public int CollisionLayers { get; set; }

	public int MaxCollisionsPerFrame { get; set; } = 32;

	private int _collisionsThisFrame;

	public event Action<CollisionShape, CollisionInfo>? OnCollision;

	public abstract CollisionInfo CheckCollision(CollisionShape other);

	public bool CollidableWith(CollisionShape other)
	{
		return (CollisionMask & other.CollisionLayers) != 0;
	}

	protected override void OnStart()
	{
		CollisionSystem.Ins.Register(this);
	}

	protected override void OnEnd()
	{
		CollisionSystem.Ins.Unregister(this);
	}

	internal void ResetCollisionsCount()
	{
		_collisionsThisFrame = 0;
	}

	internal void RaiseCollision(CollisionShape other, CollisionInfo info)
	{
		if (_collisionsThisFrame < MaxCollisionsPerFrame)
		{
			_collisionsThisFrame++;
			OnCollision?.Invoke(other, info);
		}
	}
}
