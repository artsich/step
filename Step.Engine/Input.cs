using Silk.NET.Input;
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
	private readonly IGamepad? _gamepad;

	private static readonly MouseButton[] MouseButtons = Enum.GetValues<MouseButton>();

	private readonly bool[] _currentMouseButtons = new bool[MouseButtons.Length];
	private readonly bool[] _previousMouseButtons = new bool[MouseButtons.Length];
	private readonly bool[] _keyboardKeys = new bool[350];
	private readonly bool[] _previousKeyboardKeys = new bool[350];
	private readonly bool[] _currentGamepadButtons = new bool[15]; // Assuming 15 buttons max
	private readonly bool[] _previousGamepadButtons = new bool[15];

	public Input(IInputContext inputCtx)
	{
		_mouseState = inputCtx.Mice.First(x => x.IsConnected);
		_keyboardState = inputCtx.Keyboards.First(x => x.IsConnected);
		_gamepad = inputCtx.Gamepads.FirstOrDefault(x => x.IsConnected);

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
			throw new InvalidOperationException("Camera is not set.");
		}
	}

	public Vector2f MouseScreenPosition01 { get; private set; }

	public Vector2f MouseScreenPosition => _mouseState.Position.ToGeneric() - _mouseOffset;

	public void Update(float _)
	{
		MouseScreenPosition01 = new(
			MouseScreenPosition.X / _windowSize.X,
			1f - MouseScreenPosition.Y / _windowSize.Y);

		GameRoot.I.PropagateEvent(new MouseHoverEvent(MouseWorldPosition));

		// Update gamepad button states
		if (_gamepad is not null)
		{
			Array.Copy(_currentGamepadButtons, _previousGamepadButtons, _currentGamepadButtons.Length);
			_currentGamepadButtons[(int)ButtonName.A] = _gamepad.A().Pressed;
			_currentGamepadButtons[(int)ButtonName.B] = _gamepad.B().Pressed;
			_currentGamepadButtons[(int)ButtonName.X] = _gamepad.X().Pressed;
			_currentGamepadButtons[(int)ButtonName.Y] = _gamepad.Y().Pressed;
			_currentGamepadButtons[(int)ButtonName.LeftBumper] = _gamepad.LeftBumper().Pressed;
			_currentGamepadButtons[(int)ButtonName.RightBumper] = _gamepad.RightBumper().Pressed;
			_currentGamepadButtons[(int)ButtonName.Back] = _gamepad.Back().Pressed;
			_currentGamepadButtons[(int)ButtonName.Start] = _gamepad.Start().Pressed;
			_currentGamepadButtons[(int)ButtonName.Home] = _gamepad.Home().Pressed;
			_currentGamepadButtons[(int)ButtonName.LeftStick] = _gamepad.LeftThumbstickButton().Pressed;
			_currentGamepadButtons[(int)ButtonName.RightStick] = _gamepad.RightThumbstickButton().Pressed;
			_currentGamepadButtons[(int)ButtonName.DPadUp] = _gamepad.DPadUp().Pressed;
			_currentGamepadButtons[(int)ButtonName.DPadRight] = _gamepad.DPadRight().Pressed;
			_currentGamepadButtons[(int)ButtonName.DPadDown] = _gamepad.DPadDown().Pressed;
			_currentGamepadButtons[(int)ButtonName.DPadLeft] = _gamepad.DPadLeft().Pressed;
		}

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

	internal void SetMouseOffset(Vector2f offset) => _mouseOffset = offset;

	internal void SetWindowSize(Vector2f windowSize) => _windowSize = windowSize;

	public Vector2f GetRightStick()
	{
		if (_gamepad is null)
		{
			return Vector2f.Zero;
		}
		var rightThumbstick = _gamepad.RightThumbstick();
		var stick = new Vector2f(rightThumbstick.X, rightThumbstick.Y);

		const float deadzone = 0.1f;
		if (stick.LengthSquared < deadzone * deadzone)
		{
			return Vector2f.Zero;
		}

		return stick;
	}

	public Vector2f GetLeftStick()
	{
		if (_gamepad is null)
		{
			return Vector2f.Zero;
		}
		var leftThumbstick = _gamepad.LeftThumbstick();
		var stick = new Vector2f(leftThumbstick.X, leftThumbstick.Y);
		
		const float deadzone = 0.1f;
		if (stick.LengthSquared < deadzone * deadzone)
		{
			return Vector2f.Zero;
		}
		
		return stick;
	}

	public bool IsGamepadButtonPressed(ButtonName button)
	{
		if (_gamepad is null)
		{
			return false;
		}

		try
		{
			return _currentGamepadButtons[(int)button];
		}
		catch
		{
			return false;
		}
	}

	public bool IsGamepadButtonReleased(ButtonName button)
	{
		if (_gamepad is null)
		{
			return false;
		}

		try
		{
			return !_currentGamepadButtons[(int)button] && _previousGamepadButtons[(int)button];
		}
		catch
		{
			return false;
		}
	}

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
		GameRoot.I.PropagateEvent(new KeyboardKeyEvent(arg2, true));
	}

	private void OnKeyboardKeyUp(IKeyboard arg1, Key arg2, int arg3)
	{
		if (arg2 == Key.Unknown)
		{
			return;
		}

		_keyboardKeys[(int)arg2] = false;
		GameRoot.I.PropagateEvent(new KeyboardKeyEvent(arg2, false));
	}
}
