namespace Step.Engine.Tween;

public sealed class TweenPlayer
{
	private readonly List<ITween> _tweens = [];

	public T Play<T>(T tween) where T : ITween
	{
		ArgumentNullException.ThrowIfNull(tween);
		_tweens.Add(tween);
		return tween;
	}

	public void Stop(ITween tween)
	{
		if (tween == null)
		{
			return;
		}

		var index = _tweens.IndexOf(tween);
		if (index >= 0)
		{
			_tweens.RemoveAt(index);
		}
	}

	public void Update(float deltaTime)
	{
		if (_tweens.Count == 0)
		{
			return;
		}

		for (int i = _tweens.Count - 1; i >= 0; i--)
		{
			var tween = _tweens[i];
			tween.Update(deltaTime);
			if (tween.IsFinished)
			{
				_tweens.RemoveAt(i);
			}
		}
	}

	public void Clear() => _tweens.Clear();
}

