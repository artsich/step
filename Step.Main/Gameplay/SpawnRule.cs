using OpenTK.Mathematics;
using Step.Engine;

namespace Step.Main.Gameplay;

public enum SpawnLocationType
{
	Border,
	Interior
}

public class SpawnRule
{
	public float StartTime { get; init; }

	public float SpawnProbability { get; init; }

	public Func<Vector2, GameObject> CreateEntity { get; init; }

	public SpawnLocationType SpawnLocation { get; init; } = SpawnLocationType.Border;
}
