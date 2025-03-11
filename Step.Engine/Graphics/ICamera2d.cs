namespace Step.Engine.Graphics;

public interface ICamera2d
{
	Matrix4f ViewProj { get; }
}

public static class CameraExt
{
	public static Vector2f ToClipSpace(this ICamera2d camera, Vector2f position)
	{
		var clipSpace = new Vector4f(position.X, position.Y, 0, 1) * camera.ViewProj;
		var pos = new Vector2f(
			(clipSpace.X / clipSpace.W + 1.0f) * 0.5f,
			(clipSpace.Y / clipSpace.W + 1.0f) * 0.5f);
		return pos;
	}

	public static Vector2f ScreenToWorld(this ICamera2d camera, Vector2f screenPos, Vector2f screenSize)
	{
		Vector2f normalizedScreenPos = new(
			screenPos.X / screenSize.X * 2 - 1,
			-(screenPos.Y / screenSize.Y * 2 - 1)
		);

		if (!Matrix4.Invert(camera.ViewProj, out var inverseViewProj))
		{
			throw new InvalidOperationException("");
		}
		return (new Vector4f(normalizedScreenPos, 0f, 1f) * inverseViewProj).Xy();
	}
}
