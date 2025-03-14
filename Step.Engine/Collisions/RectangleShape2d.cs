﻿using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Engine.Collisions;

public sealed class RectangleShape2d : CollisionShape
{
	private readonly Renderer _renderer;

	[EditorProperty]
	public Vector2f Size { get; set; } = Vector2f.One;

	public Box2f Aabb
	{
		get
		{
			var modelMat = GetGlobalMatrix();
			var pos = modelMat.ExtractTranslation().Xy();
			var scale = modelMat.ExtractScale().Xy();
			var scaledSize = Size * scale;
			var scaledSizeHalf = scaledSize / 2f;
			return new Box2f(pos - scaledSizeHalf, pos + scaledSizeHalf);
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
		if (Visible && IsActive)
		{
			var mat = GetGlobalMatrix();
			var scale = mat.ExtractScale().Xy();
			var scaledSize = Size * scale;
			var position = mat.ExtractTranslation().Xy();
			_renderer.DrawRect(
				position,
				scaledSize,
				new Vector4f(0f, 0.6f, 0.7f, 0.42f),
				layer: 1000);
		}
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}
