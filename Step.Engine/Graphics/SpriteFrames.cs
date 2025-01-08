namespace Step.Engine.Graphics;

public sealed class SpriteFrames(
	string name,
	bool loop,
	float speed,
	Texture2d atlas,
	Rect[] frames)
{
	private readonly float _timePerFrame = 1f / speed;
	private float _time;
	private int _currentFrame;

	public string Name => name;

	public Texture2d Atlas => atlas;

	public bool IsFinished { get; private set; }

	public void Reset()
	{
		_time = 0;
		_currentFrame = 0;
		IsFinished = false;
	}

	public void Update(float dt)
	{
		if (IsFinished)
		{
			return;
		}

		_time += dt;
		if (_time > _timePerFrame)
		{
			_time = 0f;
			_currentFrame++;

			if (_currentFrame >= frames.Length)
			{
				_currentFrame = 0;

				if (!loop)
				{
					IsFinished = true;
				}
			}
		}
	}

	public Rect GetCurrentRect()
	{
		return frames[_currentFrame];
	}
}
