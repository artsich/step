using Silk.NET.OpenAL;

namespace Step.Engine.Audio;

public class Sound : IDisposable
{
	private readonly uint _bufferId;
	private readonly AL _al;
	private readonly uint _sourceId;

	private bool _isDisposed;

	public Sound(uint bufferId, AL al)
	{
		_bufferId = bufferId;
		_al = al;
		_sourceId = _al.GenSource();
		_al.SetSourceProperty(_sourceId, SourceInteger.Buffer, _bufferId);
	}

	public void Play(bool loop = false)
	{
		if (_isDisposed) return;

		_al.SetSourceProperty(_sourceId, SourceBoolean.Looping, loop);
		_al.SourcePlay(_sourceId);
	}

	public void Pause()
	{
		if (_isDisposed) return;
		_al.SourcePause(_sourceId);
	}

	public void Stop()
	{
		if (_isDisposed) return;
		_al.SourceStop(_sourceId);
	}

	public void Dispose()
	{
		if (_isDisposed) return;
		_isDisposed = true;

		_al.SourceStop(_sourceId);
		_al.DeleteSource(_sourceId);
		_al.DeleteBuffer(_bufferId);
	}

	public void SlowDown(float pitch)
	{
		_al.SetSourceProperty(_sourceId, SourceFloat.Pitch, pitch);
	}
}
