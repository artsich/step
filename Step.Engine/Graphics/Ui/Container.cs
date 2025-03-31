using Step.Engine.Editor;

namespace Step.Engine.Graphics.UI;

public abstract class Container : Control
{
	[EditorProperty]
	public float Spacing { get; set; } = 5f;
	public bool UniformSize { get; set; } = false;

	protected Container(string name) : base(name)
	{
		Layer = 100;
	}

	protected abstract void UpdateChildrenLayout();

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		UpdateChildrenLayout();
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}