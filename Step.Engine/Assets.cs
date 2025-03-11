using Silk.NET.OpenGL;
using Step.Engine.Audio;
using Step.Engine.Converters;
using Step.Engine.Graphics;
using Step.Engine.Graphics.Particles;
using System.Text.Json;

namespace Step.Engine;

public class Assets
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true,
		Converters =
		{
			new Vector2JsonConverter(),
			new Vector4JsonConverter()
		}
	};

	public const string AssetsFolder = ".\\Assets\\";

	public static Emitter LoadEmitter(string path)
	{
		var loadedEmitter = JsonSerializer.Deserialize<Emitter>(
		File.ReadAllText(FullPath(path)), JsonOptions);

		return loadedEmitter ?? throw new InvalidOperationException("Fail during particles loading..");
	}

	public static void SaveEmitter(string path, Emitter emitter)
	{
		var emitterJson = JsonSerializer.Serialize(emitter, JsonOptions);
		File.WriteAllText(FullPath(path), emitterJson);
	}

	public static Texture2d LoadTexture2d(string path)
	{
		return new Texture2d(FullPath(path)).Load();
	}

	public static Sound LoadSound(string path)
	{
		return NAudioLoader.LoadSound(FullPath(path));
	}

	public static string FullPath(string path) => Path.Combine(AssetsFolder, path);
}
