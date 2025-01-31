using OpenTK.Mathematics;
using Step.Engine.Collisions;
using Step.Engine.Editor;

namespace Step.Engine.Physics;

public class KinematicBody2D : GameObject
{
	[EditorProperty]
	public Vector2 Velocity { get; set; }

	public KinematicBody2D(CollisionShape collisionShape)
		: base(nameof(KinematicBody2D))
	{
		AddChild(collisionShape);
	}

	protected override void OnUpdate(float dt)
	{
		GlobalPosition += Velocity * dt;
	}

	protected override void OnDebugDraw()
	{
		base.OnDebugDraw();
		EditOf.Render(this);
	}
}