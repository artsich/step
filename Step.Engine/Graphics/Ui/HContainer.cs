namespace Step.Engine.Graphics.UI;

public class HContainer(string name = nameof(HContainer)) : Container(name)
{
	public HContainer(params Control[] controls)
		: this()
	{
		foreach(var control in controls)
		{
			AddChild(control);
		}
	}

	protected override void UpdateChildrenLayout()
	{
		float currentX = 0f;
		float maxHeight = 0f;

		// First pass: calculate max dimensions
		foreach (var child in children.OfType<Control>())
		{
			if (!child.Enabled)
			{
				continue;
			}
			maxHeight = Math.Max(maxHeight, child.Size.Y);
		}

		// Second pass: position children
		foreach (var child in children.OfType<Control>())
		{
			if (!child.Enabled)
			{
				continue;
			}

			float yOffset = 0f;
			if (UniformSize)
			{
				yOffset = (maxHeight - child.Size.Y) / 2f;
			}

			child.LocalPosition = new Vector2f(currentX, yOffset);
			currentX += child.Size.X + Spacing;
		}

		Size = new Vector2f(currentX - Spacing, maxHeight);
	}
}
