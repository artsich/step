#version 430

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(binding = 0) uniform sampler2D texture1;
layout(binding = 1) uniform sampler2D texture2;
layout(binding = 2, rgba8) uniform writeonly image2D outputImage;

uniform float alphaThreshold = 1.0;

void main() {
    ivec2 pos = ivec2(gl_GlobalInvocationID.xy);
    ivec2 size = imageSize(outputImage);

    if (pos.x >= size.x || pos.y >= size.y) return;

    vec2 texCoord = (vec2(pos) + 0.5) / vec2(size);

    vec4 color1 = texture(texture1, texCoord);
    vec4 color2 = texture(texture2, texCoord);

    float blendFactor = step(alphaThreshold, color2.a);

    vec4 finalColor = mix(color1, color2, blendFactor);

    imageStore(outputImage, pos, finalColor);
}