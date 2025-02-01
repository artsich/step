using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Step.Main.Gameplay;

namespace Step.Main;

public sealed class Input(
	MouseState mouseState,
	KeyboardState keyboardState,
	Camera2d camera,
	IGameWindow window)
{
	private Vector2 _mouseOffset;

	public Vector2 MouseWorldPosition { get; private set; }

	public Vector2 MouseScreenPosition01 { get; private set; }

	public MouseState MouseState => mouseState;

	public KeyboardState KeyboardState => keyboardState;

	internal void Update(float _)
	{
		var mousePosition = mouseState.Position - _mouseOffset;
		MouseWorldPosition = camera.ScreenToWorld(mousePosition);

		MouseScreenPosition01 = new(
			mousePosition.X / window.Size.X,
			1f - mousePosition.Y / window.Size.Y
		);
	}

	internal void SetMouseOffset(Vector2 offset) => _mouseOffset = offset;
}
