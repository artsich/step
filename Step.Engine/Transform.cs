using Step.Engine.Editor;

namespace Step.Engine;

public class Transform
{
	[EditorProperty(speed: 0.1f)]
	public Vector2f Position { get; set; }

	[EditorProperty(-MathF.PI, MathF.PI)]
	public float Rotation { get; set; }

	[EditorProperty(speed: 0.1f)]
	public Vector2f Scale { get; set; }

	public Transform()
	{
		Position = Vector2f.Zero;
		Rotation = 0f;
		Scale = Vector2f.One;
	}

	public Matrix4f GetLocalMatrix()
	{
		var matScale = Scale.CreateScale();
		var matRot = Rotation.CreateRotationZ();
		var matTrans = Position.CreateTranslation();

		return matScale * matRot * matTrans;
	}
}
