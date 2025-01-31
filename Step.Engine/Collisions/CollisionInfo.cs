using OpenTK.Mathematics;

namespace Step.Engine.Collisions;

public readonly record struct CollisionInfo(
	bool HasCollision,
	Vector2 Normal,
	float Penetration)
{
	public static CollisionInfo None => new() { HasCollision = false };
}
