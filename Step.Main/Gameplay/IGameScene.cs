namespace Step.Main.Gameplay;

public interface IGameScene
{
	Player Player { get; }

	void KillThings();

	int EffectsCount<T>() where T : IEffect;
}
