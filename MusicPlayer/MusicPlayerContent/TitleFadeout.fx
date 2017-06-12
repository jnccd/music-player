sampler2D samp2D;

int TexWidth = 300;
int TexHeight = 500;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(samp2D, texCoord);
	float2 InvertedTexSize = float2(1.0 / TexHeight, 1.0 / TexWidth);

	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}