//////////////////////////////////////////////////
// URP port of the Retro 3D Shader Pack shared code.
// Holds the material properties and the retro vertex helpers (vertex snapping, affine texture mapping, draw distance)
// used by every shader in the pack.
//////////////////////////////////////////////////
#ifndef _RETRO_3D_URP_COMMON_
#define _RETRO_3D_URP_COMMON_

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_MainTex);		SAMPLER(sampler_MainTex);
TEXTURE2D(_SpecGlossMap);	SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_EmissionMap);	SAMPLER(sampler_EmissionMap);
TEXTURE2D(_BumpMap);		SAMPLER(sampler_BumpMap);

// All the shaders in the pack share this layout so that every pass is SRP Batcher compatible. Properties a shader doesn't declare simply stay unused.
CBUFFER_START(UnityPerMaterial)
	float4 _MainTex_ST; // The tiling (x, y) and offset (z, w) values for the albedo texture (also used for the other maps).
	float4 _Color;
	float4 _SpecColor; // Specular color used by the Vertex Lit shaders.
	float4 _SpecularColor; // Specular color used by the Flat Lit, Unity Lit and Sprite Lit shaders.
	float _Glossiness;
	float4 _EmissionColor;
	float _VertJitter;
	float _AffineMapIntensity;
	float _DrawDist;
CBUFFER_END

struct RetroVertex
{
	float4 positionCS;
	float3 positionWS; // The (snapped) world-space position.
	float3 positionVS; // The (snapped) view-space position.
	float3 uvAffine;
	float drawDistClip; // Whether the vertex is outside the draw distance: 0 == false, 1 == true.
};

// Quantizes the position of the vertex in screen space according to the _VertJitter value and returns it - vertex pos must be in clip space first.
float4 RetroScreenSnap(float4 vertex)
{
	float geoRes = _VertJitter * 125.0f + 1.0f; // Transforms the vertex jitter value from 0.0f (no effect) - 1.0f (full effect) into 1.0f (no effect) - 126.0f (full effect).
	float2 pixelPos = round((vertex.xy / vertex.w) * _ScreenParams.xy / geoRes) * geoRes; // Transforms the vertex using the perspective divide to find its pixel location, then quantizes it.
	vertex.xy = pixelPos / _ScreenParams.xy * vertex.w; // Transforms the quantized pixel position back into clip space.
	return vertex;
}

// Applies vertex snapping, affine mapping data and draw distance data to an object-space vertex.
RetroVertex RetroTransformVertex(float3 positionOS, float2 uv)
{
	RetroVertex o;

	float3 originalWS = TransformObjectToWorld(positionOS);
	float3 viewPos = TransformWorldToView(originalWS);

	// Vertex snapping:
	#ifdef ENABLE_SCREENSPACE_JITTER
		o.positionWS = originalWS;
		o.positionVS = viewPos;
		o.positionCS = RetroScreenSnap(TransformWorldToHClip(originalWS));
	#else
		float geoRes = (_VertJitter - 1.0f) * -1000.0f; // Transforms the vertex jitter value from 0.0f (no effect) - 1.0f (full effect) into 1000.0f (no effect) - 0.0f (full effect).
		viewPos = floor(viewPos * geoRes) / geoRes; // Quantizes the view-space vertex position.
		o.positionVS = viewPos;
		o.positionWS = mul(UNITY_MATRIX_I_V, float4(viewPos, 1.0)).xyz;
		o.positionCS = mul(GetViewToHClipMatrix(), float4(viewPos, 1.0));
	#endif

	// Affine texture mapping:
	float wVal = mul(GetViewToHClipMatrix(), o.positionCS).z;
	o.uvAffine = float3(uv * wVal, wVal);

	// Draw distance:
	o.drawDistClip = (_DrawDist != 0 && distance(originalWS, GetCameraPositionWS()) > _DrawDist) ? 1 : 0;

	return o;
}

// Blends between the perspective-correct and affine texture coordinates according to the _AffineMapIntensity value.
float2 RetroFinalUV(float2 uv, float3 uvAffine)
{
	float2 correctUV = TRANSFORM_TEX(uv, _MainTex);
	float2 affineUV = TRANSFORM_TEX((uvAffine / uvAffine.z).xy, _MainTex);
	return lerp(correctUV, affineUV, _AffineMapIntensity);
}

#endif
