using ImGuiNET;

namespace Step.Engine.Editor;

public class AudioSettingsWindow(Engine engine, float initialVolume) : IEditorView
{
	public string Name => "Audio Settings";

	public void Update(float dt)
	{
		engine.SetMasterVolume(initialVolume);
	}

	public void Draw()
	{
		if (ImGui.Begin("Audio Settings"))
		{
			if (ImGui.SliderFloat("Master volume", ref initialVolume, 0f, 1f))
			{
				engine.SetMasterVolume(initialVolume);
			}
			ImGui.End();
		}
	}
}

