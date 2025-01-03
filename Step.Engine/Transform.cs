using OpenTK.Mathematics;
using Step.Engine.Editor;

namespace Step.Engine;

public class Transform
{
	[EditorProperty(speed: 0.1f)]
	public Vector2 Position { get; set; }

	[EditorProperty(-MathF.PI, MathF.PI)]
	public float Rotation { get; set; }

	[EditorProperty(speed: 0.1f)]
	public Vector2 Scale { get; set; }

	public Transform()
	{
		Position = Vector2.Zero;
		Rotation = 0f;
		Scale = Vector2.One;
	}

	public Matrix4 GetLocalMatrix()
	{
		var matScale = Scale.CreateScale();
		var matRot = Rotation.CreateRotationZ();
		var matTrans = Position.CreateTranslation();

		return matScale * matRot * matTrans;
	}
}
