sampler uImage0 : register(s0);
float uBrightness;
float uOpacity;

float4 PixelShaderFunction(float4 sampleColor: COLOR0, float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);

    color *= sampleColor;
    color.a *= uOpacity;
    color.rgb *= uBrightness;

    return color;
}

technique Technique1
{
    pass BrightnessPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}