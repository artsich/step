using OpenTK.Mathematics;

namespace Step.Engine.Graphics;

public abstract class CanvasItem(string name = nameof(CanvasItem)) : GameObject(name)
{
	public virtual int Layer { get; set; }

	public Color4<Rgba> Color { get; set; } = Color4.White;
}
