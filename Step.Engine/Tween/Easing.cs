namespace Step.Engine.Tween;

public static class Easing
{
	public static float Linear(float t) => t;

	public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

	public static float EaseIn(float t) => t * t;

	public static float EaseOutBack(float t)
	{
		const float c1 = 1.70158f;
		const float c3 = c1 + 1f;
		var nt = t - 1f;
		return 1f + c3 * nt * nt * nt + c1 * nt * nt;
	}
}
