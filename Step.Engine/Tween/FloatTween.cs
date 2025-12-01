using System.Diagnostics;

namespace Step.Engine.Tween;

public sealed class FloatTween : ITween
{
	private readonly float _from;
	private readonly float _to;
	private readonly float _duration;
	private readonly Action<float> _setter;
	private readonly EasingFunc _easing;
	private float _elapsed;
	private bool _hasAppliedInitial;

	public FloatTween(float from, float to, float duration, Action<float> setter, EasingFunc? easing = null)
	{
		Debug.Assert(setter != null);
		_from = from;
		_to = to;
		_duration = MathF.Max(0f, duration);
		_setter = setter ?? throw new ArgumentNullException(nameof(setter));
		_easing = easing ?? Easing.Linear;
	}

	public bool IsFinished { get; private set; }

	public void Update(float deltaTime)
	{
		if (IsFinished)
		{
			return;
		}

		if (!_hasAppliedInitial)
		{
			_setter(_from);
			_hasAppliedInitial = true;
			if (_duration <= 0f)
			{
				Complete();
				return;
			}
		}

		_elapsed += MathF.Max(0f, deltaTime);
		float t = MathF.Min(_elapsed / _duration, 1f);
		_setter(Interpolate(t));

		if (_elapsed >= _duration)
		{
			Complete();
		}
	}

	private float Interpolate(float t)
	{
		var eased = Math.Clamp(_easing(t), 0f, 1f);
		return _from + (_to - _from) * eased;
	}

	private void Complete()
	{
		_setter(_to);
		IsFinished = true;
	}
}

