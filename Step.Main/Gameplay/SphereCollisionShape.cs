using OpenTK.Mathematics;
using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay;

public class SphereCollisionShape : CollisionShape
{
	private readonly Renderer renderer;

	[EditorProperty(from: 0f, speed: 0.1f)]
	public float Radius { get; set; } = 1.0f;

	public SphereCollisionShape(Renderer renderer)
		: base(CollisionSystem.Ins)
	{
		Name = nameof(SphereCollisionShape);
		this.renderer = renderer;
	}

	protected override void OnRender()
	{
		if (Visible)
		{
			var position = GetGlobalMatrix().ExtractTranslation().Xy;
			renderer.DrawRect(
				position,
				new Vector2(Radius*2f),
				new Color4<Rgba>(0f, 0.6f, 0.7f, 0.42f),
				layer: -1);
		}
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}

	public override bool CheckCollision(CollisionShape other, out Vector2 mtv)
	{
		mtv = Vector2.Zero;
		if (!IsActive)
		{
			return false;
		}

		if (other is not SphereCollisionShape otherSphere)
			return false;

		float r1 = Radius;
		float r2 = otherSphere.Radius;

		Vector2 pos1 = GetGlobalMatrix().ExtractTranslation().Xy;
		Vector2 pos2 = otherSphere.GetGlobalMatrix().ExtractTranslation().Xy;

		Vector2 diff = pos2 - pos1;
		float distance = diff.Length;

		float radiiSum = r1 + r2;

		if (distance < radiiSum)
		{
			float penetrationDepth = radiiSum - distance;

			Vector2 direction = distance > 0.0001f ? diff.Normalized() : Vector2.UnitY;
			mtv = direction * penetrationDepth;

			return true;
		}

		return false;
	}
}
