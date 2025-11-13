using Step.Engine;
using Step.Main.Gameplay.TowerDefense;
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
		
		var path = new TowerDefensePath(engine.Renderer, level);
		gameLoop.AddChild(path);
		
		var baseObj = new TowerDefenseBase(engine.Renderer, level);
		gameLoop.AddChild(baseObj);
		
		var grid = new TowerDefenseGrid(engine.Renderer, level);
		gameLoop.AddChild(grid);
		
		var spawns = new TowerDefenseSpawns(engine.Renderer, level);
		gameLoop.AddChild(spawns);
		
		return gameLoop;
	}

	private Level CreateLevel()
	{
		string[] map =
		{
			"..TTTTTTT..",
			".SPPPPPPPB.",
			"..TTTTTTT..",
		};

		return new Level()
			.LoadFromStrings(30f, map)
			.Build();
	}
}