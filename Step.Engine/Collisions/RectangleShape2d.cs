using OpenTK.Mathematics;
using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Engine.Collisions;

public class RectangleShape2d : CollisionShape
{
	private readonly Renderer _renderer;

	[EditorProperty]
	public Vector2 Size { get; set; } = Vector2.One;

	public Box2 Aabb
	{
		get
		{
			var modelMat = GetGlobalMatrix();

			var pos = modelMat.ExtractTranslation().Xy;
			var scale = modelMat.ExtractScale().Xy;
			var scaledSize = Size * scale;

			var scaledSizeHalf = scaledSize / 2f;

			return new Box2(pos - scaledSizeHalf, pos + scaledSizeHalf);
		}
	}

	public RectangleShape2d(Renderer renderer)
		: base(CollisionSystem.Ins)
	{
		Name = nameof(RectangleShape2d);
		_renderer = renderer;
	}

	protected override void OnRender()
	{
		if (Visible)
		{
			var mat = GetGlobalMatrix();
			var scale = mat.ExtractScale().Xy;
			var scaledSize = Size * scale;

			var position = mat.ExtractTranslation().Xy;
			_renderer.DrawRect(
				position,
				scaledSize,
				new Color4<Rgba>(0f, 0.6f, 0.7f, 0.42f),
				layer: -1);
		}
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}

	public override bool CheckCollision(CollisionShape other)
	{
		if (!IsActive)
		{
			return false;
		}

		if (other is RectangleShape2d otherRect)
		{
			return Aabb.TouchWith(otherRect.Aabb);
		}
		else if (other is CircleCollisionShape otherCircle)
		{
			Vector2 p1 = otherCircle.GetGlobalMatrix().ExtractTranslation().Xy;
			return CollisionHelpers.CircleVsAabb(p1, otherCircle.Radius, Aabb);
		}

		return false;
	}
}
