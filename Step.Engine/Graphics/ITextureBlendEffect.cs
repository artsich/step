namespace Step.Engine.Graphics;

public interface ITextureBlendEffect
{
	void Blend(Texture2d texture1, Texture2d texture2, out Texture2d output);

	public float AlphaThreshold { get; set; }
}