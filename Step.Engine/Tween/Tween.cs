namespace Step.Engine.Tween;

public static class Tween
{
	public static FloatTween Float(float from, float to, float duration, Action<float> setter, EasingFunc? easing = null)
		=> new(from, to, duration, setter, easing);

	public static IntervalTween Interval(float durationSeconds) => new(durationSeconds);

	public static CallbackTween Callback(Action action) => new(action);

	public static TweenSequence Sequence(params ITween[] steps) => new(steps);
}

