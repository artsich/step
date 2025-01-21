using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Step.Main.Gameplay;

namespace Step.Main;

public sealed class Input(
	MouseState mouseState,
	Camera2d camera)
{
	private Vector2 _mouseOffset;

	public Vector2 MouseScreenPosition { get; private set; }

	internal void Update(float _)
	{
		MouseScreenPosition = camera.ScreenToWorld(mouseState.Position - _mouseOffset);
	}

	internal void SetMouseOffset(Vector2 offset) => _mouseOffset = offset;
}
