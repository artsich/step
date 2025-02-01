using OpenTK.Mathematics;
using Step.Engine.Editor;

namespace Step.Engine.Graphics;

public abstract class CanvasItem(string name = nameof(CanvasItem)) : GameObject(name)
{
	[EditorProperty]
	public virtual int Layer { get; set; }

	public Color4<Rgba> Color { get; set; } = Color4.White;
}
