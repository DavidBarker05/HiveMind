//////////////////////////////////////////////////
// URP port of RetroUnlit.cginc - shared by the Unlit, Unlit (Transparent) and Sprite (Unlit) shaders.
// Define RETRO_VERTEX_COLOR before including to multiply the output by the vertex color.
//////////////////////////////////////////////////
#ifndef _RETRO_3D_UNLIT_SHADER_
#define _RETRO_3D_UNLIT_SHADER_

#include "./RetroURPCommon.hlsl"

// A struct containing the vertex data to be passed into the vertex function.
struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	#ifdef RETRO_VERTEX_COLOR
		float4 color : COLOR;
	#endif
};

// A struct containing the fragment data (a collection of values produced by the rasterizer) to be passed into the fragment function.
struct v2f
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 uv_affine : TEXCOORD1;
	float drawDistClip : TEXCOORD2; // 0 == false, 1 == true.
	half fogFactor : TEXCOORD3;
	#ifdef RETRO_VERTEX_COLOR
		half4 color : TEXCOORD4;
	#endif
};

v2f vert(appdata v)
{
	v2f o;

	RetroVertex rv = RetroTransformVertex(v.vertex.xyz, v.uv);
	o.vertex = rv.positionCS;
	o.uv = v.uv;
	o.uv_affine = rv.uvAffine;
	o.drawDistClip = rv.drawDistClip;
	o.fogFactor = ComputeFogFactor(rv.positionCS.z);
	#ifdef RETRO_VERTEX_COLOR
		o.color = v.color;
	#endif

	return o;
}

half4 frag(v2f i) : SV_Target
{
	// Draw distance:
	if (i.drawDistClip > 0)
		clip(-1);

	float2 finalUV = RetroFinalUV(i.uv, i.uv_affine);

	half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV) * _Color;
	#ifdef RETRO_VERTEX_COLOR
		col *= i.color;
	#endif

	col.rgb = MixFog(col.rgb, i.fogFactor);

	return col;
}

#endif
