#version 330

out vec4 outputColor;

uniform vec4 color = vec4(1.0, 0.0, 0.0, 1.0);

void main()
{
	outputColor = color;
}
