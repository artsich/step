namespace Step.Main;

public interface IEffect
{
	void Use();
}

public class HealEffect(int hp, Player player) : IEffect
{
	public void Use()
	{
		Console.WriteLine("Heal effect used...");
		player.AddHp(hp);
	}
}

public class BombEffect(IGameScene scene) : IEffect
{
	public void Use()
	{
		scene.KillThings();
	}
}
