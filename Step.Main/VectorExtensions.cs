using OpenTK.Mathematics;

namespace Step.Main;

public static class VectorExtensions
{
	public static Vector3 To3(this Vector2 vec, float z = 0f)
	{
		return new Vector3(vec.X, vec.Y, z);
	}
}
