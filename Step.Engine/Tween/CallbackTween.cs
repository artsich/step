namespace Step.Engine.Tween;

public sealed class CallbackTween : ITween
{
	private readonly Action _callback;
	private bool _invoked;

	public CallbackTween(Action callback)
	{
		_callback = callback ?? throw new ArgumentNullException(nameof(callback));
	}

	public bool IsFinished => _invoked;

	public void Update(float deltaTime)
	{
		if (_invoked)
		{
			return;
		}

		_callback();
		_invoked = true;
	}
}

