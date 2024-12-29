using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using Step.Main.Converters;
using Step.Main.Graphics;
using Step.Main.Graphics.Particles;
using System.Text.Json;

namespace Step.Main.Editor;

public sealed class ParticlesEditor : IEditorView, IDisposable
{
	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true,
		Converters =
		{
			new Vector2JsonConverter(),
			new Vector4JsonConverter()
		}
	};
	private readonly string _baseFolder;
	private readonly RenderTarget2d _particlesRenderTarget = new(1280, 720);

	private readonly Renderer _renderer;
	private readonly Selector _fileSelector;
	private readonly Timer _timer;

	private string[] _files;
	private string _selectedFilePath = string.Empty;
	private Particles2d? _particles;
	private Emitter? _emitter;

	public string Name => "Particles";

	public ParticlesEditor(Renderer renderer, string baseFolder = ".\\Assets\\Particles\\")
	{
		_renderer = renderer;
		_baseFolder = baseFolder;
		_files = GetParticleFiles(baseFolder);

		_fileSelector = new("Select an particles file");
		_fileSelector.OnItemSelected += OnFileSelected;

		// TODO: must be removed and changed on smtg like `Files changed event`
		_timer = new Timer(
			(_) => _files = GetParticleFiles(_baseFolder),
			null,
			TimeSpan.Zero,
			TimeSpan.FromSeconds(2));
	}

	public void Dispose()
	{
		_timer.Dispose();
		_fileSelector.OnItemSelected -= OnFileSelected;
		_particlesRenderTarget?.Dispose();
	}

	public void Update(float dt)
	{
		_particles?.Update(dt);
	}

	public void Draw()
	{
		_fileSelector.Render(_files);

		ImGui.BeginChild("Emitter controls");
		EditEmitter();
		EmitterButtons();
		ImGui.EndChild();

		DrawParticles();
	}

	private void EmitterButtons()
	{
		if (_emitter != null)
		{
			ImGui.BeginChild("Emitter2");
			if (ImGui.Button("Save"))
			{
				var selectedFile = FullPath(_selectedFilePath);
				var emitterJson = JsonSerializer.Serialize(_emitter, _jsonOptions);
				File.WriteAllText(selectedFile!, emitterJson);
				Console.WriteLine($"Particle file changed - `{_selectedFilePath}`");
			}

			ImGui.SameLine();
			if (ImGui.Button("Restart"))
			{
				if (_particles != null)
				{
					_particles.Emitting = true;
				}
			}

			ImGui.EndChild();
		}
	}

	private void DrawParticles()
	{
		if (_emitter == null)
		{
			return;
		}

		ImGui.Begin("Particles render");

		_renderer.PushRenderTarget(_particlesRenderTarget);
		GL.Clear(ClearBufferMask.ColorBufferBit);
		_particles!.Draw();
		_renderer.PopRenderTarget();

		var (width, height) = (_particlesRenderTarget.Width, _particlesRenderTarget.Height);
		ImGui.Image(_particlesRenderTarget.Color, new(width, height));

		ImGui.End();
	}

	private void EditEmitter()
	{
		if (_emitter == null)
		{
			return;
		}

		EditOf.Render(_emitter);
		_emitter.MaxSpeed = Math.Max(_emitter.MaxSpeed, _emitter.MinSpeed);
	}

	private void OnFileSelected(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
		{
			throw new ArgumentNullException(nameof(filePath));
		}

		_emitter = ReadEmitter(filePath);
		_selectedFilePath = filePath;

		if (_emitter != null)
		{
			_particles = new Particles2d(_emitter, _renderer)
			{
				Emitting = true,
			};
		}
	}

	private Emitter? ReadEmitter(string file)
	{
		string path = FullPath(file);
		var emitter = JsonSerializer.Deserialize<Emitter>(
			File.ReadAllText(path),
			_jsonOptions);

		return emitter;
	}

	private string? FullPath(string? file)
	{
		if (file == null)
		{
			return null;
		}

		return Path.Combine(_baseFolder, file);
	}

	private static string[] GetParticleFiles(string baseFolder) =>
		Directory
			.GetFiles(baseFolder, "*.json", SearchOption.AllDirectories)
			.Select(x => Path.GetRelativePath(baseFolder, x))
			.ToArray();
}
