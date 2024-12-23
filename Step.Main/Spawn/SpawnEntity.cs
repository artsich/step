using OpenTK.Mathematics;

namespace Step.Main.Spawn;

public abstract class SpawnEntity(
	float probability,
	Func<IGameScene, bool> condition,
	Func<Vector2, IGameScene, Thing> createEntity)
{
	public float Probability { get; } = probability;

	public Func<IGameScene, bool> Condition { get; } = condition;

	public Func<Vector2, IGameScene, Thing> CreateEntity { get; } = createEntity;
}
