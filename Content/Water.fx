float4x4 World;
float4x4 WVP;
float3 CamPos;
float3 SunDir;
Texture2D NormMap;
float T;
Texture2D ReflectionMap;

sampler normalMapSampler = sampler_state     
{
	Texture = <NormMap>;
	//FILTER = MIN_MAG_MIP_LINEAR;
	//AddressU = Wrap;
	//AddressV = Wrap;
};

sampler reflectionMapSampler = sampler_state
{
	Texture = <ReflectionMap>;
	//FILTER = MIN_MAG_MIP_LINEAR;
	//AddressU = Wrap;
	//AddressV = Wrap;
};



struct VS_IN
{
	float4 Position : POSITION0;
};

struct VS_OUT
{
	float4 Position : POSITION0;
	float3 WorldPos : TEXCOORD0;
	float4 SSPos : TEXCOORD1;
};

VS_OUT VS(float3 pos: SV_POSITION)
{
	VS_OUT output;
	output.Position = mul(float4(pos,1), WVP);
	output.WorldPos = mul(float4(pos, 1),  World).xyz;
	output.SSPos = output.Position;
	return output;
}

float4 PS(VS_OUT input) : COLOR0
{
	float4 vpos = input.SSPos / input.SSPos.w;
	float4 refl =  tex2D(reflectionMapSampler, (vpos+1)/2);

	float3 tn1 = tex2D(normalMapSampler, input.WorldPos.xz / 20 + float2(T*0.01, T*0.01));
	float3 tn2 = tex2D(normalMapSampler, input.WorldPos.xz / 10 + float2(0, T*0.02))/2;
	float3 n = normalize(float3(tn1.x + tn2.y, 4, tn1.y + tn2.y) - 1);

	float3 v = normalize(CamPos - input.WorldPos);
	float3 l = -SunDir;
	float3 h = normalize(v + l);
	float3 c = refl*0.2 + float3(0.1, 0.1, 0.2) + dot(n, l)* float3(0.4, 0.8, 1) + pow(dot(h, n), 50)*float3(1, 1, 0.8);
	return float4(c, 1);
}

technique Water
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
}