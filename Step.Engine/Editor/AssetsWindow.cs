using ImGuiNET;

namespace Step.Engine.Editor;

public class AssetsWindow(params IEditorView[] editors) : IEditorView
{
	public string Name => "Assets";

	private readonly List<IEditorView> _editors = [.. editors];

	public void Update(float dt)
	{
		foreach (var editor in _editors)
		{
			editor.Update(dt);
		}
	}

	public void Draw()
	{
		if (ImGui.Begin("Assets"))
		{
			if (ImGui.BeginTabBar("Main Tabs"))
			{
				foreach (var editor in _editors)
				{
					if (ImGui.BeginTabItem(editor.Name))
					{
						editor.Draw();
						ImGui.EndTabItem();
					}
				}
				ImGui.EndTabBar();
			}
			ImGui.End();
		}
	}
}

