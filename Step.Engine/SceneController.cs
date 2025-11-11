using Step.Engine.Collisions;

namespace Step.Engine;

public sealed class SceneController
{
	private GameObject? _scene;

	public GameObject Scene => _scene ?? throw new InvalidOperationException("Scene is null");

	public void SetScene(GameObject scene)
	{
		_scene?.End();
		CollisionSystem.Ins.Reset();

		_scene = scene;
		_scene.Start();
	}

	public void Update(float deltaTime)
	{
		_scene?.Update(deltaTime);
	}

	public void Draw()
	{
		_scene?.Draw();
	}

	public void DebugDraw()
	{
		_scene?.DebugDraw();
	}
}

