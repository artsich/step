using Step.Engine;
using Step.Engine.Audio;
using Step.Engine.Collisions;
using Step.Engine.Graphics;
using Step.Main.Gameplay.Actors;
using Step.Main.Gameplay.UI;

namespace Step.Main.Gameplay.Builders;

public class GameBuilder(Engine.Engine engine, float gameCameraWidth, float gameCameraHeight)
{
	private Texture2d? _gliderTexture;
	private Texture2d? _circleTexture;
	private Texture2d? _playerTexture;
	private Texture2d? _crossTexture;

	public GameLoop Build()
	{
		LoadAssets();

		var gameLoop = new GameLoop(engine)
		{
			Name = "Game loop"
		};
		var player = CreatePlayer();
		var frame = new Frame(engine.Renderer);
		var spawner = CreateSpawner(player);

		gameLoop.AddChild(frame);
		gameLoop.AddChild(player);
		gameLoop.AddChild(spawner);
		gameLoop.AddChild(new GameTimer(engine.Renderer)
		{
			LocalPosition = new(0f, 70f)
		});

		return gameLoop;
	}

	private Player CreatePlayer()
	{
		var player = new Player(
			engine.Input,
			new RectangleShape2d(engine.Renderer)
			{
				Size = new Vector2f(16f),
				CollisionLayers = (int)PhysicLayers.Player,
				CollisionMask = (int)(PhysicLayers.Enemy | PhysicLayers.Frame)
			});

		var playerSprite = new PlayerSprite();
		playerSprite.AddChild(new Sprite2d(engine.Renderer, _playerTexture!)
		{
			Name = "Frame",
			Color = Constants.GameColors.Player,
		});

		playerSprite.AddChild(new Sprite2d(engine.Renderer, engine.Renderer.DefaultWhiteTexture)
		{
			Name = "Health",
			Color = Color.LightGreen,
			Pivot = new Vector2f(0f),
			LocalTransform = new Transform()
			{
				Position = new Vector2f(-8f, -8f),
				Scale = new Vector2f(16f),
			}
		});

		player.AddChild(playerSprite);
		player.AddAbility(new SpeedIncreaseAbility(player));
		player.AddAbility(new RegenerationAbility(player));
		player.AddAbility(new SizeChangerAbility(player) { Duration = 3f });
		player.AddAbility(new TimeFreezeAbility() { Duration = 2f });
		player.AddAbility(new MagnetAbility(50f, player, engine.Renderer));
		player.AddAbility(new ShieldAbility(player, new PlayerShield(engine.Input, engine.Renderer)) { Duration = 3f });

		return player;
	}

	private Spawner CreateSpawner(Player player)
	{
		var enemyFactory = new EnemyFactory(
			engine.Renderer,
			_gliderTexture!,
			_circleTexture!,
			_crossTexture!,
			player);

		return new Spawner(
			new Box2f(-gameCameraWidth / 2f, -gameCameraHeight / 2f, gameCameraWidth / 2f, gameCameraHeight / 2f),
			[
				new SpawnRule
				{
					StartTime = 0f,
					SpawnWeight = 1f,
					CreateEntity = enemyFactory.CreateCircle
				},
				new SpawnRule
				{
					StartTime = 60f,
					SpawnWeight = 0.05f,
					CreateEntity = enemyFactory.CreateGlider
				},
				new SpawnRule
				{
					StartTime = 30f,
					SpawnWeight = 0.1f,
					CreateEntity = enemyFactory.CreateCross,
					SpawnLocation = SpawnLocationType.Interior,
				}
			]);
	}

	private void LoadAssets()
	{
		AudioManager.Ins.LoadSound("start", "Music/ok_lets_go.mp3");
		AudioManager.Ins.LoadSound("main_theme", "Music/air-ambience-234180.mp3");
		AudioManager.Ins.LoadSound("player_hurt_glider", "Music/hurt.wav");
		AudioManager.Ins.LoadSound("player_hurt_circle", "Music/hurt2.wav");
		AudioManager.Ins.LoadSound("player_pickup", "Music/pickup.wav");
		AudioManager.Ins.LoadSound("wall_collision", "Music/wall_collision.mp3");

		_gliderTexture = Assets.LoadTexture2d("Textures/glider-enemy.png");
		_circleTexture = Assets.LoadTexture2d("Textures/circle-enemy.png");
		_playerTexture = Assets.LoadTexture2d("Textures/player.png");
		_crossTexture = Assets.LoadTexture2d("Textures/cross-enemy.png");
	}
}