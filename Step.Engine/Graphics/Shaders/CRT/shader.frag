#version 330 core

uniform sampler2D sourceTexture;
uniform float distortion = 0.04;
uniform float bendScale = 1.5;
uniform float dispersion = 0.0011;
uniform float vignetteIntensity = 1.5;
uniform float vignetteRoundness = 0.5;
uniform vec2 vignetteTarget = vec2(0.5);
uniform vec2 texSize = vec2(1.0);

uniform float time;

in vec2 texCoord;
out vec4 fragColor;

// TODO: Looks not perfect, try to make it proportional
vec2 applyBarrelDistortion(vec2 uv) {
    vec2 disp = uv - vec2(0.5);
    float dist2 = dot(disp, disp);
    disp *= dist2 * distortion * bendScale;
    return uv + disp;
}


vec3 applyChromaticAberration(vec2 uv)
{
    vec2 uvR = uv + vec2(dispersion, 0.0);
    vec2 uvG = uv;
    vec2 uvB = uv - vec2(dispersion, 0.0);

    float r = texture(sourceTexture, uvR).r;
    float g = texture(sourceTexture, uvG).g;
    float b = texture(sourceTexture, uvB).b;

    return vec3(r, g, b);
}

float calculateEdgeFade(vec2 uv)
{
    float edgeX = 0.009;
    float edgeY = 0.009;
    
    float fade = 1.0;
    fade *= smoothstep(0.0, edgeX, uv.x) * smoothstep(1.0, 1.0 - edgeX, uv.x);
    fade *= smoothstep(0.0, edgeY, uv.y) * smoothstep(1.0, 1.0 - edgeY, uv.y);
    
    return fade;
}

float calculateVignette(vec2 uv)
{
    vec2 position = (uv - vignetteTarget) * vec2(texSize.x/texSize.y, 1.0);
    
    float vignette = length(position);
    vignette = pow(vignette, vignetteRoundness);
    vignette = smoothstep(0.0, vignetteIntensity, vignette);
    return 1.0 - vignette;
}

float calculateGrain(vec2 uv)
{
    // TODO: can be calculated on CPU.
    float frameTime = floor(time * 30.0) / 30.0;

    return mix(1.0, fract(sin(dot(uv*texSize + frameTime*10.0, 
                        vec2(12.9898,78.233)))*43758.5453), 0.12);
}

void main()
{
    vec2 uv = applyBarrelDistortion(texCoord);
    vec3 color = applyChromaticAberration(uv);
    float fade = calculateEdgeFade(uv);
    float vignette = calculateVignette(uv);
    float grain = calculateGrain(uv);

    color *= fade * vignette * grain;
    fragColor = vec4(color, 1.0);
}