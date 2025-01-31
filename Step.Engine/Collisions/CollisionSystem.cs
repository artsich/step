using System.Diagnostics;

namespace Step.Engine.Collisions;

public class CollisionSystem
{
	private static readonly CollisionSystem _instance = new();

	public static CollisionSystem Ins => _instance;

	private readonly List<CollisionShape> _shapes = [];

	private CollisionSystem() { }

	public int Count => _shapes.Count;

	public T Register<T>(T shape) where T : CollisionShape
	{
		if (!_shapes.Contains(shape))
		{
			_shapes.Add(shape);
		}

		return shape;
	}

	public void Unregister(CollisionShape shape)
	{
		_shapes.Remove(shape);
	}

	public void Process()
	{
		foreach (var shape in _shapes)
		{
			shape.ResetCollisionsCount();
		}

		for (int i = 0; i < _shapes.Count; i++)
		{
			for (int j = i + 1; j < _shapes.Count; j++)
			{
				var shapeA = _shapes[i];
				var shapeB = _shapes[j];

				if (!shapeA.IsActive || !shapeB.IsActive)
					continue;

				if (!shapeA.CollidableWith(shapeB) && !shapeB.CollidableWith(shapeA))
					continue;

				CollisionInfo info = shapeA.CheckCollision(shapeB);
				if (info.HasCollision)
				{
					ResolveCollision(shapeA, shapeB, info);
				}
			}
		}
	}

	public void Reset()
	{
		_shapes.Clear();
	}

	private static void ResolveCollision(CollisionShape shapeA, CollisionShape shapeB, CollisionInfo info)
	{
		var aWithB = shapeA.CollidableWith(shapeB);
		var bWithA = shapeB.CollidableWith(shapeA);

		if (aWithB && bWithA)
		{
			ResolveCollisionOverlap(shapeA, shapeB, info);
		}

		if (aWithB)
		{
			shapeA.RaiseCollision(shapeB, info);
		}

		if (bWithA)
		{
			shapeB.RaiseCollision(
				shapeA,
				info with
				{ 
					Normal = -info.Normal 
				});
		}
	}

	private static void ResolveCollisionOverlap(CollisionShape shapeA, CollisionShape shapeB, CollisionInfo info)
	{
		Debug.Assert(shapeA.Parent != null, "Shape A must have a parent.");
		Debug.Assert(shapeB.Parent != null, "Shape B must have a parent.");

		var a = shapeA.Parent!;
		var b = shapeB.Parent!;

		if (!shapeA.IsStatic && !shapeB.IsStatic)
		{
			a.GlobalPosition += info.Normal * info.Penetration * 0.5f;
			b.GlobalPosition -= info.Normal * info.Penetration * 0.5f;
		}
		else if (!shapeA.IsStatic)
		{
			a.GlobalPosition += info.Normal * info.Penetration;
		}
		else if (!shapeB.IsStatic)
		{
			b.GlobalPosition -= info.Normal * info.Penetration;
		}
	}
}
