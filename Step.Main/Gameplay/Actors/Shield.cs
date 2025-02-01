using OpenTK.Mathematics;
using Serilog;
using Step.Engine;
using Step.Engine.Collisions;
using Step.Engine.Editor;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Actors;

public class PlayerShield : GameObject
{
	private readonly Input _input;
	private readonly CircleCollisionShape _collisionShape;
	private Sprite2d _sprite;

	[EditorProperty]
	public float Radius
	{
		get => _collisionShape.Radius;
		set => _collisionShape.Radius = value;
	}

	public PlayerShield(Input input, Renderer renderer)
	{
		_input = input;
		Name = nameof(PlayerShield);

		_collisionShape = new CircleCollisionShape(renderer)
		{
			CollisionLayers = (int)PhysicLayers.Shield,
			CollisionMask = (int)PhysicLayers.Enemy,
			IsStatic = true,

			IsActive = false,
			Visible = false
		};
		AddChild(_collisionShape);
		Radius = 30f;

		_sprite = new Sprite2d(renderer, renderer.DefaultWhiteTexture)
		{
			Visible = false,
			Color = new (1f, 0.7f, 0.93f, 1f),
			Shader = new Shader(
				"Assets/Shaders/Shield/shader.vert",
				"Assets/Shaders/Shield/shader.frag"),
			LocalTransform = new()
			{
				Scale = new Vector2(Radius * 2f)
			},
		};
		AddChild(_sprite);
	}

	protected override void OnStart()
	{
		_collisionShape.OnCollision += OnCollisionWithEnemy;
	}

	protected override void OnEnd()
	{
		_collisionShape.OnCollision -= OnCollisionWithEnemy;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (_sprite.Visible)
		{
			Vector2 arcDir = (_input.MouseWorldPosition - GlobalPosition).Normalized();
			float arcAngle = MathF.Atan2(arcDir.Y, arcDir.X);
			_sprite.Shader!.SetFloat("arcAngle", arcAngle);
		}
	}

	public void Enable()
	{
		_collisionShape.IsActive = true;
		_sprite.Visible = true;
	}

	public void Disable()
	{
		_collisionShape.IsActive = false;
		_sprite.Visible = false;
	}

	private void OnCollisionWithEnemy(CollisionShape shape, CollisionInfo info)
	{
		if (shape.Parent is CircleEnemy || shape.Parent is GliderEntity)
		{
			float collisionAngle = MathF.Atan2(info.Normal.Y, info.Normal.X);

			Vector2 arcDir = GlobalPosition - _input.MouseWorldPosition;
			float arcAngle = MathF.Atan2(arcDir.Y, arcDir.X);

        	if (MathF.Abs(collisionAngle - arcAngle) <= MathF.PI / 4f)
			{
				shape.Parent.QueueFree();
			}
		}
	}
}
