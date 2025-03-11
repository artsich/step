namespace Step.Engine.Graphics;

public interface IPostEffect
{
	void Apply(Texture2d input, out Texture2d output);

	void DebugDraw();
}