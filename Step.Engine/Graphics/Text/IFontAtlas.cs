using static Step.Engine.Graphics.Text.FontAtlas;

namespace Step.Engine.Graphics.Text;

public interface IFontAtlas
{
	Texture2d? Atlas { get; }

	Dictionary<char, GlyphInfo> GlyphMetrics { get; }

	class Fake(Dictionary<char, GlyphInfo> glyphs, Texture2d? atlas) : IFontAtlas
	{
		public Fake() : this([], null) { }

		public Dictionary<char, GlyphInfo> GlyphMetrics { get; set; } = glyphs;

		public Texture2d? Atlas => atlas;
	}
}