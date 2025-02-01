#version 330

#define PI 3.14159265
#define TWO_PI PI * 2.0

in vec2 TexCoord;
in vec4 Color;

out vec4 outputColor;

uniform float arcAngle = 0.0;

void main()
{
    vec2 uv = TexCoord - vec2(0.5);
    float radius = 0.49;
    float thickness = 0.02;

    float d = length(uv);
    float a = atan(uv.y, uv.x);

    float da = mod(a - arcAngle + PI, TWO_PI) - PI;

    float halfArc = PI / 4.0;
    float arcMask = smoothstep(halfArc, halfArc - 0.01, abs(da));
    float circleMask = smoothstep(radius + thickness, radius, d) * 
                       smoothstep(radius - thickness, radius, d);

    vec3 color = vec3(arcMask * circleMask);
    outputColor = vec4(color, arcMask * circleMask) * Color;
}
