using Step.Engine.Graphics;

namespace Step.Engine;

public sealed class GameRoot
{
	private readonly static GameRoot _instance = new();

	public static GameRoot I => _instance;

	private readonly SceneController _sceneController = new();
	private readonly CameraStack _cameraStack = new();
	private readonly DeferredActionQueue _deferredActions = new();
	private readonly GameLoopCoordinator _loopCoordinator;

	private GameRoot()
	{
		_loopCoordinator = new GameLoopCoordinator(_sceneController, _deferredActions);
	}

	public float TimeScale
	{
		get => _loopCoordinator.TimeScale;
		set => _loopCoordinator.TimeScale = value;
	}

	public float RealDt => _loopCoordinator.RealDeltaTime;

	public float ScaledDt => _loopCoordinator.ScaledDeltaTime;

	public GameObject Scene => _sceneController.Scene;

	public ICamera2d? CurrentCamera => _cameraStack.Current;

	public void PushCamera(ICamera2d camera) => _cameraStack.Push(camera);

	public void PopCamera() => _cameraStack.Pop();

	public void SetScene(GameObject scene) => _sceneController.SetScene(scene);

	internal void Update(float dt) => _loopCoordinator.Update(dt);

	internal void Draw() => _loopCoordinator.Draw();

	internal void DebugDraw() => _loopCoordinator.DebugDraw();

	internal void Defer(Action action) => _deferredActions.Enqueue(action);
}
