sampler2D samp2D;

int TexWidth = 300;
int TexHeight = 500;

// OLD
/*const float BlurWeights[7] =
{
	0.064759,
	0.120985,
	0.176033,
	0.199471,
	0.176033,
	0.120985,
	0.064759,
};*/

const float BlurWeights[15] = {
	0.0151,
	0.0311,
	0.0574,
	0.0946,
	0.1397,
	0.1844,
	0.2178,
	0.2599,
	0.2178,
	0.1844,
	0.1397,
	0.0946,
	0.0574,
	0.0311,
	0.0151
};

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 color = 0;
	float2 InvertedTexSize = float2(1.0 / TexHeight, 1.0 / TexWidth);

	for (int i = 0; i < 15; i++)
	{
		color += tex2D(samp2D, texCoord + float2(i - 7, 0) * InvertedTexSize) * BlurWeights[i];
	}
	color.rgb /= 1.8f;

	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}