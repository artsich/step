namespace Step.Main.Gameplay.InputUtils;

internal class MouseActivityTracker
{
	private Vector2f _lastMousePosition;
	private bool _wasMouseMoving;

	public bool TryActivate(Vector2f currentMousePos, float threshold = 0.1f)
	{
		var mouseDelta = (currentMousePos - _lastMousePosition).LengthSquared;
		if (mouseDelta > threshold)
		{
			_wasMouseMoving = true;
		}

		_lastMousePosition = currentMousePos;
		return _wasMouseMoving;
	}

	public void Reset()
	{
		_wasMouseMoving = false;
	}
}
