#version 330 core

vec2 positions[4] = vec2[4](
    vec2(-1, -1), // Bottom-left
    vec2( 1, -1), // Bottom-right
    vec2( 1,  1), // Top-right
    vec2(-1,  1)  // Top-left
);

vec2 texCoords[4] = vec2[4](
    vec2(0.0, 0.0), // Bottom-left
    vec2(1.0, 0.0), // Bottom-right
    vec2(1.0, 1.0), // Top-right
    vec2(0.0, 1.0)  // Top-left
);

out vec2 texCoord;

void main(void)
{
    gl_Position = vec4(positions[gl_VertexID], 0.0, 1.0);
    texCoord = texCoords[gl_VertexID];
} 