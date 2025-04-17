namespace Step.Engine.Graphics;

public sealed class Viewport(Engine engine, ICamera2d camera2d, Vector2i size) : CanvasItem, IDisposable
{
	private readonly RenderTarget2d _renderTarget = new(size.X, size.Y, true);
	private readonly Renderer _renderer = engine.Renderer;

	public Texture2d ColorTexture => _renderTarget.Color;

	public Vector4f ClearColor { get; set; }

	public void Dispose() => _renderTarget?.Dispose();

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		GameRoot.I.PushCamera(camera2d);
	}

	protected internal override void OnUpdateEnd()
	{
		base.OnUpdateEnd();
		GameRoot.I.PopCamera();
	}

	protected override void OnRender()
	{
		base.OnRender();
		GameRoot.I.PushCamera(camera2d);
		_renderer.SetCamera(camera2d);
		_renderer.PushRenderTarget(_renderTarget);
		_renderTarget.Clear(ClearColor);
	}

	protected internal override void OnRenderEnd()
	{
		base.OnRenderEnd();
		_renderer.Flush();
		_renderer.PopRenderTarget();
		_renderer.SetCamera(null);
		GameRoot.I.PopCamera();
	}
}
