using ImGuiNET;
using Step.Engine.Collisions;

namespace Step.Engine.Editor;

public class PerformanceWindow(Engine engine) : IEditorView
{
	public string Name => "Performance";

	// todo: looks weird time is passed in update?
	private double _lastRenderTime;

	public void Update(float dt)
	{
		_lastRenderTime = dt;
	}

	public void Draw()
	{
		if (ImGui.Begin("Performance"))
		{
			var ms = _lastRenderTime * 1000f;
			var fps = 1000 / ms;
			ImGui.Text($"Render time: {ms:F2}ms | {fps:F2}fps");
			ImGui.Text($"Update time: {engine.LastUpdateTime * 1000:F2}ms");

			ImGui.Separator();
			ImGui.Text($"Collision shapes: {CollisionSystem.Ins.Count}");

			ImGui.Separator();
			ImGui.Text($"GPU Draw time: {engine.Renderer.Stats.GpuTimeMs:F5} ms");
			ImGui.Text($"Shaders used: {engine.Renderer.Stats.ActiveShaders}");

			ImGui.End();
		}
	}
}

