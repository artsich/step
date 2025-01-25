using OpenTK.Mathematics;
using Step.Engine.Editor;

namespace Step.Engine.Graphics;

public class Sprite2d : CanvasItem
{
	private readonly Renderer _renderer;
	private readonly Texture2d _atlas;
	private readonly Rect? _region;

	public GeometryType GType = GeometryType.Quad;

	[EditorProperty]
	public Vector2 Pivot { get; set; } = new Vector2(0.5f);

	public Sprite2d(Renderer renderer, Texture2d atlas, Rect? region = null)
		: base(nameof(Sprite2d))
	{
		_renderer = renderer;
		_atlas = atlas;
		_region = region ?? new Rect(0f, 0f, atlas.Width, atlas.Height);
		LocalTransform.Scale = new Vector2(_atlas.Width, _atlas.Height);
	}

	protected override void OnRender()
	{
		_renderer.SubmitCommand(new()
		{
			Type = GType,
			Atlas = _atlas,
			AtlasRect = _region,
			Color = Color,
			Layer = Layer,
			ModelMatrix = GetGlobalMatrix(),
			Pivot = Pivot,
		});
	}

	protected override void OnDebugDraw()
	{
		base.OnDebugDraw();
		EditOf.Render(this);
	}
}
