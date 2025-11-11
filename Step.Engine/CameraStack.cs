using Step.Engine.Graphics;

namespace Step.Engine;

public sealed class CameraStack
{
	private readonly Stack<ICamera2d> _cameras = new();

	public ICamera2d? Current => _cameras.TryPeek(out var camera) ? camera : null;

	public void Push(ICamera2d camera)
	{
		_cameras.Push(camera);
	}

	public void Pop()
	{
		if (_cameras.Count == 0)
		{
			throw new InvalidOperationException("Camera stack is empty");
		}

		_cameras.Pop();
	}
}
