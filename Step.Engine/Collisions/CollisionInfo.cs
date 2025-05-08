namespace Step.Engine.Collisions;

public readonly record struct CollisionInfo(
	bool HasCollision,
	Vector2f Normal,
	float Penetration,
	Vector2f Position)
{
	public static CollisionInfo None => new() { HasCollision = false };
}
