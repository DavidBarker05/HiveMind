//////////////////////////////////////////////////
// URP DepthOnly and ShadowCaster pass code for the opaque Retro 3D shaders.
// Define RETRO_PASS_DEPTHONLY or RETRO_PASS_SHADOWCASTER before including.
//////////////////////////////////////////////////
#ifndef _RETRO_3D_URP_DEPTH_SHADOW_
#define _RETRO_3D_URP_DEPTH_SHADOW_

#include "./RetroURPCommon.hlsl"

#if defined(RETRO_PASS_SHADOWCASTER)
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

	// For directional lights _LightDirection is used when applying the shadow normal bias, for spot and point lights it is computed from _LightPosition.
	float3 _LightDirection;
	float3 _LightPosition;
#endif

struct DepthAttributes
{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
};

struct DepthVaryings
{
	float4 positionCS : SV_POSITION;
	float drawDistClip : TEXCOORD0;
};

DepthVaryings DepthVert(DepthAttributes input)
{
	DepthVaryings o;

	#if defined(RETRO_PASS_SHADOWCASTER)
		// Shadows are cast from the un-snapped mesh, as the snapping is relative to the camera.
		float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
		float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
		#if _CASTING_PUNCTUAL_LIGHT_SHADOW
			float3 lightDirectionWS = normalize(_LightPosition - positionWS);
		#else
			float3 lightDirectionWS = _LightDirection;
		#endif
		o.positionCS = ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS)));
		o.drawDistClip = 0;
	#else
		RetroVertex v = RetroTransformVertex(input.positionOS.xyz, input.uv);
		o.positionCS = v.positionCS;
		o.drawDistClip = v.drawDistClip;
	#endif

	return o;
}

half4 DepthFrag(DepthVaryings i) : SV_Target
{
	if (i.drawDistClip > 0)
		clip(-1);
	return 0;
}

#endif
