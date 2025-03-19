using Step.Engine.Editor;

namespace Step.Engine.Graphics.UI;

public abstract class Control(string name) : CanvasItem(name)
{
	[EditorProperty]
	public virtual Vector2f Size { get; set; }
}