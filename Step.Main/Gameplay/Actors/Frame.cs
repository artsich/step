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

		AddChild(
			new StaticBody2d(
				new RectangleShape2d(renderer)
				{
					Size = new Vector2(Thickness, totalWidth),
					CollisionLayers = (int)PhysicLayers.Frame,
					CollisionMask = (int)PhysicLayers.Player,
					Visible = true,
					IsStatic = true,
				})
			{
				Name = "Left",
				LocalPosition = new Vector2(-InnerWidth / 2 - Thickness / 2, 0)
			});

		AddChild(
			new StaticBody2d(
				new RectangleShape2d(renderer)
				{
					Size = new Vector2(totalWidth, Thickness),
					CollisionLayers = (int)PhysicLayers.Frame,
					CollisionMask = (int)PhysicLayers.Player,
					Visible = true,
					IsStatic = true,
				})
			{
				Name = "Top",
				LocalPosition = new Vector2(0, -InnerWidth / 2 - Thickness / 2)
			});

		AddChild(
			new StaticBody2d(
				new RectangleShape2d(renderer)
				{
					Size = new Vector2(Thickness, totalWidth),
					CollisionLayers = (int)PhysicLayers.Frame,
					CollisionMask = (int)PhysicLayers.Player,
					Visible = true,
					IsStatic = true,
				})
			{
				Name = "Right",
				LocalPosition = new Vector2(InnerWidth / 2 + Thickness / 2, 0)
			});

		AddChild(
			new StaticBody2d(
				new RectangleShape2d(renderer)
				{
					Size = new Vector2(totalWidth, Thickness),
					CollisionLayers = (int)PhysicLayers.Frame,
					CollisionMask = (int)PhysicLayers.Player,
					Visible = true,
					IsStatic = true,
				})
			{
				Name = "Bottom",
				LocalPosition = new Vector2(0, InnerWidth / 2 + Thickness / 2)
			});
	}
}
