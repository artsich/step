using Step.Engine.Collisions;

namespace Step.Engine.Physics;

public sealed class StaticBody2d : GameObject
{
	public StaticBody2d(CollisionShape collisionShape)
	: base(nameof(StaticBody2d))
	{
		collisionShape.IsStatic = true;
		AddChild(collisionShape);
	}
}
