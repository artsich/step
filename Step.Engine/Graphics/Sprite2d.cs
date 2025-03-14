﻿using Step.Engine.Editor;

namespace Step.Engine.Graphics;

public sealed class Sprite2d : CanvasItem
{
	private readonly Renderer _renderer;
	private readonly Texture2d _atlas;
	private readonly Rect? _region;

	public GeometryType GType = GeometryType.Quad;

	[EditorProperty]
	public Vector2f Pivot { get; set; } = new(0.5f);

	public Sprite2d(Renderer renderer, Texture2d atlas, Rect? region = null)
		: base(nameof(Sprite2d))
	{
		_renderer = renderer;
		_atlas = atlas;
		_region = region ?? new Rect(0f, 0f, atlas.Width, atlas.Height);
		LocalTransform.Scale = new Vector2f(_atlas.Width, _atlas.Height);
	}

	protected override void OnRender()
	{
		// todo: Children should not be rendered too.
		if (!Visible)
		{
			return;
		}

		_renderer.SubmitCommand(new()
		{
			Type = GType,
			Atlas = _atlas,
			AtlasRect = _region,
			Color = Color,
			Layer = Layer,
			ModelMatrix = GetGlobalMatrix(),
			Pivot = Pivot,
			Shader = Shader,
		});
	}

	protected override void OnDebugDraw()
	{
		base.OnDebugDraw();
		EditOf.Render(this);
	}
}
