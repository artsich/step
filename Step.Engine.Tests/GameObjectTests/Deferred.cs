namespace Step.Engine.Tests.GameObjectTests;

public class Deferred
{
	[Fact]
	public void Ok()
	{
		bool actionExecuted = false;
		var dummyObject = new TestGameObject("Dummy");
		dummyObject.CallDeferred(() =>
		{
			actionExecuted = true;
		});

		GameRoot.I.SetScene(dummyObject);

		Assert.False(actionExecuted);

		GameRoot.I.Update(0.016f);

		Assert.True(actionExecuted);
	}

	[Fact]
	public void QueueFree_Ok()
	{
		var parent = new TestGameObject("Parent");
		var child = new TestGameObject("Child");
		parent.AddChild(child);
		GameRoot.I.SetScene(parent);

		child.QueueFree();

		Assert.Single(parent.GetChildsOf<TestGameObject>());

		GameRoot.I.Update(0.016f);

		Assert.Empty(parent.GetChildsOf<TestGameObject>());
		Assert.True(child.EndCalled);
	}
}
