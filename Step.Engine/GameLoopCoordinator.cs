using Step.Engine.Collisions;

namespace Step.Engine;

public sealed class GameLoopCoordinator(
	SceneController sceneController,
	DeferredActionQueue deferredActions)
{
	public float TimeScale { get; set; } = 1.0f;

	public float RealDeltaTime { get; private set; }

	public float ScaledDeltaTime { get; private set; }

	public void Update(float deltaTime)
	{
		RealDeltaTime = deltaTime;
		ScaledDeltaTime = deltaTime * TimeScale;
		sceneController.Update(ScaledDeltaTime);
		CollisionSystem.Ins.Process();
		deferredActions.Process();
	}

	public void Draw()
	{
		sceneController.Draw();
	}

	public void DebugDraw()
	{
		sceneController.DebugDraw();
	}
}
