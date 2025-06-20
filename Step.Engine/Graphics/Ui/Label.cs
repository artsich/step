using Serilog;
using Step.Engine.Editor;
using Step.Engine.Graphics.Text;

namespace Step.Engine.Graphics.UI;

public interface IFontProvider
{
	IFontAtlas Get(string fontPath, float fontSize);

	class Fake(IFontAtlas atlas) : IFontProvider
	{
		public IFontAtlas Get(string fontPath, float fontSize)
		{
			return atlas;
		}
	}
}

public class FontProvider : IFontProvider
{
	public IFontAtlas Get(string fontPath, float fontSize)
	{
		return FontAtlas.CreateFromFile(fontPath, fontSize);
	}
}

public sealed class Label : Control
{
	private readonly IRenderCommands _renderer;
	private readonly IFontProvider _fontProvider;

	private IFontAtlas? _font;
	private Vector2f _minSize;

	private string _text = string.Empty;
	private readonly float _fontSize = 16f;
	private readonly string _fontPath = "EngineData/Fonts/ProggyClean.ttf";

	private bool _isDirty = true;

	[EditorProperty]
	public string Text
	{
		get => _text;
		set
		{
			if (_text != value)
			{
				_text = value;
				_isDirty = true;
			}
		}
	}

	public override Vector2f Size
	{
		get => _minSize;
		set
		{
			if (_font == null || string.IsNullOrEmpty(_text))
			{
				_minSize = Vector2f.Zero;
				return;
			}
			else {
				var textSize = CalculateTextSize(_font!, Text);
				_minSize = new Vector2f(
					Math.Max(value.X, textSize.X),
					Math.Max(value.Y, textSize.Y));
			}
		}
	}

	public Label(
		IRenderCommands renderer,
		IFontProvider fontProvider,
		string fontPath = "",
		int fontSize = 0) : base(nameof(Label))
	{
		_renderer = renderer;
		_fontProvider = fontProvider;

		_fontPath = string.IsNullOrEmpty(fontPath) ? _fontPath : fontPath;
		_fontSize = fontSize > 0 ? fontSize : _fontSize;

		Layer = 100;
	}

	public Label(IRenderCommands render, string fontPath = "", int fontSize = 0)
		: this(render, new FontProvider(), fontPath, fontSize)
	{
	}

	protected override void OnStart()
	{
		base.OnStart();
		_font = _fontProvider.Get(_fontPath, _fontSize);
		TryFixDirty();
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		TryFixDirty();
	}

	protected override void OnRender()
	{
		if (!Visible || _font == null || string.IsNullOrEmpty(_text))
			return;

		var model = GetGlobalMatrix();

		float currentX = 0;
		foreach (var c in _text)
		{
			if (_font.GlyphMetrics.TryGetValue(c, out var metrics))
			{
				var atlasRect = metrics.AtlasRect;
				float glyphX = currentX + metrics.Bearing.X;
				float glyphY = -(metrics.Size.Y - metrics.Bearing.Y);

				var glyphModelMat = Matrix4.CreateScale(atlasRect.Width, atlasRect.Height, 1f) * Matrix4.CreateTranslation(glyphX, glyphY, 0f);

				_renderer.SubmitCommand(new RenderCmd
				{
					ModelMatrix = glyphModelMat * model,
					Atlas = _font.Atlas,
					AtlasRect = atlasRect,
					Type = GeometryType.Quad,
					Color = Color,
					Pivot = new Vector2f(0f, 0f),
					Layer = Layer,
				});

				currentX += metrics.Advance;
			}
			else
			{
				Log.Logger.Warning($"Invalid character: '{c} - '{(int)c}'");
			}
		}
	}

	private void TryFixDirty()
	{
		if (_isDirty)
		{
			_minSize = CalculateTextSize(_font!, Text) * LocalTransform.Scale; // TODO: or global transform
			_isDirty = false;
		}
	}

	private static Vector2f CalculateTextSize(IFontAtlas font, string text)
	{
		Vector2f size = Vector2f.Zero;
		foreach (var c in text)
		{
			if (font.GlyphMetrics.TryGetValue(c, out var metrics))
			{
				size.X += metrics.Advance;
				size.Y = Math.Max(size.Y, metrics.Size.Y);
			}
		}
		return size;
	}
}
