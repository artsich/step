using OpenTK.Mathematics;
using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Engine.Collisions;

public class CircleCollisionShape : CollisionShape
{
	private readonly Renderer renderer;

	[EditorProperty(from: 0f, speed: 0.1f)]
	public float Radius { get; set; } = 1.0f;

	public CircleCollisionShape(Renderer renderer)
		: base(CollisionSystem.Ins)
	{
		Name = nameof(CircleCollisionShape);
		this.renderer = renderer;
	}

	protected override void OnRender()
	{
		if (Visible && IsActive)
		{
			var position = GetGlobalMatrix().ExtractTranslation().Xy;
			renderer.DrawCircle(
				position,
				Radius,
				new Color4<Rgba>(0f, 0.6f, 0.7f, 0.42f),
				layer: -1);
		}
	}

	protected override void OnDebugDraw()
	{
		base.OnDebugDraw();
		EditOf.Render(this);
	}

	public override bool CheckCollision(CollisionShape other)
	{
		if (!IsActive)
		{
			return false;
		}

		if (other is CircleCollisionShape otherCircle)
		{
			Vector2 p1 = GetGlobalMatrix().ExtractTranslation().Xy;
			Vector2 p2 = otherCircle.GetGlobalMatrix().ExtractTranslation().Xy;

			return CollisionHelpers.CircleVsCircle(p1, Radius, p2, otherCircle.Radius);
		}
		else if (other is RectangleShape2d otherRectangle)
		{
			Vector2 p1 = GetGlobalMatrix().ExtractTranslation().Xy;
			return CollisionHelpers.CircleVsAabb(p1, Radius, otherRectangle.Aabb);
		}

		return false;
	}
}
