namespace Step.Engine.Editor;

[AttributeUsage(AttributeTargets.Property)]
public class EditorPropertyAttribute(float from, float to, bool isColor = false) : Attribute
{
	public EditorPropertyAttribute(bool isColor)
		: this(0f, 0f, isColor)
	{
	}

	public EditorPropertyAttribute() : this(false) 
	{
	}

	public bool IsColor { get; } = isColor;

	public float From { get; } = from;

	public float To { get; } = to;
}
