using OpenTK.Mathematics;
using Step.Main.Graphics;

namespace Step.Main.Gameplay;

public class Thing : GameObject
{
	private readonly Renderer _renderer;
	private readonly IEffect? effect;

	public float Speed { get; set; } = 60f;

	public Vector2 Size { get; }

	public Color4<Rgba> Color { get; init; } = Color4.Green;

	public Texture2d? Texture { get; init; }

	public Thing(Vector2 position, Vector2 size, Renderer renderer, IEffect? effect = null)
	{
		this.effect = effect;
		Size = size;
		_renderer = renderer;
		LocalTransform.Position = position;
	}

	protected override void OnUpdate(float dt)
	{
		var pos = LocalTransform.Position;
		pos.Y -= Speed * dt;
		LocalTransform.Position = pos;
	}

	protected override void OnRender()
	{
		_renderer.DrawObject(LocalTransform.Position, Size, Color4.White, Texture);
	}

	public void ApplyEffect(Player player)
	{
		if (effect is not null)
		{
			player.AddEffect(effect);
		}
	}

	public bool HasEffect<T>() where T : IEffect => effect is T;

	public Box2 BoundingBox => new(
		LocalTransform.Position - Size / 2f,
		LocalTransform.Position + Size / 2f);
}