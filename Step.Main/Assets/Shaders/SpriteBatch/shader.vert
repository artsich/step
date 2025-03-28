#version 410 core

layout (location = 0) in int aTexId;
layout (location = 1) in int aGType;
layout (location = 2) in vec2 aPos;
layout (location = 3) in vec2 aTexCoord;
layout (location = 4) in vec4 aColor;

flat out int TexId;
flat out int GType;
out vec2 TexCoord;
out vec4 Color;

uniform mat4 viewProj = mat4(1.0);

void main(void)
{
    Color = aColor;
    TexId = aTexId;
    GType = aGType;
    TexCoord = aTexCoord;
    gl_Position = viewProj * vec4(aPos, 0.0, 1.0);
}
