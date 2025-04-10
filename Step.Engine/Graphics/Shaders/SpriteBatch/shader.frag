#version 410 core

flat in int TexId;
flat in int GType; // 0 - quad, 1 - circle
in vec2 TexCoord;
in vec4 Color;

out vec4 outputColor;

uniform sampler2D diffuseTextures[16]; // for mac 16 max

void main()
{
    float gtype = 1.0 - float(GType) * step(0.5, distance(TexCoord, vec2(0.5)));
    outputColor = texture(diffuseTextures[TexId], TexCoord) * Color * gtype;
}
