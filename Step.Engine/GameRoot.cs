using Step.Engine.Collisions;

namespace Step.Engine;

public sealed class GameRoot
{
	private readonly static GameRoot _instance = new();

	public static GameRoot I => _instance;

	private GameObject _scene;
	private readonly Queue<Action> _deferredActions = [];

	public void SetScene(GameObject scene)
	{
		_scene?.End();
		CollisionSystem.Ins.Reset();

		_scene = scene;
		_scene.Start();
	}

	public void Update(float dt)
	{
		_scene.Update(dt);
		CollisionSystem.Ins.Process();

		ProcessDeferred();
	}

	public void Draw()
	{
		_scene.Draw();
	}

	public void DebugDraw()
	{
		_scene.DebugDraw();
	}

	public void Defer(Action action)
	{
		_deferredActions.Enqueue(action);
	}

	private void ProcessDeferred()
	{
		while (_deferredActions.TryDequeue(out var action))
		{
			action();
		}
	}
}
