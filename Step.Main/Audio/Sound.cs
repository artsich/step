using OpenTK.Audio.OpenAL;

namespace Step.Main.Audio;

internal class Sound : IDisposable
{
	private int _bufferId;
	private int _sourceId;

	private bool _isDisposed;

	internal Sound(int bufferId)
	{
		_bufferId = bufferId;

		// Generate a source to play this buffer
		_sourceId = AL.GenSource();
		AL.Source(_sourceId, ALSourcei.Buffer, _bufferId);
	}

	/// <summary>
	/// Plays the sound.
	/// </summary>
	public void Play(bool loop = false)
	{
		if (_isDisposed) return;

		AL.Source(_sourceId, ALSourceb.Looping, loop);
		AL.SourcePlay(_sourceId);
	}

	/// <summary>
	/// Pauses the sound.
	/// </summary>
	public void Pause()
	{
		if (_isDisposed) return;
		AL.SourcePause(_sourceId);
	}

	/// <summary>
	/// Stops the sound.
	/// </summary>
	public void Stop()
	{
		if (_isDisposed) return;
		AL.SourceStop(_sourceId);
	}

	/// <summary>
	/// Cleans up the underlying OpenAL resources.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed) return;
		_isDisposed = true;

		AL.SourceStop(_sourceId);
		AL.DeleteSource(_sourceId);
		AL.DeleteBuffer(_bufferId);
	}
}
