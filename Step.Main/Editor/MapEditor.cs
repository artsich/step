using ImGuiNET;
using Step.Engine;
using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Main.Editor;

public sealed class MapEditor : IEditorView
{
	public string Name => "Map editor";

	private const float TileSize = 20f;
	
	private readonly RenderTarget2d _renderTarget;
	
	private string[] map =
	{
		"................",
		"................",
		"................",
		"................",
		"................",
		"................",
		"................",
		"................",
		"................",
	};

	private readonly Renderer _renderer;
	private readonly float TargetAspectRatio = 16f / 9f;
	
	public MapEditor(Vector2i clientSize, ICamera2d camera)
	{
		_renderer = new Renderer(clientSize.X, clientSize.Y, Ctx.GL);
		_renderer.Load();
		_renderer.SetCamera(camera);

		_renderTarget = new RenderTarget2d(1920, 1080);;
	}
	
	public void Update(float dt)
	{
	}

	public void Draw()
	{
		ImGui.Text("Map editor is under construction.");

		if (ImGui.Begin("Map editor", ImGuiWindowFlags.NoScrollbar))
		{
			_renderer.PushRenderTarget(_renderTarget);
			_renderer.Clear();
			
			_renderer.DrawCircle(Vector2f.Zero, 10f, Color.Red);
			
			_renderer.Flush();
			_renderer.PopRenderTarget();
			
			var imgSize = StepMath.AdjustToAspect(
					TargetAspectRatio,
					ImGui.GetContentRegionAvail().FromSystem())
				.ToSystem();

			ImGui.Image((nint)_renderTarget.Color.Handle, imgSize, new(0f, 1f), new(1f, 0f));
			ImGui.End();
		}
	}
}


