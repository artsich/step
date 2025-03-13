#version 410 core

vec2 positions[4] = vec2[4](
    vec2(-0.5, -0.5), // Bottom-left
    vec2( 0.5, -0.5), // Bottom-right
    vec2( 0.5,  0.5), // Top-right
    vec2(-0.5,  0.5)  // Top-left
);

vec2 texCoords[4] = vec2[4](
    vec2(0.0, 0.0), // Bottom-left
    vec2(1.0, 0.0), // Bottom-right
    vec2(1.0, 1.0), // Top-right
    vec2(0.0, 1.0)  // Top-left
);

out vec2 TexCoord;

uniform mat4 viewProj = mat4(1.0);
uniform mat4 model = mat4(1.0);

void main(void)
{
    vec4 pos = vec4(positions[gl_VertexID], 0.0, 1.0);
    TexCoord = texCoords[gl_VertexID];
    gl_Position = viewProj * model * pos;
}
