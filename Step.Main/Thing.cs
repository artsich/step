using OpenTK.Mathematics;
using System.Drawing;

namespace Step.Main;

public interface IThing
{
	Vector2 Position { get; }

	Vector2 Size { get; }

	Box2 BoundingBox { get; }

	Color4<Rgba> Color { get; }

	void Update(float dt);

	void ApplyEffect(Player player);
}

public class Thing(Vector2 position, Vector2 size, float acceleration = 20f) : IThing
{
	public Vector2 Position { get; private set; } = position;

	public Vector2 Size { get; } = size;

	public virtual Color4<Rgba> Color => Color4.Green;

	public void Update(float dt)
	{
		var pos = Position;
		pos.Y -= acceleration * dt;
		Position = pos;
	}

	public virtual void ApplyEffect(Player player)
	{
	}

	public Box2 BoundingBox => new(Position - (Size / 2f), Position + (Size / 2f));
}