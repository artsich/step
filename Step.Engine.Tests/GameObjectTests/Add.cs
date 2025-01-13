namespace Step.Engine.Tests.GameObjectTests;

public class Add
{
	[Fact]
	public void Ok()
	{
		var parent = new TestGameObject("Parent");
		var child = new TestGameObject("Child");

		parent.AddChild(child);

		var children = parent.GetChildsOf<TestGameObject>().ToList();
		Assert.Single(children);
		Assert.Equal("Child", children[0].Name);
		Assert.Same(child.Parent, parent);
	}
}
