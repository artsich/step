using Serilog;
using Silk.NET.GLFW;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

namespace Step.Engine.Graphics.Text;

public sealed class FontAtlas : IFontAtlas
{
	private readonly static Dictionary<string, FontAtlas> Atlases = [];

	private bool _debug = false;
	private int _cellSize;
	private int _charsPerRow;
	private float _maxBearingY;

	public Texture2d? Atlas { get; private set; }

	public Dictionary<char, GlyphInfo> GlyphMetrics { get; private set; } = [];

	public struct GlyphInfo
	{
		public Rect AtlasRect;
		public Vector2f Size;
		public Vector2f Bearing; // Offset from baseline to left/top of glyph
		public float Advance;    // Horizontal offset to advance to next glyph
	}

	public FontAtlas(string fontPath, float fontSize)
	{
		Log.Logger.Information("------------------");
		Log.Logger.Information($"Generating Font Atlas for: {fontPath} with {fontSize}px");

		GenerateAtlas(fontPath, fontSize);
		Log.Logger.Information("------------------");
	}

	private void GenerateAtlas(string fontPath, float fontSize)
	{
		var font = FontManager.GetFont(fontPath, fontSize);

		// Фиксированные параметры атласа
		_charsPerRow = 16;
		int totalChars = 127 - 32 + 1;
		int rows = (int)Math.Ceiling(totalChars / (float)_charsPerRow);

		// Сначала измеряем все символы, чтобы определить максимальные размеры
		MeasureAllGlyphs(font, fontSize);

		// Размер ячейки для символа (учитываем и верхнюю, и нижнюю части)
		_cellSize = (int)Math.Ceiling(fontSize * 2f);

		int atlasWidth = _cellSize * _charsPerRow;
		int atlasHeight = _cellSize * rows;

		Console.WriteLine($"Creating font atlas: {atlasWidth}x{atlasHeight}, Cell size: {_cellSize}px");

		using var atlasImage = new Image<Rgba32>(atlasWidth, atlasHeight);

		DrawGlyphsToAtlas(atlasImage, font, fontSize);

		using var ms = new MemoryStream();
		atlasImage.SaveAsPng(ms);
		ms.Position = 0;
		Atlas = Texture2d.LoadFromStream(ms);

		if (_debug)
		{
			atlasImage.SaveAsBmp(File.Open("font_atlas.bmp", FileMode.Create));
		}
	}

	private void MeasureAllGlyphs(Font font, float fontSize)
	{
		float advanceScale = GetAdvanceScale(font, fontSize);
		_maxBearingY = 0;

		for (char c = (char)32; c <= (char)127; c++)
		{
			var metrics = MeasureGlyph(font, c, advanceScale, fontSize);
			GlyphMetrics[c] = metrics;

			_maxBearingY = MathF.Max(_maxBearingY, metrics.Bearing.Y);
		}
	}

	private static float GetAdvanceScale(Font font, float fontSize)
	{
		float advanceScale = 1.0f;
		if (font.TryGetGlyphs(new CodePoint('A'), out IReadOnlyList<Glyph> testGlyphs) && testGlyphs.Count > 0)
		{
			ushort unitsPerEm = testGlyphs[0].GlyphMetrics.UnitsPerEm;
			if (unitsPerEm > 0)
			{
				advanceScale = fontSize / unitsPerEm;
				Console.WriteLine($"Advance scale: {advanceScale} (fontSize: {fontSize}, unitsPerEm: {unitsPerEm})");
			}
		}
		Debug.Assert(advanceScale > 0.0f);
		return advanceScale;
	}

	private void DrawGlyphsToAtlas(Image<Rgba32> atlasImage, Font font, float fontSize)
	{
		int charIndex = 0;

		for (char c = (char)32; c <= (char)127; c++)
		{
			int col = charIndex % _charsPerRow;
			int row = charIndex / _charsPerRow;

			int cellX = col * _cellSize;
			int cellY = row * _cellSize;

			DrawGlyphInCell(atlasImage, font, c, cellX, cellY, GlyphMetrics[c]);

			charIndex++;
		}
	}

	private static GlyphInfo MeasureGlyph(Font font, char c, float advanceScale, float fontSize)
	{
		float glyphWidth = 0;
		float glyphHeight = 0;
		float bearingX = 0;
		float bearingY = 0;
		float advance = 0;

		if (font.TryGetGlyphs(new CodePoint(c), out IReadOnlyList<Glyph> glyphs) && glyphs.Count > 0)
		{
			Glyph glyph = glyphs[0];

			FontRectangle box = glyph.BoundingBox(GlyphLayoutMode.Horizontal, System.Numerics.Vector2.Zero, 72);
			glyphWidth = (int)Math.Ceiling(box.Width);
			glyphHeight = (int)Math.Ceiling(box.Height);

			bearingX = box.X;
			bearingY = -box.Y;

			advance = MathF.Round(glyph.GlyphMetrics.AdvanceWidth * advanceScale);
		}
		else
		{
			Debug.Assert(false, "Wrong glyph, symbol without glyph");
		}

		return new GlyphInfo
		{
			Size = new Vector2f(glyphWidth, glyphHeight),
			Bearing = new Vector2f(bearingX, bearingY),
			Advance = advance,
		};
	}

	private void DrawGlyphInCell(Image<Rgba32> atlasImage, Font font, char c,
		int cellX, int cellY, GlyphInfo metrics)
	{
		using var tempImage = new Image<Rgba32>(_cellSize, _cellSize);
		tempImage.Mutate(ctx => ctx.Clear(SixLabors.ImageSharp.Color.Transparent));
		var options = new DrawingOptions
		{
			GraphicsOptions = new GraphicsOptions
			{
				Antialias = false, // if mod 16 then off
			}
		};
		tempImage.Mutate(ctx => ctx.DrawText(
			options,
			c.ToString(),
			font,
			SixLabors.ImageSharp.Color.White,
			new PointF(0, _maxBearingY - metrics.Bearing.Y)
		));

		Rectangle bounds = FindBounds(tempImage);

		// Вычисляем позицию базовой линии для всех символов
		// Базовая линия должна быть на одинаковой высоте для всех символов в атласе
		float baselineY = cellY + _maxBearingY;

		// Вычисляем позицию для отрисовки символа
		// Для всех символов используем одинаковую базовую линию
		float textX = cellX;

		// Важно: вычисляем Y-координату, отнимая bearingY от базовой линии
		// Это обеспечивает, что все символы будут выровнены по базовой линии
		float textY = baselineY - metrics.Bearing.Y;

		// Рисуем символ в атласе
		atlasImage.Mutate(ctx => ctx.DrawText(
			options,
			c.ToString(),
			font,
			SixLabors.ImageSharp.Color.White,
			new PointF(textX, textY)
		));

		// Если символ имеет видимые пиксели
		if (bounds.Width > 0 && bounds.Height > 0)
		{
			// Сохраняем прямоугольник символа в атласе с учетом его реальных границ
			GlyphMetrics[c] = GlyphMetrics[c] with
			{
				AtlasRect = new Rect(
				cellX + bounds.X,
				cellY + bounds.Y,
				bounds.Width,
				bounds.Height)
			};
		}
		else
		{
			// Для пустых символов (например, пробел) используем стандартный прямоугольник
			GlyphMetrics[c] = GlyphMetrics[c] with
			{
				AtlasRect = new Rect(
					(int)textX,
					(int)textY,
					metrics.Size.X,
					metrics.Size.Y)
			};
		}

		if (_debug)
		{
			// Рисуем границы ячейки для отладки
			atlasImage.Mutate(ctx =>
			{
				// Рисуем границу ячейки
				ctx.Draw(
					SixLabors.ImageSharp.Color.Orange,
					1,
					new RectangleF(cellX, cellY, _cellSize, _cellSize)
				);

				// Рисуем границу символа
				if (GlyphMetrics.TryGetValue(c, out var m))
				{
					var rect = m.AtlasRect;
					ctx.Draw(
						SixLabors.ImageSharp.Color.Green,
						1,
						new RectangleF(rect.X, rect.Y, rect.Width, rect.Height)
					);
				}

				// Рисуем базовую линию
				var points = new PointF[]
				{
					new(cellX, baselineY),
					new(cellX + _cellSize, baselineY)
				};

				ctx.DrawLine(
					SixLabors.ImageSharp.Color.Blue,
					1f,
					points
				);
			});
		}
	}

	private static Rectangle FindBounds(Image<Rgba32> image)
	{
		int minX = image.Width;
		int minY = image.Height;
		int maxX = 0;
		int maxY = 0;
		bool foundPixel = false;

		for (int y = 0; y < image.Height; y++)
		{
			for (int x = 0; x < image.Width; x++)
			{
				if (image[x, y].A > 0)
				{
					minX = Math.Min(minX, x);
					minY = Math.Min(minY, y);
					maxX = Math.Max(maxX, x);
					maxY = Math.Max(maxY, y);
					foundPixel = true;
				}
			}
		}

		if (!foundPixel)
		{
			return new Rectangle(0, 0, 0, 0);
		}

		return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
	}

	public static FontAtlas CreateFromFile(string fontPath, float fontSize)
	{
		var key = $"{fontPath}_{fontSize}";
		if (!Atlases.TryGetValue(key, out var fontAtlas))
		{
			fontAtlas = new FontAtlas(fontPath, fontSize);
			Atlases[key] = fontAtlas;
		}

		return fontAtlas;
	}
}
