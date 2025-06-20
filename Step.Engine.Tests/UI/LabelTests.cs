using Step.Engine.Graphics;
using Step.Engine.Graphics.Text;
using Step.Engine.Graphics.UI;
using static Step.Engine.Graphics.Text.FontAtlas;

namespace Step.Engine.Tests.UI;

public class LabelTests
{
	[Fact]
	public void SizeCalculatedCorrectly()
	{
		// Arrange
		var fakeAtlas = new IFontAtlas.Fake();
		fakeAtlas.GlyphMetrics['A'] = new GlyphInfo
		{
			Advance = 10f,
			Size = new Vector2f(8, 12),
			Bearing = new Vector2f(0, 10),
			AtlasRect = new Rect(0, 0, 8, 12)
		};
		fakeAtlas.GlyphMetrics[' '] = new GlyphInfo
		{
			Advance = 5f,
			Size = new Vector2f(5, 12),
			Bearing = new Vector2f(0, 10),
			AtlasRect = new Rect(0, 0, 5, 12)
		};
		fakeAtlas.GlyphMetrics['B'] = new GlyphInfo
		{
			Advance = 12f,
			Size = new Vector2f(9, 14),
			Bearing = new Vector2f(0, 11),
			AtlasRect = new Rect(0, 0, 9, 14)
		};
		var label = new Label(
			new IRenderCommands.Fake(),
			new IFontProvider.Fake(fakeAtlas))
		{
			Text = "A B A",
		};
		label.Start();

		// Act
		var size = label.Size;

		// Assert
		// "A B A" = A(10) + space(5) + B(12) + space(5) + A(10) = 42
		Assert.Equal(42f, size.X);
		Assert.Equal(14f, size.Y); // max height is B(14)
	}
}
