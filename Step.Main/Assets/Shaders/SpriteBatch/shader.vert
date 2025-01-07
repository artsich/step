#version 460 core

layout (location = 0) in int aTexId;
layout (location = 1) in vec2 aPos;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec4 aColor;

out flat int TexId;
out vec2 TexCoord;
out vec4 Color;

uniform mat4 viewProj = mat4(1.0);

void main(void)
{
    Color = aColor;
    TexId = aTexId;
    TexCoord = aTexCoord;
    gl_Position = viewProj * vec4(aPos, 0.0, 1.0);
}
