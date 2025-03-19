namespace Step.Engine.Graphics.UI;

public class VContainer() : Container(nameof(VContainer))
{
	public VContainer(params Control[] controls)
	: this()
	{
		foreach (var control in controls)
		{
			AddChild(control);
		}
	}

	protected override void UpdateChildrenLayout()
	{
		float currentY = 0f;
		float maxWidth = 0f;

		foreach (var child in children.OfType<Control>())
		{
			child.LocalPosition = new Vector2f(0f, currentY);
			currentY += child.Size.Y + Spacing;
			maxWidth = Math.Max(maxWidth, child.Size.X);
		}

		Size = new Vector2f(maxWidth, currentY - Spacing);
	}
}