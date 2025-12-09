using ImGuiNET;
using Silk.NET.Maths;

namespace Step.Engine.Editor;

public class GameRenderWindow(Engine engine) : IEditorView
{
	public string Name => "Game render";

	private const float TargetAspectRatio = 16f / 9f;

	public void Draw()
	{
		var renderResult = GameRoot.I.Scene as RenderResult;
		if (renderResult == null)
			return;

		if (ImGui.Begin("Game render", ImGuiWindowFlags.NoScrollbar))
		{
			var availRegion = ImGui.GetContentRegionAvail().FromSystem();
			var imgSize = StepMath
				.AdjustToAspect(
					TargetAspectRatio,
					availRegion)
				.ToSystem();

			var headerOffset = new Vector2f(
				(ImGui.GetWindowSize().X - availRegion.X) / 2f,
				ImGui.GetWindowSize().Y - availRegion.Y);

			ImGui.Image((nint)renderResult.ResultTexture.Handle, imgSize, new(0f, 1f), new(1f, 0f));

			var windowPos = ImGui.GetWindowPos().FromSystem();
			engine.Input.SetMouseOffset(windowPos + headerOffset);
			engine.Input.SetWindowSize(imgSize.FromSystem());
			ImGui.End();
		}
	}
}

