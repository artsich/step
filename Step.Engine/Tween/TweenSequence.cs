namespace Step.Engine.Tween;

public sealed class TweenSequence : ITween
{
	private readonly Queue<ITween> _steps = new();
	private ITween? _current;

	public TweenSequence(IEnumerable<ITween> steps)
	{
		foreach (var step in steps)
		{
			_steps.Enqueue(step);
		}

		_current = _steps.Count > 0 ? _steps.Dequeue() : null;
	}

	public bool IsFinished => _current == null;

	public void Update(float deltaTime)
	{
		if (_current == null)
		{
			return;
		}

		_current.Update(deltaTime);

		while (_current != null && _current.IsFinished)
		{
			_current = _steps.Count > 0 ? _steps.Dequeue() : null;
			if (_current == null)
			{
				break;
			}

			_current.Update(0f);
		}
	}
}

