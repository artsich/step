using Step.Engine;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Tower : GameObject
{
	private const float SpriteScaleFactor = 0.9f;

	public Tower(Renderer renderer, Vector2f position, float cellSize) : base(nameof(Tower))
	{
		LocalTransform.Position = position;

		var sprite = new Sprite2d(renderer, Assets.LoadTexture2d("Textures\\spr_tower_lightning_tower.png"))
		{
			Layer = 8
		};
		var scaledSize = cellSize * SpriteScaleFactor;
		sprite.LocalTransform.Scale = new Vector2f(scaledSize, scaledSize);

		AddChild(sprite);
	}
}

