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
					Position = new Vector2f(0f, 75f)
				}
			});
		
		var level = CreateLevel();
		
		var path = new TowerDefense.Path(engine.Renderer, level);
		gameLoop.AddChild(path);
		
		var baseObj = new Base(engine.Renderer, level);
		gameLoop.AddChild(baseObj);
		
		var grid = new Towers(engine.Renderer, level);
		gameLoop.AddChild(grid);
		
		var spawns = new Spawns(engine.Renderer, level);
		gameLoop.AddChild(spawns);
		
		return gameLoop;
	}

	private Level CreateLevel()
	{
		string[] map =
		{
			"....T....",
			"...PPP...",
			"..TPTP.B.",
			"...P.PTP.",
			".SPPTPPP.",
		};

		return new Level()
			.LoadFromStrings(30f, map)
			.ConfigureSpawn(enemyCount: 100, spawnFrequency: 0.25f)
			.Build();
	}
}