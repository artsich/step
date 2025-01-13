using OpenTK.Mathematics;
using Step.Engine.Collisions;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay;

public class Thing : RectangleShape2d
{
	private readonly Renderer _renderer;
	private readonly IEffect? effect;

	public float Speed { get; set; } = 60f;

	public Color4<Rgba> Color { get; init; } = Color4.White;

	public Texture2d? Texture { get; init; }

	public bool IsFriend { get; init; }

	private static int Ids = 0;

	public Thing(
		Vector2 position,
		Vector2 size,
		Renderer renderer,
		IEffect? effect = null
	) : base(renderer)
	{
		this.effect = effect;
		Size = size;
		_renderer = renderer;
		LocalTransform.Position = position;
		Visible = true;

		Name = $"Thing: {Ids++}";
	}

	public void ApplyEffect(Player player)
	{
		if (effect is not null)
		{
			player.AddEffect(effect);
		}
	}

	public bool HasEffect<T>() where T : IEffect => effect is T;

	protected override void OnStart()
	{
		OnCollision += OnCollisionWithPlayer;
		base.OnStart();
	}

	protected override void OnUpdate(float dt)
	{
		var pos = LocalTransform.Position;
		pos.Y -= Speed * dt;
		LocalTransform.Position = pos;
	}

	protected override void OnRender()
	{
		_renderer.DrawObject(LocalTransform.Position, Size, Color, Texture);

		base.OnRender();
	}

	protected override void OnEnd()
	{
		OnCollision -= OnCollisionWithPlayer;
		base.OnEnd();
	}

	private void OnCollisionWithPlayer(CollisionShape shape)
	{
		if (shape is Player player)
		{
			if (IsFriend)
			{
				player.Damage(1);
			}
			else
			{
				player.Take(this);
			}

			QueueFree();
		}
	}
}