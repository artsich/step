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

		var maxHeight = children.OfType<Control>().Max(x => x.Size.Y);

		foreach (var child in children.OfType<Control>())
		{
			float yOffset = 0f;
			if (maxHeight > child.Size.Y)
			{
				yOffset = (maxHeight - child.Size.Y) / 2f;
			}

			child.LocalPosition = new Vector2f(currentX, yOffset);
			currentX += child.Size.X + Spacing;
		}

		Size = new Vector2f(currentX - Spacing, maxHeight);
	}
}
