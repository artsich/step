using Serilog;
using Silk.NET.OpenAL;

namespace Step.Engine.Audio;

public unsafe class AudioManager : IDisposable
{
	private readonly ALContext _alc;
	private readonly AL _al;

	private Device* _device;
	private Context* _context;

	private readonly Dictionary<string, Sound> _loadedSounds = [];

	private static AudioManager? _instance;

	public static AudioManager Ins => _instance ??= new AudioManager();

	public static AL Al => _instance!._al;

	private AudioManager()
	{
		_alc = ALContext.GetApi();
		_al = AL.GetApi();

		_device = _alc.OpenDevice(null);
		if (_device == null)
		{
			Log.Logger.Warning("Failed to open the default OpenAL device.");
			return;
		}

		_context = _alc.CreateContext(_device, null);
		if (_context == null)
		{
			Log.Logger.Warning("Failed to create an OpenAL audio context.");
			return;
		}

		_alc.MakeContextCurrent(_context);
		PrintOpenALInfo();
	}

	public void SetMasterVolume(float volume)
	{
		volume = Math.Clamp(volume, 0.0f, 1.0f);
		_al.SetListenerProperty(ListenerFloat.Gain, volume);
	}

	public void SlowDown(float pitch)
	{
		foreach (var (_, sound) in _loadedSounds)
		{
			sound.SlowDown(pitch);
		}
	}

	public void LoadSound(string key, string filePath)
	{
		if (_loadedSounds.ContainsKey(key))
		{
			Log.Logger.Warning($"AudioManager: Sound with key '{key}' already exists.");
		}

		Sound sound = Assets.LoadSound(filePath);
		_loadedSounds[key] = sound;
	}

	public void PlaySound(string key, bool loop = false)
	{
		if (_loadedSounds.TryGetValue(key, out var sound))
		{
			sound.Play(loop);
		}
		else
		{
			Log.Logger.Error($"AudioManager: Sound with key '{key}' not found.");
		}
	}

	public void PauseSound(string key)
	{
		if (_loadedSounds.TryGetValue(key, out var sound))
		{
			sound.Pause();
		}
	}

	public void StopSound(string key)
	{
		if (_loadedSounds.TryGetValue(key, out var sound))
		{
			sound.Stop();
		}
	}

	public void UnloadSound(string key)
	{
		if (_loadedSounds.TryGetValue(key, out var sound))
		{
			sound.Dispose();
			_loadedSounds.Remove(key);
		}
	}

	public void UnloadSounds()
	{
		foreach (var sound in _loadedSounds.Values)
		{
			sound.Dispose();
		}
		_loadedSounds.Clear();
	}

	public void Dispose()
	{
		if (_instance == null)
		{
			return;
		}

		UnloadSounds();

		_alc.MakeContextCurrent(null);
		if (_context != null)
		{
			_alc.DestroyContext(_context);
			_context = null;
		}

		if (_device != null)
		{
			_alc.CloseDevice(_device);
			_device = null;
		}

		_instance = null;
	}

	private void PrintOpenALInfo()
	{
		// Assumes an AL context is already current.
		string vendor = _al.GetStateProperty(StateString.Vendor) ?? "Vendor not found...";
		string renderer = _al.GetStateProperty(StateString.Renderer) ?? "Renderer not found...";
		string version = _al.GetStateProperty(StateString.Version) ?? "OpenAL version not found...";
		string extensionsStr = _al.GetStateProperty(StateString.Extensions) ?? "Extensions not found...";

		var extensions = extensionsStr.Split(' ');
		extensionsStr = string.Join('\n', extensions);

		Log.Logger.Information("---------------------------------------");
		Log.Logger.Information("OpenAL Information:");
		Log.Logger.Information($"Vendor:   {vendor}");
		Log.Logger.Information($"Renderer: {renderer}");
		Log.Logger.Information($"Version:  {version}");
		Log.Logger.Information($"Extensions:\n{extensionsStr}");
		Log.Logger.Information("---------------------------------------");
	}
}
