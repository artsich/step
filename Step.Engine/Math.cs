namespace Step.Engine;

public static class StepMath
{
	public static float LerpAngle(float from, float to, float t)
	{
		float diff = NormalizeAngle(to - from);
		return from + diff * t;
	}

	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	public static float NormalizeAngle(float angle)
	{
		return MathF.IEEERemainder(angle, MathF.PI * 2);
	}

	public static Vector2f AdjustToAspect(float targetAspect, Vector2f size)
	{
		float aspect = size.X / size.Y;

		Vector2f result = Vector2f.One;
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
