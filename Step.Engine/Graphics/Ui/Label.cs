using Serilog;
using Step.Engine.Editor;
using Step.Engine.Graphics.Text;

namespace Step.Engine.Graphics.UI;

public sealed class Label : CanvasItem
{
	private readonly Renderer renderer;
	private FontAtlas? _font;

	private string _text = string.Empty;
	private bool _isDirty = true;
	private float _fontSize = 16f;
	private string _fontPath = "EngineData/Fonts/ProggyClean.ttf";

	[EditorProperty]
	public string Text
	{
		get => _text;
		set
		{
			if (_text != value)
			{
				_text = value;
			}
		}
	}

	[EditorProperty]
	public string FontPath
	{
		get => _fontPath;
		set
		{
			if (_fontPath != value)
			{
				_fontPath = value;
				_isDirty = true;
			}
		}
	}

	[EditorProperty]
	public float FontSize
	{
		get => _fontSize;
		set
		{
			if (_fontSize != value)
			{
				_fontSize = value;
				_isDirty = true;
			}
		}
	}

	public Label(Renderer renderer) : base(nameof(Label))
	{
		this.renderer = renderer;
		Layer = 100;
	}
	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);

		if (_isDirty)
		{
			LoadFont();
			_isDirty = false;
		}
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

				renderer.SubmitCommand(new RenderCmd
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
				Log.Logger.Warning($"Invlaid character: '{c} - '{(int)c}'");
			}
		}
	}

	private void LoadFont()
	{
		if (string.IsNullOrEmpty(FontPath))
			return;

		_font = FontAtlas.CreateFromFile(FontPath, _fontSize);
	}
}
