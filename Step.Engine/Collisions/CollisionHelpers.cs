﻿namespace Step.Engine.Collisions;

public static class CollisionHelpers
{
	public static CollisionInfo CircleVsCircle(Vector2f p1, float r1, Vector2f p2, float r2)
	{
		Vector2f diff = p2 - p1;
		float distance = diff.Length;
		float sumRadius = r1 + r2;

		if (distance >= sumRadius)
			return CollisionInfo.None;

		Vector2f normal = distance < 0.0001f 
			? Vector2f.UnitX 
			: diff / distance;

		return new CollisionInfo(true, normal, sumRadius - distance);
	}

	public static CollisionInfo CircleVsAabb(Vector2f circlePos, float radius, Box2f aabb)
	{
		Vector2f closestPoint = Vector2.Clamp(circlePos, aabb.Min, aabb.Max);
		Vector2f diff = circlePos - closestPoint;
		float distanceSquared = diff.LengthSquared;

		if (distanceSquared > radius * radius)
			return CollisionInfo.None;

		float distance = MathF.Sqrt(distanceSquared);
		
		if (distance < 0.0001f)
		{
			Vector2f aabbCenter = (aabb.Min + aabb.Max) * 0.5f;
			Vector2f aabbHalfSize = (aabb.Max - aabb.Min) * 0.5f;
			Vector2f circleToCenter = circlePos - aabbCenter;
			
			float xRatio = MathF.Abs(circleToCenter.X) / aabbHalfSize.X;
			float yRatio = MathF.Abs(circleToCenter.Y) / aabbHalfSize.Y;

			if (xRatio > yRatio)
			{
				float sign = MathF.Sign(circleToCenter.X);
				return new CollisionInfo
				{
					HasCollision = true,
					Normal = new Vector2f(sign, 0),
					Penetration = radius
				};
			}
			else
			{
				float sign = MathF.Sign(circleToCenter.Y);
				return new CollisionInfo
				{
					HasCollision = true,
					Normal = new Vector2f(0, sign),
					Penetration = radius
				};
			}
		}

		return new CollisionInfo
		{
			HasCollision = true,
			Normal = diff / distance,
			Penetration = radius - distance
		};
	}

	public static CollisionInfo AabbVsAabb(Box2f a, Box2f b)
	{
		float overlapX = MathF.Min(a.Max.X, b.Max.X) - MathF.Max(a.Min.X, b.Min.X);
		float overlapY = MathF.Min(a.Max.Y, b.Max.Y) - MathF.Max(a.Min.Y, b.Min.Y);

		if (overlapX <= 0 || overlapY <= 0)
			return CollisionInfo.None;

		Vector2f normal;
		float penetration;

		if (overlapX < overlapY)
		{
			float sign = (a.Center.X < b.Center.X) ? -1 : 1;
			normal = new Vector2f(sign, 0);
			penetration = overlapX;
		}
		else
		{
			float sign = (a.Center.Y < b.Center.Y) ? -1 : 1;
			normal = new Vector2f(0, sign);
			penetration = overlapY;
		}

		return new CollisionInfo
		{
			HasCollision = true,
			Normal = normal,
			Penetration = penetration
		};
	}
}
