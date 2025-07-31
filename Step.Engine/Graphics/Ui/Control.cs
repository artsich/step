using Step.Engine.Editor;

namespace Step.Engine.Graphics.UI;

public abstract class Control(string name) : CanvasItem(name)
{
	[Export]
	public virtual Vector2f Size { get; set; }
}