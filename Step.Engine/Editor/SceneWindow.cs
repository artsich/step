using ImGuiNET;

namespace Step.Engine.Editor;

public class SceneWindow : IEditorView
{
	public string Name => "Scene";

	public void Draw()
	{
		if (ImGui.Begin("Scene"))
		{
			GameRoot.I.DebugDraw();
			ImGui.End();
		}
	}
}

