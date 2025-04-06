#version 430

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(binding = 0) uniform sampler2D inputTexture;
layout(binding = 1, rgba8) uniform writeonly image2D outputImage;

const float TwoPi = 6.28318530718;

uniform float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
uniform float Quality = 3.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
uniform float Size = 8.0; // BLUR SIZE (Radius)

void main() {
	ivec2 pos = ivec2(gl_GlobalInvocationID.xy);
	ivec2 size = imageSize(outputImage);

	if (pos.x >= size.x || pos.y >= size.y) return;
	
	vec2 texCoord = (vec2(pos) + 0.5) / vec2(size);
	vec2 texOffset = 1.0 / vec2(size);
	
	vec2 radius = Size * texOffset;
	
	vec4 color = texture(inputTexture, texCoord);

	for(float d = 0.0; d < TwoPi; d += TwoPi/Directions)
	{
		for(float i = 1.0/Quality; i <= 1.0; i += 1.0/Quality)
		{
			vec2 offset = vec2(cos(d), sin(d)) * radius * i;
			color += texture(inputTexture, texCoord + offset);
		}
	}
	
	color /= Quality * Directions - 15.0;
	
	imageStore(outputImage, pos, color);
}
