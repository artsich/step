using SixLabors.Fonts;

namespace Step.Engine.Graphics.Text;

// TODO: * - этот символ чуть выше чем надо
internal static class FontManager
{
	private static readonly Dictionary<string, Font> _fonts = [];
	private static readonly Dictionary<string, FontFamily> _fontFamilies = [];
	private static readonly FontCollection _fontCollection = new();

	public static FontFamily LoadFontFamily(string path)
	{
		if (_fontFamilies.TryGetValue(path, out var fontFamily))
		{
			return fontFamily;
		}

		fontFamily = _fontCollection.Add(path);
		_fontFamilies[path] = fontFamily;
		return fontFamily;
	}

	public static Font GetFont(string path, float size)
	{
		string key = $"{path}_{size}";
		if (_fonts.TryGetValue(key, out var font))
		{
			return font;
		}

		var fontFamily = LoadFontFamily(path);
		font = fontFamily.CreateFont(size, FontStyle.Regular);
		_fonts[key] = font;
		return font;
	}
}
