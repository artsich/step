using ImGuiNET;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Step.Engine.Graphics;
using Step.Engine.Graphics.Particles;

namespace Step.Engine.Editor;

public sealed class ParticlesEditor : IEditorView, IDisposable
{
	private readonly string _baseFolder = Path.Combine(Assets.AssetsFolder, "Particles");
	private readonly RenderTarget2d _particlesRenderTarget;
	private readonly GL GL = Ctx.GL;
	private readonly Renderer _renderer;
	private readonly Selector _fileSelector;
	private readonly Timer _timer;

	private string[] _files;
	private string _selectedFilePath = string.Empty;
	private Particles2d? _particles;
	private Emitter? _emitter;

	public string Name => "Particles";

	private readonly float TargetAspectRatio = 16f / 9f;

	public ParticlesEditor(Vector2i clientSize, ICamera2d camera)
	{
		_particlesRenderTarget = new(1920, 1080);
		_renderer = new Renderer(clientSize.X, clientSize.Y, GL);
		_renderer.Load();
		_renderer.SetCamera(camera);
		_files = GetParticleFiles(_baseFolder);

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

		DrawParticlesPreview();
	}

	private void EmitterButtons()
	{
		if (_emitter != null)
		{
			ImGui.BeginChild("Emitter2");
			if (ImGui.Button("Save"))
			{
				Assets.SaveEmitter(
					Path.Combine("Particles", _selectedFilePath),
					_emitter);
				Log.Logger.Information($"Particle file changed - `{_selectedFilePath}`");
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

	private void DrawParticlesPreview()
	{
		if (_emitter == null)
		{
			return;
		}

		if (ImGui.Begin("Particles render", ImGuiWindowFlags.NoScrollbar))
		{
			_renderer.PushRenderTarget(_particlesRenderTarget);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_particles!.Draw();
			_renderer.Flush();
			_renderer.PopRenderTarget();

			var imgSize = StepMath.AdjustToAspect(
				TargetAspectRatio,
				ImGui.GetContentRegionAvail().FromSystem())
			.ToSystem();

			ImGui.Image((nint)_particlesRenderTarget.Color.Handle, imgSize, new(0f, 1f), new(1f, 0f));

			ImGui.End();
		}
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

		_emitter = Assets.LoadEmitter(Path.Combine("Particles", filePath));
		_selectedFilePath = filePath;

		if (_emitter != null)
		{
			_particles = new Particles2d(_emitter, _renderer)
			{
				Emitting = true,
			};
		}
	}

	private static string[] GetParticleFiles(string baseFolder) =>
		Directory
			.GetFiles(baseFolder, "*.json", SearchOption.AllDirectories)
			.Select(x => Path.GetRelativePath(baseFolder, x))
			.ToArray();
}
