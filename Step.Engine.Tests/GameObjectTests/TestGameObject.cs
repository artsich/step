namespace Step.Engine.Tests.GameObjectTests;

public class TestGameObject(string name = nameof(TestGameObject)) : GameObject(name)
{
	public bool EndCalled { get; private set; }

	protected override void OnEnd()
	{
		EndCalled = true;
	}
}
