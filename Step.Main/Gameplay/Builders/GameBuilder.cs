using Step.Engine;
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
	public GameLoop Build()
	{
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
			"...........",
			"...T.B.T...",
			"..TPPPPPT..",
			"...PTTTP...",
			"..PP...PP..",
			"..S.....S..",
		};

		// Создаем 5 волн
		var waves = new[]
		{
			// Волна 1: 5 врагов Enemy1 (2 HP), спавн раз в секунду
			new WaveConfig(
				enemyTypes: new[] { new EnemyTypeWeight(EnemyType.Enemy1, 1f) },
				totalEnemyCount: 5,
				spawnIntervalSeconds: 1f),

			// Волна 2: 7 врагов Enemy2 (3 HP), спавн каждые 0.7 секунды
			new WaveConfig(
				enemyTypes: new[] { new EnemyTypeWeight(EnemyType.Enemy2, 1f) },
				totalEnemyCount: 7,
				spawnIntervalSeconds: 0.7f),

			// Волна 3: микс врагов (70% Enemy1, 30% Enemy2)
			new WaveConfig(
				enemyTypes: new[]
				{
					new EnemyTypeWeight(EnemyType.Enemy1, 0.7f),
					new EnemyTypeWeight(EnemyType.Enemy2, 0.3f)
				},
				totalEnemyCount: 10,
				spawnIntervalSeconds: 0.8f),

			// Волна 4: микс всех типов врагов
			new WaveConfig(
				enemyTypes: new[]
				{
					new EnemyTypeWeight(EnemyType.Enemy1, 0.5f),
					new EnemyTypeWeight(EnemyType.Enemy2, 0.3f),
					new EnemyTypeWeight(EnemyType.Enemy3, 0.2f)
				},
				totalEnemyCount: 12,
				spawnIntervalSeconds: 0.6f),

			// Волна 5: больше сильных врагов
			new WaveConfig(
				enemyTypes: new[]
				{
					new EnemyTypeWeight(EnemyType.Enemy2, 0.4f),
					new EnemyTypeWeight(EnemyType.Enemy3, 0.6f)
				},
				totalEnemyCount: 15,
				spawnIntervalSeconds: 0.5f)
		};

		return new Level()
			.LoadFromStrings(30f, map)
			.ConfigureWaves(waves)
			.Build();
	}
}