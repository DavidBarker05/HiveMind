//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		02.10.19
// Date last edited:	14.11.19
// References:			http://wiki.unity3d.com/index.php/CGVertexLit
// URP (Render Graph compatible) port.
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Vertex Lit (Transparent)"
{
	Properties
	{
		_MainTex("Albedo Texture", 2D) = "white" {}
		_Color("Albedo Color Tint", Color) = (1, 1, 1, 1)
		_SpecGlossMap("Specular Map", 2D) = "white" {}
		_SpecColor("Specular Color", Color) = (0, 0, 0, 1)
		_Glossiness("Smoothness", Range(0.01, 1.0)) = 0.5
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
			Blend SrcAlpha OneMinusSrcAlpha // Specifies the equation used for blending overlapping transparent surfaces.
			ColorMask RGB

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
			#pragma shader_feature_local ENABLE_SCREENSPACE_JITTER
			#pragma shader_feature_local USING_SPECULAR_MAP // Whether the shader is using the specular map or specular color.
			#pragma shader_feature_local EMISSION_ENABLED
			#pragma shader_feature_local USING_EMISSION_MAP
			#include "./CG_Includes/RetroVertexLit.hlsl" // The include file containing the majority of the shader code which is shared between the transparent and non-transparent variants of the shader.

			ENDHLSL
		}
	}

	CustomEditor "RetroVertexLitShaderCustomGUI"
}
