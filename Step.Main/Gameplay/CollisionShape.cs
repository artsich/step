using OpenTK.Mathematics;
using Step.Engine;
using Step.Engine.Editor;

namespace Step.Main.Gameplay;

public abstract class CollisionShape : GameObject
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
	private readonly CollisionSystem _collisionSystem;

	protected CollisionShape(CollisionSystem collisionSystem)
	{
		_collisionSystem = collisionSystem;
		_collisionSystem.Register(this);
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

	public abstract bool CheckCollision(CollisionShape other, out Vector2 mtv);
}
