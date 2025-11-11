namespace Step.Engine;

public sealed class DeferredActionQueue
{
	private readonly Queue<Action> _actions = new();

	public void Enqueue(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		_actions.Enqueue(action);
	}

	public void Process()
	{
		while (_actions.TryDequeue(out var action))
		{
			action();
		}
	}
}
