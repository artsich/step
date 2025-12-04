using ImGuiNET;
using Step.Engine.Editor;

namespace Step.Main.Editor;

public sealed class MapEditor : IEditorView
{
	public string Name => "Map editor";

	public void Update(float dt)
	{
	}

	public void Draw()
	{
		ImGui.Text("Map editor is under construction.");

		if (ImGui.Begin("Map editor", ImGuiWindowFlags.NoScrollbar))
		{
			ImGui.End();
		}
	}
}


