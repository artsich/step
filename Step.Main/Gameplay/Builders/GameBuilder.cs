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
		gameLoop.AddChild(new GameTimer(engine.Renderer));
		return gameLoop;
	}
}