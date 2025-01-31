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
	{
		Name = nameof(RectangleShape2d);
		_renderer = renderer;
	}

	public override CollisionInfo CheckCollision(CollisionShape other)
	{
		if (!IsActive)
			return CollisionInfo.None;

		if (other is RectangleShape2d otherRect)
		{
			return CollisionHelpers.AabbVsAabb(Aabb, otherRect.Aabb);
		}
		
		if (other is CircleCollisionShape otherCircle)
		{
			var info = CollisionHelpers.CircleVsAabb(
				otherCircle.GlobalPosition,
				otherCircle.Radius,
				Aabb);
			
			return info with
			{
				Normal = -info.Normal,
			};
		}

		return CollisionInfo.None;
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
}
