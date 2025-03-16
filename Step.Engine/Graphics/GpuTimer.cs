using Silk.NET.OpenGL;

namespace Step.Engine.Graphics;

public sealed class GpuTimer : IDisposable
{
	private readonly uint[] _queryObjects = new uint[2];
	private int _currentQueryIndex;
	private bool _isRunning;
	private bool _disposed;
	private readonly float[] _cache = [0f, 0f];

	private readonly GL _gl;

	public GpuTimer(GL gl)
	{
		_gl = gl;

		_gl.CreateQueries(QueryTarget.TimeElapsed, 2, _queryObjects);
		_currentQueryIndex = 0;
		_isRunning = false;
		_disposed = false;
	}

	public void Start()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (_isRunning)
			throw new InvalidOperationException("GpuTimer is already running.");

		_gl.BeginQuery(QueryTarget.TimeElapsed, _queryObjects[_currentQueryIndex]);
		_isRunning = true;
	}

	public float Stop()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (!_isRunning)
			throw new InvalidOperationException("GpuTimer is not running.");

		_gl.EndQuery(QueryTarget.TimeElapsed);
		_isRunning = false;

		int previousQueryIndex = 1 - _currentQueryIndex;

		_gl.GetQueryObject(
			_queryObjects[previousQueryIndex],
			QueryObjectParameterName.ResultAvailable,
			out int available);

		if (available == 0)
		{
			return _cache[_currentQueryIndex];
		}

		_gl.GetQueryObject(
			_queryObjects[previousQueryIndex],
			QueryObjectParameterName.Result,
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
			_gl.EndQuery(QueryTarget.TimeElapsed);
			_isRunning = false;
		}

		_gl.DeleteQueries(2, _queryObjects);
		_disposed = true;
	}
}