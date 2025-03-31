namespace Step.Engine.Graphics.UI;

public class VContainer() : Container(nameof(VContainer))
{
	public VContainer(params Control[] controls)
		: this()
	{
		foreach (var control in controls.Reverse())
		{
			AddChild(control);
		}
	}

	protected override void UpdateChildrenLayout()
	{
		float currentY = 0f;
		float maxWidth = 0f;

		// First pass: calculate max width
		foreach (var child in children.OfType<Control>())
		{
			if (!child.Enabled)
			{
				continue;
			}
			maxWidth = Math.Max(maxWidth, child.Size.X);
		}

		foreach (var child in children.OfType<Control>())
		{
			if (!child.Enabled)
			{
				continue;
			}

			float xOffset = 0f;
			if (UniformSize)
			{
				xOffset = (maxWidth - child.Size.X) / 2f;
				child.Size = child.Size with { X = maxWidth };
			}

			child.LocalPosition = new Vector2f(xOffset, currentY);
			currentY += child.Size.Y + Spacing;
		}

		Size = new Vector2f(maxWidth, currentY - Spacing);
	}
}