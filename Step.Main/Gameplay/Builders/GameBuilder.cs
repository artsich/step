using Step.Engine;
using Step.Engine.Audio;
using Step.Main.Gameplay.TowerDefense;
using Step.Main.Gameplay.TowerDefense.Core;
using Step.Main.Gameplay.UI;

namespace Step.Main.Gameplay.Builders;

public interface IGameBuilder
{
	GameLoop Build();
}

public class GameBuilder(Engine.Engine engine) : IGameBuilder
{
	private const float TileSize = 20f;

	public GameLoop Build()
	{
		AudioManager.Ins.LoadSound("base_heal", "Music/TD/base_heal.mp3");
		AudioManager.Ins.LoadSound("base_upgrade", "Music/TD/base_upgrade.mp3");
		AudioManager.Ins.LoadSound("main_theme", "Music/TD/main_theme.mp3");
		AudioManager.Ins.LoadSound("fight", "Music/TD/fight.mp3");
		AudioManager.Ins.LoadSound("base_hit", "Music/TD/base_hit.mp3");

		var gameLoop = new GameLoop();
		
		gameLoop.AddChild(
			new GameTimer(engine.Renderer) 
			{ 
				LocalTransform = new Transform()
				{
					Position = new Vector2f(120f, 75f)
				}
			});
		
		var level = CreateLevel();
		
		var path = new TowerDefense.Path(engine.Renderer, level);
		
		var baseObj = new Base(engine.Renderer, level);
		baseObj.Dead += () =>
		{
			gameLoop.OnFinish?.Invoke();
		};

		var spawns = new Spawns(engine.Renderer, level);

		var economySettings = TowerEconomySettings.Default;
		var economy = new TowerEconomy(spawns, economySettings);
		
		var grid = new Towers(engine.Renderer, engine.Input, level, spawns, economy);

		var baseSupportPanel = new BaseSupportPanel(engine.Renderer, engine.Input, economy, baseObj);
		baseObj.AddChild(baseSupportPanel);

		gameLoop.AddChild(path);
		gameLoop.AddChild(baseObj);
		gameLoop.AddChild(spawns);
		gameLoop.AddChild(economy);
		gameLoop.AddChild(grid);

		var goldCounter = new GoldCounter(engine.Renderer, economy)
		{
			LocalTransform = new Transform()
			{
				Position = new Vector2f(-130f, 55f)
			}
		};
		gameLoop.AddChild(goldCounter);

		var phaseController = new TowerDefensePhaseController(
			engine.Renderer,
			engine.Input,
			spawns,
			grid,
			baseObj,
			baseSupportPanel);
		gameLoop.AddChild(phaseController);
		
		return gameLoop;
	}

	private Level CreateLevel()
	{
		string[] map =
		{
			"................",
			"......PPPPP.....",
			"......P...PPB...",
			"...TPPPPP.......",
			"....P...P.......",
			"...PP...PPPPP...",
			"...P........P...",
			"...P........PP..",
			"...S.........S..",
		};

		var waves = new[]
		{
			new WaveConfig(
				enemyTypes: [new EnemyTypeWeight(EnemyType.Enemy1, 1f)],
				totalEnemyCount: 10,
				spawnIntervalSeconds: 1f),

			new WaveConfig(
				enemyTypes: [new EnemyTypeWeight(EnemyType.Enemy2, 1f)],
				totalEnemyCount: 20,
				spawnIntervalSeconds: 0.7f),

			new WaveConfig(
				enemyTypes:
				[
					new EnemyTypeWeight(EnemyType.Enemy1, 0.7f),
					new EnemyTypeWeight(EnemyType.Enemy2, 0.3f)
				],
				totalEnemyCount: 24,
				spawnIntervalSeconds: 0.8f),

			new WaveConfig(
				enemyTypes:
				[
					new EnemyTypeWeight(EnemyType.Enemy1, 0.5f),
					new EnemyTypeWeight(EnemyType.Enemy2, 0.3f),
					new EnemyTypeWeight(EnemyType.Enemy3, 0.2f)
				],
				totalEnemyCount: 25,
				spawnIntervalSeconds: 0.6f),

			new WaveConfig(
				enemyTypes:
				[
					new EnemyTypeWeight(EnemyType.Enemy2, 0.4f),
					new EnemyTypeWeight(EnemyType.Enemy3, 0.6f)
				],
				totalEnemyCount: 15,
				spawnIntervalSeconds: 0.5f)
		};

		return new Level()
			.LoadFromStrings(TileSize, map)
			.ConfigureWaves(waves)
			.Build();
	}
}