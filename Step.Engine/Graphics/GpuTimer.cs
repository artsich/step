using OpenTK.Graphics.OpenGL;

namespace Step.Engine.Graphics;

public class GpuTimer : IDisposable
{
	private readonly int[] _queryObjects = new int[2];
	private int _currentQueryIndex;
	private bool _isRunning;
	private bool _disposed;
	private readonly float[] _cache = [0f, 0f];

	public GpuTimer()
	{
		GL.CreateQueries(QueryTarget.TimeElapsed, 2, _queryObjects);
		_currentQueryIndex = 0;
		_isRunning = false;
		_disposed = false;
	}

	public void Start()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (_isRunning)
			throw new InvalidOperationException("GpuTimer is already running.");

		GL.BeginQuery(QueryTarget.TimeElapsed, _queryObjects[_currentQueryIndex]);
		_isRunning = true;
	}

	public float Stop()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (!_isRunning)
			throw new InvalidOperationException("GpuTimer is not running.");

		GL.EndQuery(QueryTarget.TimeElapsed);
		_isRunning = false;

		int previousQueryIndex = 1 - _currentQueryIndex;

		GL.GetQueryObjecti(
			_queryObjects[previousQueryIndex],
			QueryObjectParameterName.QueryResultAvailable,
			out int available);

		if (available == 0)
		{
			return _cache[_currentQueryIndex];
		}

		GL.GetQueryObjectui64(
			_queryObjects[previousQueryIndex],
			QueryObjectParameterName.QueryResult,
			out ulong nanoseconds);

		_cache[previousQueryIndex] = nanoseconds / 1_000_000f;
		_currentQueryIndex = previousQueryIndex;
		return _cache[previousQueryIndex];
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		if (_isRunning)
		{
			GL.EndQuery(QueryTarget.TimeElapsed);
			_isRunning = false;
		}

		GL.DeleteQueries(2, _queryObjects);
		_disposed = true;
	}
}
