using Step.Engine.Editor;

namespace Step.Engine;

public class Transform
{
	[Export(speed: 0.1f)]
	public Vector2f Position { get; set; }

	[Export(-MathF.PI, MathF.PI)]
	public float Rotation { get; set; }

	[Export(speed: 0.1f)]
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
