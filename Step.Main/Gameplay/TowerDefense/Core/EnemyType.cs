namespace Step.Main.Gameplay.TowerDefense.Core;

public enum EnemyType
{
	Enemy1,
	Enemy2,
	Enemy3
}

public readonly struct EnemyTypeConfig(EnemyType type, float health, float moveSpeed, Vector4f color)
{
	public EnemyType Type { get; } = type;

	public float Health { get; } = health;
	
	public float MoveSpeed { get; } = moveSpeed;
	
	public Vector4f Color { get; } = color;

	public static EnemyTypeConfig GetDefault(EnemyType type)
	{
		return type switch
		{
			EnemyType.Enemy1 => new EnemyTypeConfig(type, 2f, 25f, Vector4f.One),
			EnemyType.Enemy2 => new EnemyTypeConfig(type, 3f, 25f, new Vector4f(0.8f, 0.3f, 0.3f, 1f)),
			EnemyType.Enemy3 => new EnemyTypeConfig(type, 5f, 25f, new Vector4f(0.3f, 0.3f, 0.8f, 1f)),
			_ => new EnemyTypeConfig(EnemyType.Enemy1, 2f, 25f, Vector4f.One)
		};
	}
}

