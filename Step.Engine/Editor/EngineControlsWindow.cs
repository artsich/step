using ImGuiNET;

namespace Step.Engine.Editor;

public class EngineControlsWindow(Engine engine) : IEditorView
{
	public string Name => "Engine controls";

	private bool _gameLoopPaused;

	public void Update(float dt)
	{
		engine.SetGameLoopPaused(_gameLoopPaused);
	}

	public void Draw()
	{
		if (ImGui.Begin("Engine controls"))
		{
			if (ImGui.Button("Clear console"))
			{
				Console.Clear();
			}

			if (ImGui.Button(_gameLoopPaused ? "Paused" : "Un pause"))
			{
				_gameLoopPaused = !_gameLoopPaused;
			}

			ImGui.End();
		}
	}
}

