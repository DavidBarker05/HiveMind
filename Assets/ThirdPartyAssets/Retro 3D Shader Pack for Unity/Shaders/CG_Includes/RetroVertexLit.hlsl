//////////////////////////////////////////////////
// URP port of RetroVertexLit.cginc.
// URP has no 'Vertex' light mode, so the lights (main + additional) are accumulated per-vertex in the vertex function.
//////////////////////////////////////////////////
#ifndef _RETRO_3D_VERTEX_LIT_SHADER_
#define _RETRO_3D_VERTEX_LIT_SHADER_

#include "./RetroURPCommon.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 texcoord : TEXCOORD0;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 uv_affine : TEXCOORD1;
	half3 diffuse : TEXCOORD2;
	half3 specular : TEXCOORD3;
	float drawDistClip : TEXCOORD4; // 0 == false, 1 == true.
	half fogFactor : TEXCOORD5;
};

// Adds the diffuse and specular contribution of a light to the running totals.
void RetroAccumulateLight(Light light, float3 normalWS, float3 viewDirWS, inout half3 diffuse, inout half3 specular)
{
	half atten = light.distanceAttenuation;
	half3 lightColor = light.color * atten;

	diffuse += lightColor * saturate(dot(normalWS, light.direction));

	float3 h = normalize(viewDirWS + light.direction);
	float nh = saturate(dot(normalWS, h));
	specular += lightColor * (pow(nh, _Glossiness * 128.0) * 0.5);
}

v2f vert(appdata v)
{
	v2f o;

	RetroVertex rv = RetroTransformVertex(v.vertex.xyz, v.texcoord);
	o.vertex = rv.positionCS;
	o.uv = v.texcoord;
	o.uv_affine = rv.uvAffine;
	o.drawDistClip = rv.drawDistClip;

	// Lighting:
	float3 normalWS = normalize(TransformObjectToWorldNormal(v.normal));
	float3 viewDirWS = normalize(GetCameraPositionWS() - rv.positionWS);

	half3 diffuse = SampleSH(normalWS); // The initial base diffuse color is the ambient light.
	half3 specular = 0;

	RetroAccumulateLight(GetMainLight(), normalWS, viewDirWS, diffuse, specular);

	#if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX) || USE_CLUSTER_LIGHT_LOOP
		InputData inputData = (InputData)0; // Required by LIGHT_LOOP_BEGIN when using the clustered light loop.
		inputData.positionWS = rv.positionWS;
		inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(rv.positionCS);
		uint lightCount = GetAdditionalLightsCount();
		LIGHT_LOOP_BEGIN(lightCount)
			RetroAccumulateLight(GetAdditionalLight(lightIndex, rv.positionWS), normalWS, viewDirWS, diffuse, specular);
		LIGHT_LOOP_END
	#endif

	float4 emissionParameter = float4(0, 0, 0, 0);
	#ifdef EMISSION_ENABLED
		emissionParameter = _EmissionColor;
		#ifdef USING_EMISSION_MAP
			emissionParameter *= SAMPLE_TEXTURE2D_LOD(_EmissionMap, sampler_EmissionMap, v.texcoord, 0);
		#endif
	#endif
	o.diffuse = (diffuse * _Color.rgb + emissionParameter.rgb) * 2;

	float4 specularParameter = _SpecColor;
	#ifdef USING_SPECULAR_MAP
		specularParameter = SAMPLE_TEXTURE2D_LOD(_SpecGlossMap, sampler_SpecGlossMap, v.texcoord, 0);
	#endif
	o.specular = specular * specularParameter.rgb * 2;

	o.fogFactor = ComputeFogFactor(rv.positionCS.z);

	return o;
}

half4 frag(v2f i) : SV_Target
{
	// Draw distance:
	if (i.drawDistClip > 0)
		clip(-1);

	float2 finalUV = RetroFinalUV(i.uv, i.uv_affine);

	half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);
	col.rgb = col.rgb * i.diffuse + i.specular;
	col.a = col.a * _Color.a;

	col.rgb = MixFog(col.rgb, i.fogFactor);

	return col;
}

#endif
