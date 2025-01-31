using Step.Engine.Collisions;

namespace Step.Engine.Physics;

public class StaticBody2d : GameObject
{
	public StaticBody2d(CollisionShape collisionShape)
	: base(nameof(KinematicBody2D))
	{
		collisionShape.IsStatic = true;
		AddChild(collisionShape);
	}
}
