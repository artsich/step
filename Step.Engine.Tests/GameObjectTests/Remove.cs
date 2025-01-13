namespace Step.Engine.Tests.GameObjectTests;

public class Remove
{
	[Fact]
	public void Ok()
	{
		var parent = new TestGameObject("Parent");
		var child = new TestGameObject("Child");

		parent.AddChild(child);
		Assert.Single(parent.GetChildsOf<TestGameObject>());

		parent.RemoveChild(child);
		var children = parent.GetChildsOf<TestGameObject>().ToList();
		Assert.Empty(children);

		Assert.Null(child.Parent);
	}

}
