using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class TowerDefenseSpawns : GameObject
{
	private readonly float _spawnSize = 25f;
	
	private Vector4f SpawnColor { get; set; } = new(0f, 0f, 1f, 1f);

	public TowerDefenseSpawns(Renderer renderer, Level level) : base(nameof(TowerDefenseSpawns))
	{
		foreach (var spawnPos in level.SpawnPositions)
		{
			var spawnSprite = new Sprite2d(renderer, renderer.DefaultWhiteTexture)
			{
				Color = SpawnColor,
				Layer = 5,
				LocalTransform = new Transform
				{
					Position = spawnPos,
					Scale = new Vector2f(_spawnSize, _spawnSize)
				}
			};
			
			AddChild(spawnSprite);
		}
	}
}
