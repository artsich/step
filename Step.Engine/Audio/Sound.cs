using OpenTK.Audio.OpenAL;

namespace Step.Engine.Audio;

public class Sound : IDisposable
{
	private readonly int _bufferId;
	private readonly int _sourceId;

	private bool _isDisposed;

	public Sound(int bufferId)
	{
		_bufferId = bufferId;

		_sourceId = AL.GenSource();
		AL.Source(_sourceId, ALSourcei.Buffer, _bufferId);
	}

	public void Play(bool loop = false)
	{
		if (_isDisposed) return;

		AL.Source(_sourceId, ALSourceb.Looping, loop);
		AL.SourcePlay(_sourceId);
	}

	public void Pause()
	{
		if (_isDisposed) return;
		AL.SourcePause(_sourceId);
	}

	public void Stop()
	{
		if (_isDisposed) return;
		AL.SourceStop(_sourceId);
	}

	public void Dispose()
	{
		if (_isDisposed) return;
		_isDisposed = true;

		AL.SourceStop(_sourceId);
		AL.DeleteSource(_sourceId);
		AL.DeleteBuffer(_bufferId);
	}
}
