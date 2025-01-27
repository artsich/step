using Step.Engine.Graphics;

namespace Step.Main;

public interface IPostEffect
{
	void Apply(Texture2d input, out Texture2d output);
}
