sampler2D samp2D;

int TexWidth = 300;
int TexHeight = 500;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(samp2D, texCoord);
	float2 diff = texCoord - float2(0.5, 0.5);
	float dist = (diff.x * diff.x + diff.y * diff.y) * 2;
	float mult = smoothstep(0.6, 0, dist);
	if (mult < 0.2f)
		mult = 0.2f;
	color.rgb *= 1 - ((1 - mult * 1.5f));
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}