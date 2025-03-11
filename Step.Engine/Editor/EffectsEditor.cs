using Step.Engine.Graphics;

namespace Step.Engine.Editor;

public class EffectsEditor(params IPostEffect[] effects) : IEditorView
{
	public string Name => "Effects";

	public void Draw()
	{
		foreach (var effect in effects)
		{
			//if (ImGui.BeginTabItem(name))
			{
				effect.DebugDraw();
				//ImGui.EndTabItem();
			}
		}
	}

	public void Update(float dt)
	{
	}
}
