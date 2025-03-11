using Step.Engine.Editor;

namespace Step.Engine.Graphics;

public abstract class CanvasItem(string name = nameof(CanvasItem)) : GameObject(name)
{
	[EditorProperty]
	public virtual int Layer { get; set; } = 1;

	public Vector4f Color { get; set; } = Vector4f.One;

	public Shader? Shader { get; set; }

	public bool Visible { get; set; } = true;
}
