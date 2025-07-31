namespace Step.Engine.Editor;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ExportAttribute(float from = 0f, float to = 0f, float speed = 0f, bool isColor = false) : Attribute
{
	public ExportAttribute(bool isColor)
		: this(0f, 0f, 0f, isColor)
	{
	}

	public ExportAttribute() : this(false) 
	{
	}

	public bool IsColor { get; } = isColor;

	public float Speed { get; set; } = speed;

	public float From { get; } = from;

	public float To { get; } = to;
}
