namespace Step.Engine.Collisions;

public class CollisionSystem
{
	private readonly static CollisionSystem _instance = new();

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
				var a = _shapes[i];
				var b = _shapes[j];

				var aWithB = a.CollidableWith(b);
				var bWithA = b.CollidableWith(a);

				if ((aWithB || bWithA) && a.CheckCollision(b))
				{
					if (aWithB)
						a.RaiseCollision(b);

					if (bWithA)
						b.RaiseCollision(a);
				}
			}
		}
	}

	public void Reset()
	{
		_shapes.Clear();
	}
}
