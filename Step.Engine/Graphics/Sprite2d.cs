using Step.Engine.Editor;

namespace Step.Engine.Graphics;

public sealed class Sprite2d : CanvasItem, ISerializable
{
	private Renderer? _renderer;
	private Texture2d? _atlas;
	private Rect? _region;

	public GeometryType GType = GeometryType.Quad;

	[Export]
	public Vector2f Pivot { get; set; } = new(0.5f);

	// Конструктор для сериализации
	public Sprite2d() : base(nameof(Sprite2d))
	{
	}

	public Sprite2d(Renderer renderer, Texture2d atlas, Rect? region = null)
		: base(nameof(Sprite2d))
	{
		_renderer = renderer;
		_atlas = atlas;
		_region = region ?? new Rect(0f, 0f, atlas.Width, atlas.Height);
		LocalTransform.Scale = new Vector2f(_atlas.Width, _atlas.Height);
	}

	public void DeserializeFromYaml(YamlNode data)
	{
		var mapping = (YamlMappingNode)data;
		
		// Получаем зависимости
		_renderer = GameRoot.I.Renderer;
		
		if (mapping.ContainsKey("texture"))
		{
			_atlas = Assets.LoadTexture2d(mapping["texture"].ToString());
			_region = new Rect(0f, 0f, _atlas.Width, _atlas.Height);
			LocalTransform.Scale = new Vector2f(_atlas.Width, _atlas.Height);
		}
		
		// Устанавливаем свойства
		if (mapping.ContainsKey("layer"))
			Layer = int.Parse(mapping["layer"].ToString());
		
		if (mapping.ContainsKey("color"))
		{
			var colorStr = mapping["color"].ToString();
			Color = ParseVector4f(colorStr);
		}
		
		if (mapping.ContainsKey("pivot"))
		{
			var pivotStr = mapping["pivot"].ToString();
			Pivot = ParseVector2f(pivotStr);
				}
	}
	
	private static Vector2f ParseVector2f(string str)
	{
		var clean = str.Trim('[', ']');
		var parts = clean.Split(',');
		return new Vector2f(float.Parse(parts[0]), float.Parse(parts[1]));
	}
	
	private static Vector4f ParseVector4f(string str)
	{
		var clean = str.Trim('[', ']');
		var parts = clean.Split(',');
		return new Vector4f(
			float.Parse(parts[0]), 
			float.Parse(parts[1]), 
			float.Parse(parts[2]), 
			float.Parse(parts[3])
		);
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
