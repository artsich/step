using ImGuiNET;

namespace Step.Engine.Editor;

public class GameControlsWindow(Engine engine) : IEditorView
{
	public string Name => "Game controls";

	public void Draw()
	{
		if (ImGui.Begin("Game controls"))
		{
			if (ImGui.Button("Game & Assets reload"))
			{
				engine.ReloadGame();
			}

			ImGui.Text($"Mouse: {engine.Input.MouseWorldPosition.X:F0}, {engine.Input.MouseWorldPosition.Y:F0}");

			ImGui.End();
		}
	}
}

