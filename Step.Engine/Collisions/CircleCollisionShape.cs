using OpenTK.Mathematics;
using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Engine.Collisions;

public class CircleCollisionShape : CollisionShape
{
	private readonly Renderer _renderer;

	[EditorProperty(from: 0f, speed: 0.1f)]
	public float Radius { get; set; } = 1.0f;

	public CircleCollisionShape(Renderer renderer)
	{
		Name = nameof(CircleCollisionShape);
		_renderer = renderer;
	}

	public override CollisionInfo CheckCollision(CollisionShape other)
	{
		if (!IsActive)
			return CollisionInfo.None;

		if (other is CircleCollisionShape otherCircle)
		{
			return CollisionHelpers.CircleVsCircle(
				GlobalPosition, 
				Radius,
				otherCircle.GlobalPosition, 
				otherCircle.Radius);
		}
		
		if (other is RectangleShape2d otherRectangle)
		{
			return CollisionHelpers.CircleVsAabb(
				GlobalPosition,
				Radius,
				otherRectangle.Aabb);
		}

		return CollisionInfo.None;
	}

	protected override void OnRender()
	{
		if (Visible && IsActive)
		{
			_renderer.DrawCircle(
				GlobalPosition,
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
}
