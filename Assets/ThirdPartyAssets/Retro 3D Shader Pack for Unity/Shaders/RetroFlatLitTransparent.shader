//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		18.08.19
// Date last edited:	14.11.19
// URP (Render Graph compatible) port.
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Flat Lit (Transparent)"
{
	Properties
	{
		_MainTex("Albedo Texture", 2D) = "white" {}
		_Color("Albedo Color Tint", Color) = (1, 1, 1, 1)
		_SpecGlossMap("Specular Map", 2D) = "white" {}
		_SpecularColor("Specular Color", Color) = (0, 0, 0, 1)
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.0
		[HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
		[HDR] _EmissionMap("Emission Map", 2D) = "black" {}

		_VertJitter("Vertex Jitter", Range(0.0, 0.999)) = 0.95 // The range used to set the geometric resolution of each vertex position value in order to create a vertex jittering/snapping effect.
		_AffineMapIntensity("Affine Texture Mapping Intensity", Range(0.0, 1.0)) = 1.0 // The intensity of the affine texture mapping effect - set to 0.0 for perspective-correct texture mapping.
		_DrawDist("Draw Distance", Float) = 0 // The max draw distance from the camera to each vertex, with all vertices outside this range being clipped - set to 0 for infinite range.
	}

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }

		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha // Equivalent of the built-in 'alpha:fade' surface option.

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _LIGHT_COOKIES
			#pragma multi_compile_fog
			#pragma shader_feature_local USING_SPECULAR_MAP // Whether the shader is using the specular map or specular color.
			#pragma shader_feature_local EMISSION_ENABLED
			#pragma shader_feature_local USING_EMISSION_MAP
			#define RETRO_FLAT_NORMAL
			#include "./CG_Includes/RetroSurfaceLit.hlsl" // The include file that contains the majority of the shader code which is shared between the lit shaders.

			ENDHLSL
		}
	}

	CustomEditor "RetroFlatLitShaderCustomGUI"
}
