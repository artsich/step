using OpenTK.Mathematics;

namespace Step.Engine;

public static class VectorExtensions
{
	public static Matrix4 CreateTranslation(this Vector2 pos)
	{
		return Matrix4.CreateTranslation(pos.X, pos.Y, 0f);
	}

	public static Matrix4 CreateScale(this Vector2 scale)
	{
		return Matrix4.CreateScale(scale.X, scale.Y, 1f);
	}

	public static Matrix4 CreateRotationZ(this float angleRadians)
	{
		return Matrix4.CreateRotationZ(angleRadians);
	}

	public static Vector3 To3(this Vector2 vec, float z = 0f)
	{
		return new Vector3(vec.X, vec.Y, z);
	}

	public static Vector2 FromSystem(this System.Numerics.Vector2 vector)
	{
		return new (vector.X, vector.Y);
	}

	public static System.Numerics.Vector2 ToSystem(this Vector2 vector)
	{
		return new (vector.X, vector.Y);
	}

	public static Vector4 FromSystem(this System.Numerics.Vector4 vector)
	{
		return new(vector.X, vector.Y, vector.Z, vector.W);
	}

	public static System.Numerics.Vector4 ToSystem(this Vector4 vector)
	{
		return new(vector.X, vector.Y, vector.Z, vector.W);
	}
}
