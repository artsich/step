using Step.Engine;
using Step.Engine.Collisions;
using Step.Engine.Graphics;

namespace Step.Main.Gameplay.Tron;

public sealed class TronGame : GameObject
{
	private readonly Engine.Engine _engine;
	private readonly Renderer _renderer;
	private readonly Camera2d _camera;
	private TronPlayer? _player;

	private readonly float _arenaHalfWidth;
	private readonly float _arenaHalfHeight;
	private readonly float _wallThickness = 6f;

	public TronGame(Engine.Engine engine, Camera2d camera, float arenaWidth, float arenaHeight)
	{
		_engine = engine;
		_renderer = engine.Renderer;
		_camera = camera;
		_arenaHalfWidth = arenaWidth * 0.5f;
		_arenaHalfHeight = arenaHeight * 0.5f;
		Name = nameof(TronGame);
	}

	protected override void OnStart()
	{
		BuildWalls();
		SpawnPlayer();
	}

	private void BuildWalls()
	{
		// Left
		AddChild(CreateWall(new Vector2f(-_arenaHalfWidth, 0f), new Vector2f(_wallThickness, _arenaHalfHeight * 2f), 1));
		// Right
		AddChild(CreateWall(new Vector2f(_arenaHalfWidth, 0f), new Vector2f(_wallThickness, _arenaHalfHeight * 2f), 2));
		// Top
		AddChild(CreateWall(new Vector2f(0f, _arenaHalfHeight), new Vector2f(_arenaHalfWidth * 2f, _wallThickness), 3));
		// Bottom
		AddChild(CreateWall(new Vector2f(0f, -_arenaHalfHeight), new Vector2f(_arenaHalfWidth * 2f, _wallThickness), 4));
	}

	private GameObject CreateWall(Vector2f center, Vector2f size, int n)
	{
		var wall = new GameObject($"Wall_{n}");
		wall.LocalTransform.Position = center;

		var rect = new RectangleShape2d(_renderer)
		{
			Size = size,
			Visible = false,
			IsStatic = true,
			CollisionLayers = (int)PhysicLayers.Frame,
			CollisionMask = (int)PhysicLayers.Player,
		};
		wall.AddChild(rect);

		var sprite = new Sprite2d(_renderer, _renderer.DefaultWhiteTexture)
		{
			Color = new Vector4f(0.2f, 0.3f, 0.5f, 1f),
			Layer = 0,
		};
		sprite.LocalTransform.Scale = size;
		wall.AddChild(sprite);

		return wall;
	}

	private void SpawnPlayer()
	{
		_player = new TronPlayer(_renderer, _engine.Input, size: 6f, speed: 80f)
		{
			LocalTransform =
			{
				Position = new Vector2f(0, 0)
			}
		};
		_player.OnDeath += () =>
		{
			(GameRoot.I.Scene as GameScene)?.PlayNewGame();
		};
		AddChild(_player);
	}
}


