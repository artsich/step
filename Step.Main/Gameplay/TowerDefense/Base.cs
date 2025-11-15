using Step.Engine;
using Step.Engine.Graphics;
using Step.Main.Gameplay.TowerDefense.Core;

namespace Step.Main.Gameplay.TowerDefense;

public sealed class Base : GameObject
{
	private const float BaseSize = 40f;

	private Vector4f BaseColor { get; set; } = new(1f, 0f, 0f, 1f);

	public Base(Renderer renderer, Level level) : base(nameof(Base))
	{
		var baseSprite = new Sprite2d(renderer, renderer.DefaultWhiteTexture)
		{
			Color = BaseColor,
			Layer = 6,
			LocalTransform = new Transform
			{
				Position = level.BasePosition,
				Scale = new Vector2f(BaseSize, BaseSize)
			}
		};
		
		AddChild(baseSprite);
	}
}

