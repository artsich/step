namespace Step.Engine;

public static class MatrixExtensions
{
	public static Vector3f ExtractTranslation(this Matrix4f mat)
	{
		return mat.Row4.Xyz();
	}

	public static Vector3f ExtractScale(this Matrix4f mat)
	{
		return new Vector3f(mat.Row1.Xyz().Length, mat.Row2.Xyz().Length, mat.Row3.Xyz().Length);
	}

	public static Matrix4f Inverted(this Matrix4f mat)
	{
		var isInverted = Matrix4.Invert(mat, out var result);
		return isInverted ? result : Matrix4f.Identity;
	}
}

public static class VectorExtensions
{
	public static Vector2f Xy(this Vector4f a)
	{
		return new Vector2f(a.X, a.Y);
	}

	public static Vector3f Xyz(this Vector4f a)
	{
		return new Vector3f(a.X, a.Y, a.Z);
	}

	public static Vector2f Xy(this Vector3f a)
	{
		return new Vector2f(a.X, a.Y);
	}

	public static Matrix4f CreateTranslation(this Vector2f pos)
	{
		return Matrix4.CreateTranslation(pos.X, pos.Y, 0f);
	}

	public static Matrix4f CreateScale(this Vector2f scale)
	{
		return Matrix4.CreateScale(scale.X, scale.Y, 1f);
	}

	public static Matrix4f CreateRotationZ(this float angleRadians)
	{
		return Matrix4.CreateRotationZ(angleRadians);
	}

	public static Vector3f To3(this Vector2f vec, float z = 0f)
	{
		return new Vector3f(vec.X, vec.Y, z);
	}

	public static Vector2f FromSystem(this System.Numerics.Vector2 vector)
	{
		return new (vector.X, vector.Y);
	}

	//public static System.Numerics.Vector2 ToSystem(this Vector2f vector)
	//{
	//	return new (vector.X, vector.Y);
	//}

	public static Vector4f FromSystem(this System.Numerics.Vector4 vector)
	{
		return new(vector.X, vector.Y, vector.Z, vector.W);
	}

	public static System.Numerics.Vector4 ToSystem(this Vector4f vector)
	{
		return new(vector.X, vector.Y, vector.Z, vector.W);
	}

	public static Vector2f MoveToward(this Vector2f from, Vector2f to, float dt)
	{
		var direction = from.DirectionTo(to);
		return from + direction * dt;
	}

	public static Vector2f DirectionTo(this Vector2f from, Vector2f to)
	{
		return Vector2.Normalize(to - from);
	}
}
