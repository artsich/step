namespace Step.Main.Gameplay;

public interface IGameScene
{
	Player Player { get; }

	void KillEnemies();

	int EffectsCount<T>() where T : IEffect;
}
