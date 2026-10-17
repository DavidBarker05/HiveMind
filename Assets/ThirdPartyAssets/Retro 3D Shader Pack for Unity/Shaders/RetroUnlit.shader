//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		09.07.19
// Date last edited:	14.11.19
// URP (Render Graph compatible) port.
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Unlit"
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
		Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fog
			#pragma shader_feature_local ENABLE_SCREENSPACE_JITTER
			#include "./CG_Includes/RetroUnlit.hlsl" // The include file containing the majority of the shader code which is shared between the transparent and non-transparent variants of the shader.

            ENDHLSL
        }

		Pass
		{
			Name "DepthOnly"
			Tags { "LightMode" = "DepthOnly" }

			ZWrite On
			ColorMask R

			HLSLPROGRAM

			#pragma vertex DepthVert
			#pragma fragment DepthFrag
			#pragma shader_feature_local ENABLE_SCREENSPACE_JITTER
			#define RETRO_PASS_DEPTHONLY
			#include "./CG_Includes/RetroURPDepthShadow.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual
			ColorMask 0

			HLSLPROGRAM

			#pragma vertex DepthVert
			#pragma fragment DepthFrag
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
			#define RETRO_PASS_SHADOWCASTER
			#include "./CG_Includes/RetroURPDepthShadow.hlsl"

			ENDHLSL
		}
    }

	CustomEditor "RetroUnlitShaderCustomGUI"
}
