using Serilog;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using Silk.NET.Maths;
using Step.Engine.Graphics;

namespace Step.Engine;

// todo: The class is not stable if keyboard or mouse disabled and connected again...
public sealed class Input
{
	private Vector2f _mouseOffset;
	private Vector2f _windowSize;
	private readonly IMouse _mouseState;
	private readonly IKeyboard _keyboardState;

	private static readonly MouseButton[] MouseButtons = Enum.GetValues<MouseButton>();
	private readonly bool[] _currentMouseButtons = new bool[MouseButtons.Length];
	private readonly bool[] _previousMouseButtons = new bool[MouseButtons.Length];
	private readonly bool[] _keyboardKeys = new bool[350];
	private readonly bool[] _previousKeyboardKeys = new bool[350];

	public Input(
		IMouse mouseState,
		IKeyboard keyboardState)
	{
		_mouseState = mouseState;
		_keyboardState = keyboardState;

		_keyboardState.KeyDown += OnKeyboardKeyDown;
		_keyboardState.KeyUp += OnKeyboardKeyUp;
	}

	public Vector2f MouseWorldPosition
	{
		get
		{
			var camera = GameRoot.I.CurrentCamera;
			if (camera is not null)
			{
				return camera.ScreenToWorld(MouseScreenPosition, _windowSize);
			}
			return MouseScreenPosition;
		}
	}

	public Vector2f MouseScreenPosition01 { get; private set; }

	public Vector2f MouseScreenPosition => _mouseState.Position.ToGeneric() - _mouseOffset;

	public void Update(float _)
	{
		MouseScreenPosition01 = new(
			MouseScreenPosition.X / _windowSize.X,
			1f - MouseScreenPosition.Y / _windowSize.Y);

		foreach (var button in MouseButtons)
		{
			if (button == MouseButton.Unknown)
			{
				continue;
			}
			_previousMouseButtons[(int)button] = _currentMouseButtons[(int)button];
			_currentMouseButtons[(int)button] = _mouseState.IsButtonPressed(button);
		}

		Array.Copy(_keyboardKeys, _previousKeyboardKeys, _keyboardKeys.Length);
	}

	public void SetMouseOffset(Vector2f offset) => _mouseOffset = offset;

	public void SetWindowSize(Vector2f windowSize) => _windowSize = windowSize;

	public bool IsKeyPressed(Key key)
	{
		return _keyboardKeys[(int)key];
	}

	public bool IsKeyReleased(Key key)
	{
		return !IsKeyPressed(key);
	}

	public bool IsMouseButtonPressed(MouseButton button)
	{
		return _mouseState.IsButtonPressed(button);
	}

	public bool IsMouseButtonReleased(MouseButton button)
	{
		return !IsMouseButtonPressed(button);
	}

	public bool IsKeyJustPressed(Key key)
	{
		int keyIndex = (int)key;
		return _keyboardKeys[keyIndex] && !_previousKeyboardKeys[keyIndex];
	}

	public bool IsKeyJustReleased(Key key)
	{
		int keyIndex = (int)key;
		return !_keyboardKeys[keyIndex] && _previousKeyboardKeys[keyIndex];
	}

	public bool IsMouseButtonJustPressed(MouseButton button)
	{
		int buttonIndex = (int)button;
		return _currentMouseButtons[buttonIndex] && !_previousMouseButtons[buttonIndex];
	}

	public bool IsMouseButtonJustReleased(MouseButton button)
	{
		int buttonIndex = (int)button;
		return !_currentMouseButtons[buttonIndex] && _previousMouseButtons[buttonIndex];
	}

	private void OnKeyboardKeyDown(IKeyboard arg1, Key arg2, int arg3)
	{
		if (arg2 == Key.Unknown)
		{
			return;
		}
		_keyboardKeys[(int)arg2] = true;
	}

	private void OnKeyboardKeyUp(IKeyboard arg1, Key arg2, int arg3)
	{
		if (arg2 == Key.Unknown)
		{
			return;
		}
		_keyboardKeys[(int)arg2] = false;
	}
}
