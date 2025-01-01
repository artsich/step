using OpenTK.Mathematics;

namespace Step.Main;

public static class StepMath
{
	public static Vector2 AdjustToAspect(float targetAspect, Vector2 size)
	{
		float aspect = size.X / size.Y;

		Vector2 result = Vector2.One;
		if (aspect > targetAspect)
		{
			result.Y = size.Y;
			result.X = size.Y * targetAspect;
		}
		else
		{
			result.X = size.X;
			result.Y = size.X / targetAspect;
		}

		return result;
	}
}
