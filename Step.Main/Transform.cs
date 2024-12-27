using OpenTK.Mathematics;

namespace Step.Main;

public class Transform
{
	public Vector2 Position;
	public float Rotation;
	public Vector2 Scale;

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
