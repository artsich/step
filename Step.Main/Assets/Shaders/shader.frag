#version 410 core

out vec4 outputColor;

in vec2 TexCoord;

uniform vec4 color = vec4(1.0, 0.0, 0.0, 1.0);
uniform sampler2D diffuseTexture;

void main()
{
	outputColor = texture(diffuseTexture, TexCoord) * color;
}
