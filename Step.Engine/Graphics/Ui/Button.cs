using Silk.NET.Maths;
using Step.Engine.Graphics.Text;

namespace Step.Engine.Graphics.UI;

public enum TextAlignment
{
	Left,
	Center,
	Right
}

public class Button : Control
{
	private readonly Renderer _renderer;
	private readonly Action _onClick;
	private readonly Input _input;
	private readonly Label _label;
	private bool _isHovered;
	private bool _isPressed;
	private Vector2f _padding = new(5f);
	private TextAlignment _textAlignment = TextAlignment.Left;
	private Vector2f _size;

	private static readonly Vector4f NormalColor = new(0.6f, 0.6f, 0.6f, 1.0f);
	private static readonly Vector4f HoverColor = new(0.8f, 0.8f, 0.8f, 1.0f);
	private static readonly Vector4f PressedColor = new(0.7f, 0.7f, 0.7f, 1.0f);

	public TextAlignment TextAlignment
	{
		get => _textAlignment;
		set
		{
			_textAlignment = value;
		}
	}

	public Vector4f TextColor
	{
		set
		{
			_label.Color = value;
		}
	}

	public string Text
	{
		set 
		{
			_label.Text = value;
		}
	}

	public override Vector2f Size 
	{ 
		get => _size;
		set
		{
			var baseSize = _label.Size + _padding * 2;
			_size = new Vector2f(
				Math.Max(value.X, baseSize.X),
				Math.Max(value.Y, baseSize.Y)
			);
		}
	}

	public Button(string text, Action onClick, Input input, Renderer renderer) : base(nameof(Button))
	{
		Name = text;
		_onClick = onClick;
		_input = input;
		_renderer = renderer;
		Layer = 100;

		_label = new Label(renderer)
		{
			Text = text,
			FontSize = 16f,
			Layer = 101,
		};
		AddChild(_label);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);

		switch (_textAlignment)
		{
			case TextAlignment.Left:
				_label.LocalPosition = new Vector2f(-Size.X/2f + _padding.X, -_label.Size.Y/2f);
				break;
			case TextAlignment.Center:
				_label.LocalPosition = -_label.Size/2f;
				break;
			case TextAlignment.Right:
				_label.LocalPosition = new Vector2f(Size.X/2f - _label.Size.X - _padding.X, -_label.Size.Y/2f);
				break;
		}

		var mousePos = _input.MouseWorldPosition;
		var position = GlobalPosition - (Size * 0.5f);

		_isHovered = mousePos.X >= position.X &&
					mousePos.X <= position.X + Size.X &&
					mousePos.Y >= position.Y &&
					mousePos.Y <= position.Y + Size.Y;

		if (_isHovered && _input.IsMouseButtonJustPressed(Silk.NET.Input.MouseButton.Left))
		{
			_isPressed = true;
		}
		else if (_isPressed && _input.IsMouseButtonReleased(Silk.NET.Input.MouseButton.Left))
		{
			_isPressed = false;
			if (_isHovered)
			{
				_onClick?.Invoke();
			}
		}
	}

	protected override void OnRender()
	{
		if (!Visible)
			return;

		var model = Matrix4.CreateScale(Size.X, Size.Y, 1f) * GetGlobalMatrix();

		var color = _isHovered ? (_isPressed ? PressedColor : HoverColor) : NormalColor;

		_renderer.SubmitCommand(new RenderCmd
		{
			ModelMatrix = model,
			Type = GeometryType.Quad,
			Color = color,
			Pivot = new Vector2f(0.5f),
			Layer = Layer
		});
	}
}