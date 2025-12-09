using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace Step.Engine.Editor;

public class EditorUI(ImGuiController imGuiController, List<IEditorView> windows)
{
	public void Update(float dt)
	{
		imGuiController.Update(dt);
		
		foreach (var window in windows)
		{
			window.Update(dt);
		}
	}

	public void Render()
	{
		ImGui.DockSpaceOverViewport();
		
		foreach (var window in windows)
		{
			window.Draw();
		}

		imGuiController.Render();
	}
}

