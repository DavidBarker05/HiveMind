//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		18.07.19
// Date last edited:	03.11.19
// URP (Render Graph compatible) port.
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Unlit (Transparent)"
{
	Properties
	{
		_MainTex("Albedo Texture", 2D) = "white" {}
		_Color("Albedo Color Tint", Color) = (1, 1, 1, 1)

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

			Blend SrcAlpha OneMinusSrcAlpha // Specifies the equation used for blending overlapping transparent surfaces.
			ZWrite Off

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma shader_feature_local ENABLE_SCREENSPACE_JITTER
			#include "./CG_Includes/RetroUnlit.hlsl" // The include file containing the majority of the shader code which is shared between the transparent and non-transparent variants of the shader.

			ENDHLSL
		}
	}

	CustomEditor "RetroUnlitShaderCustomGUI"
}
