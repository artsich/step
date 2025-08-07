using Step.Engine.Editor;

namespace Step.Engine.Graphics.UI;

public sealed class TextureRect : Control
{
	private readonly Renderer renderer;
	private Texture2d? _texture;
	private Rect? _region;

	[Export]
	public Vector2f Pivot { get; set; } = new(0f);

	public GeometryType Type = GeometryType.Quad;

	public TextureRect(Renderer renderer) : base(nameof(TextureRect))
	{
		this.renderer = renderer;
		Layer = 100;
	}

	public void SetTexture(Texture2d texture, Rect? region = null)
	{
		_texture = texture;
		_region = region ?? new Rect(0f, 0f, texture.Width, texture.Height);
		Size = new Vector2f(_region.Value.Width, _region.Value.Height) * LocalTransform.Scale;
	}

	protected override void OnRender()
	{
		if (!Visible)
			return;

		var size = Size / LocalTransform.Scale;
		var model = Matrix4.CreateScale(size.X, size.Y, 1f) * GetGlobalMatrix();
		renderer.SubmitCommand(new RenderCmd
		{
			Type = Type,
			Atlas = _texture,
			AtlasRect = _region,
			Color = Color,
			Layer = Layer,
			ModelMatrix = model,
			Pivot = Pivot,
		});
	}

	protected override void OnDebugDraw()
	{
		EditOf.Render(this);
	}
}
