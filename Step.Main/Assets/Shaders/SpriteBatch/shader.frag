#version 460 core

in flat int TexId;
in flat int GType; // 0 - quad, 1 - circle
in vec2 TexCoord;
in vec4 Color;

out vec4 outputColor;

uniform sampler2D diffuseTextures[32];

void main()
{
    float gtype = 1.0 - float(GType) * step(0.5, distance(TexCoord, vec2(0.5)));
    outputColor = texture(diffuseTextures[TexId], TexCoord) * Color * gtype;
}
