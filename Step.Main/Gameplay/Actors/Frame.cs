using Step.Engine;
using Step.Engine.Collisions;
using Step.Engine.Graphics;
using Step.Engine.Physics;
using OpenTK.Mathematics;

namespace Step.Main.Gameplay.Actors;

public class Frame : GameObject
{
	public float Thickness { get; } = 2f;

	public float InnerWidth { get; } = 50f;

	public Frame(Renderer renderer) : base(nameof(Frame))
	{
		float totalWidth = Thickness * 2f + InnerWidth;

		AddWall(renderer, "Left", new Vector2(-InnerWidth / 2 - Thickness / 2, 0), new Vector2(Thickness, totalWidth));
		AddWall(renderer, "Top", new Vector2(0, -InnerWidth / 2 - Thickness / 2), new Vector2(totalWidth, Thickness));
		AddWall(renderer, "Right", new Vector2(InnerWidth / 2 + Thickness / 2, 0), new Vector2(Thickness, totalWidth));
		AddWall(renderer, "Bottom", new Vector2(0, InnerWidth / 2 + Thickness / 2), new Vector2(totalWidth, Thickness));
	}

	private void AddWall(Renderer renderer, string name, Vector2 position, Vector2 size)
	{
		var wall = new StaticBody2d(
			new RectangleShape2d(renderer)
			{
				Size = size,
				CollisionLayers = (int)PhysicLayers.Frame,
				CollisionMask = (int)PhysicLayers.Player,
				IsStatic = true,
			})
		{
			Name = name,
			LocalPosition = position
		};

		wall.AddChild(new Sprite2d(renderer, renderer.DefaultWhiteTexture)
		{
			Layer = 0,
			LocalTransform = new()
			{
				Scale = size
			}
		});

		AddChild(wall);
	}
}
