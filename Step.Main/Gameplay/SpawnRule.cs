﻿using Step.Engine;

namespace Step.Main.Gameplay;

public enum SpawnLocationType
{
	Border,
	Interior
}

public class SpawnRule
{
	public float StartTime { get; init; }

	public float SpawnWeight { get; init; }

	public Func<Vector2f, GameObject> CreateEntity { get; init; }

	public SpawnLocationType SpawnLocation { get; init; } = SpawnLocationType.Border;
}
