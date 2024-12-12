#version 330 core

layout(location = 0) in vec3 aPosition;

uniform mat4 viewProj = mat4(1.0);
uniform mat4 model = mat4(1.0);

void main(void)
{
	gl_Position = viewProj * model * vec4(aPosition, 1.0);
}
