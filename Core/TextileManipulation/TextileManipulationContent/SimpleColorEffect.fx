float4x4 WorldViewProj;
float TextureAlpha = 1.0;

struct PS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

PS_INPUT VS(float4 pos : POSITION0, float4 color : COLOR0)
{
    PS_INPUT output = (PS_INPUT)0;
    output.Position = mul(pos, WorldViewProj);
    output.Color = color;
    return output;
}

float4 PS(PS_INPUT input) : COLOR0
{
    return float4(input.Color.xyz, input.Color.w * TextureAlpha);
}

technique PointSpriteTechnique
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
