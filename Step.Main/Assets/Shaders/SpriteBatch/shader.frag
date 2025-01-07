#version 460 core

in flat int TexId;
in vec2 TexCoord;
in vec4 Color;

out vec4 outputColor;

uniform sampler2D diffuseTextures[32];

void main()
{
    outputColor = texture(diffuseTextures[TexId], TexCoord) * Color;
}
