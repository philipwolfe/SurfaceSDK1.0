float4x4 WorldViewProj;
texture UserTexture;
float TextureAlpha = 1.00;

sampler2D TextureSampler = sampler_state 
{
    texture = <UserTexture>;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

struct PS_INPUT
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

PS_INPUT VertexShader(float4 pos : POSITION0, float2 texCoord : TEXCOORD0)
{
	PS_INPUT Output = (PS_INPUT)0;
	Output.Position = mul(pos, WorldViewProj);
	Output.TexCoord = texCoord;

	return Output;
}

float4 PixelShader(PS_INPUT input) : COLOR0
{
	float2 texCoord = input.TexCoord.xy;
	float4 texColor = tex2D(TextureSampler, texCoord);
	return float4(texColor.xyz, texColor.w * TextureAlpha);
}

technique TextureMapping
{
    pass P0
    {      
        vertexShader = compile vs_2_0 VertexShader();
        pixelShader = compile ps_2_0 PixelShader();
    }
}


