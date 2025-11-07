using Step.Engine;
using Step.Engine.Collisions;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Tron;

public sealed class TrailSegment : GameObject
{
	private readonly Renderer _renderer;
	private readonly RectangleShape2d _shape;
	private readonly float _thickness;

	private Vector2f _start;
	private Vector2f _end;
	private Vector2f _dir;

	public Vector4f Color { get; set; } = new(0.8f, 0.7f, 0.2f, 1f);

	public TrailSegment(Renderer renderer, float thickness)
	{
		_renderer = renderer;
		_thickness = thickness;
		_shape = new RectangleShape2d(renderer)
		{
			Visible = false,
			IsStatic = true,
			CollisionLayers = (int)PhysicLayers.Trail,
			CollisionMask = (int)PhysicLayers.Player,
		};
		AddChild(_shape);
	}

	public void BeginAt(Vector2f start, Vector2f direction)
	{
		_start = start;
		_end = start;
		_dir = direction;
		UpdateShapeAndTransform();
	}

	public void ExtendTo(Vector2f end)
	{
		_end = end;
		UpdateShapeAndTransform();
	}

	public float GetLength()
	{
		if (Math.Abs(_dir.X) > 0f)
		{
			return Math.Abs(_end.X - _start.X);
		}
		return Math.Abs(_end.Y - _start.Y);
	}

	public float TrimFromStart(float delta)
	{
		float length = GetLength();
		float cut = MathF.Min(delta, length);
		if (cut <= 0f)
		{
			return 0f;
		}

		if (Math.Abs(_dir.X) > 0f)
		{
			var sign = Math.Sign(_end.X - _start.X);
			_start.X += sign * cut;
		}
		else
		{
			var sign = Math.Sign(_end.Y - _start.Y);
			_start.Y += sign * cut;
		}

		UpdateShapeAndTransform();
		return cut;
	}

	protected override void OnRender()
	{
		var size = GetSize();
		_renderer.DrawRect(GetCenter(), size, Color, layer: 1);
	}

	private void UpdateShapeAndTransform()
	{
		_shape.Size = GetSize();
		_shape.LocalTransform.Position = GetCenter();
	}

	private Vector2f GetSize()
	{
		if (Math.Abs(_dir.X) > 0f)
		{
			var width = Math.Max(Math.Abs(_end.X - _start.X), 0.0001f);
			return new Vector2f(width, _thickness);
		}
		var height = Math.Max(Math.Abs(_end.Y - _start.Y), 0.0001f);
		return new Vector2f(_thickness, height);
	}

	private Vector2f GetCenter()
	{
		return new Vector2f((_start.X + _end.X) * 0.5f, (_start.Y + _end.Y) * 0.5f);
	}
}


