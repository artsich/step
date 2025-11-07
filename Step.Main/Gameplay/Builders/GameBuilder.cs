using Step.Main.Gameplay.UI;
using Step.Main.Gameplay.Tron;
using Step.Engine;
using Step.Engine.Graphics;

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
		gameLoop.AddChild(new GameTimer(engine.Renderer) { LocalTransform = new Transform() { Position = new(0f, 60f) }	 });

		// Build simple Tron arena and player
		var camera = GameRoot.I.CurrentCamera as Camera2d;
		if (camera is not null)
		{
			var camScale = camera.LocalTransform.Scale;
			var arenaWidth = 320f; // matches Program.cs GameCameraWidth
			var arenaHeight = arenaWidth * (1f / (16f / 9f));
			var tron = new TronGame(engine, camera, arenaWidth, arenaHeight);
			gameLoop.AddChild(tron);
		}

		return gameLoop;
	}
}