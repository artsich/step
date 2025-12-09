using ImGuiNET;

namespace Step.Engine.Editor;

public class DebugLogWindow : IEditorView
{
	public string Name => "Debug Log";

	public void Draw()
	{
		ImGui.ShowDebugLogWindow();
	}
}

