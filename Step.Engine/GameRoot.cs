using Step.Engine.Collisions;
using Step.Engine.Graphics;

namespace Step.Engine;

public sealed class GameRoot
{
	private readonly static GameRoot _instance = new();

	public static GameRoot I => _instance;

	private GameObject? _scene;
	private readonly Queue<Action> _deferredActions = [];

	public float TimeScale = 1.0f;

	public float RealDt { get; private set; }

	public float ScaledDt { get; private set; }

	public GameObject Scene => _scene ?? throw new InvalidOperationException("Scene is null");

	public ICamera2d? CurrentCamera => _cameras.TryPeek(out var result) ? result : null;

	private Stack<ICamera2d> _cameras = [];

	public void PushCamera(ICamera2d camera) => _cameras.Push(camera);

	public void PopCamera() => _cameras.Pop();

	public void SetScene(GameObject scene)
	{
		_scene?.End();
		CollisionSystem.Ins.Reset();

		_scene = scene;
		_scene.Start();
	}

	public void Update(float dt)
	{
		RealDt = dt;
		ScaledDt = dt * TimeScale;

		_scene?.Update(ScaledDt);
		CollisionSystem.Ins.Process();

		ProcessDeferred();
	}

	public void Draw()
	{
		_scene?.Draw();
	}

	public void DebugDraw()
	{
		_scene?.DebugDraw();
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
