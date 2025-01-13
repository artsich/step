using OpenTK.Mathematics;

namespace Step.Engine.Collisions;

public static class CollisionHelpers
{
	public static bool CircleVsCircle(Vector2 p1, float r1, Vector2 p2, float r2)
	{
		Vector2 diff = p2 - p1;
		float distance = diff.Length;
		return distance < (r1 + r2);
	}

	public static bool CircleVsAabb(Vector2 p1, float r1, Box2 aabb)
	{
		Vector2 closestPoint = Vector2.Clamp(p1, aabb.Min, aabb.Max);

		Vector2 difference = p1 - closestPoint;
		float distanceSquared = difference.LengthSquared;

		return distanceSquared <= r1 * r1;
	}
}
