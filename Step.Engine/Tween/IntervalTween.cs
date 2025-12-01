namespace Step.Engine.Tween;

public sealed class IntervalTween : ITween
{
	private float _remaining;

	public IntervalTween(float durationSeconds)
	{
		_remaining = MathF.Max(0f, durationSeconds);
	}

	public bool IsFinished => _remaining <= 0f;

	public void Update(float deltaTime)
	{
		if (IsFinished)
		{
			return;
		}

		_remaining -= MathF.Max(0f, deltaTime);
	}
}

