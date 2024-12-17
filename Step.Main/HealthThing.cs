namespace Step.Main;

public interface IEffect
{
	void Use();
}

public class HealEffect(int hp, Player player) : IEffect
{
	public void Use()
	{
		player.AddHp(hp);
		Console.WriteLine("Heal effect used...");
	}
}

public class KillAllEffect(IGameScene scene) : IEffect
{
	public void Use()
	{
		scene.KillThings();
		Console.WriteLine("Kill all used...");
	}
}
