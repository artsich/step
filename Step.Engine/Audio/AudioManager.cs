using OpenTK.Audio.OpenAL;

namespace Step.Engine.Audio;

public class AudioManager : IDisposable
{
	private ALDevice _device;
	private ALContext _context;

	private readonly Dictionary<string, Sound> _loadedSounds = [];

	private static AudioManager? _instance;

	public static AudioManager Ins => _instance ??= new AudioManager();

	private AudioManager()
	{
		_device = ALC.OpenDevice(null);
		if (_device == IntPtr.Zero)
			throw new Exception("Failed to open the default OpenAL device.");

		_context = ALC.CreateContext(_device, (int[]?)null);
		if (_context == IntPtr.Zero)
			throw new Exception("Failed to create an OpenAL audio context.");

		ALC.MakeContextCurrent(_context);

		PrintOpenALInfo();
	}

	public void SetMasterVolume(float volume)
	{
		volume = Math.Clamp(volume, 0.0f, 1.0f);
		AL.Listener(ALListenerf.Gain, volume);
	}

	public void LoadSound(string key, string filePath)
	{
		if (_loadedSounds.ContainsKey(key))
		{
			Console.WriteLine($"AudioManager: Sound with key '{key}' already exists.");
		}

		Sound sound = NAudioLoader.LoadSound(filePath);
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
			Console.WriteLine($"AudioManager: Sound with key '{key}' not found.");
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

	public void Dispose()
	{
		// Dispose all loaded sounds
		foreach (var sound in _loadedSounds.Values)
		{
			sound.Dispose();
		}
		_loadedSounds.Clear();

		ALC.MakeContextCurrent(ALContext.Null);
		if (_context != IntPtr.Zero)
		{
			ALC.DestroyContext(_context);
			_context = ALContext.Null;
		}

		if (_device != IntPtr.Zero)
		{
			ALC.CloseDevice(_device);
			_device = ALDevice.Null;
		}

		_instance = null;
	}

	private static void PrintOpenALInfo()
	{
		// Assumes an AL context is already current.
		string vendor = AL.Get(ALGetString.Vendor) ?? "Vendor not found...";
		string renderer = AL.Get(ALGetString.Renderer) ?? "Renderer not found...";
		string version = AL.Get(ALGetString.Version) ?? "OpenAL version not found...";
		string extensionsStr = AL.Get(ALGetString.Extensions) ?? "Extensions not found...";

		var extensions = extensionsStr.Split(' ');
		extensionsStr = string.Join('\n', extensions);

		Console.WriteLine("---------------------------------------");
		Console.WriteLine("OpenAL Information:");
		Console.WriteLine($"Vendor:   {vendor}");
		Console.WriteLine($"Renderer: {renderer}");
		Console.WriteLine($"Version:  {version}");
		Console.WriteLine($"Extensions:\n{extensionsStr}");
		Console.WriteLine("---------------------------------------");
	}
}
