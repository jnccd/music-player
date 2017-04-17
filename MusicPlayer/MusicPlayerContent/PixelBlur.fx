sampler2D samp2D;

int TexHeight = 500;
int TexWidth = 300;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 color = 0;
	float2 InvertedTexSize = float2(1.0 / TexHeight, 1.0 / TexWidth);

	color = tex2D(samp2D, texCoord) / 4;

	color += tex2D(samp2D, texCoord + float2(0, -1) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(0, 1) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(1, 0) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(-1, 0) * InvertedTexSize) / 16;

	color += tex2D(samp2D, texCoord + float2(0, -2) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(0, 2) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(2, 0) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(-2, 0) * InvertedTexSize) / 16;

	color += tex2D(samp2D, texCoord + float2(1, -1) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(1, 1) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(-1, -1) * InvertedTexSize) / 16;
	color += tex2D(samp2D, texCoord + float2(-1, 1) * InvertedTexSize) / 16;

	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}