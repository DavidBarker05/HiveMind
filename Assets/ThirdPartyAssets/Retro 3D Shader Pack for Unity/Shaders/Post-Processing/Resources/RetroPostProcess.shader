//////////////////////////////////////////////////
// Author:              LEAKYFINGERS
// Date created:        27.10.19
// Date last edited:    07.11.19
// URP (Render Graph compatible) port - drawn by RetroPostProcessFeature using Blitter.
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Post-Process"
{
	Properties
	{
		_DitherPattern("Dither Pattern", 2D) = "black" {}
	}

	HLSLINCLUDE

	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl" // Provides the fullscreen triangle vertex function (Vert), the Varyings struct and _BlitTexture.

	float _PixelScale; // The value used to scale every pixel in the image if the shader isn't using 'fixed resolution' mode (_FixedVerticalResolution set to -1).
	float _FixedVerticalResolution; // The fixed vertical resolution used to pixelate the image if the shader is using 'fixed resolution' mode (_PixelScale set to -1).
	float _SourceRenderWidth; // The width of the source rendered image in texels.
	float _SourceRenderHeight; // The height of the source rendered image in texels.

	float _ColorDepth; // The value which determines the range of the color palette applied to the image.

	TEXTURE2D(_DitherPattern); // The pattern used to implement ordered dithering.
	SAMPLER(sampler_DitherPattern);
	float4 _DitherPattern_TexelSize; // Four float values containing the texel dimensions of the dither pattern texture (1.0 / width, 1.0 / height, width, height).
	float _DitherPatternScale; // The value used to scale the dither pattern.
	float _DitherThreshold; // The threshold used to control the range of colors that are affected by the dithering effect.
	float _DitherIntensity;

	float4 Frag(Varyings i) : SV_Target
	{
		float2 uv = i.texcoord;

		// Pixelation:
		// If using fixed resolution mode, calulates the appropriate pixel scale value using the fixed vertical resolution value.
		float finalPixelScale = _PixelScale;
		if (_FixedVerticalResolution > 0)
			finalPixelScale = _SourceRenderHeight / _FixedVerticalResolution;
		float4 outputColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
		if (finalPixelScale > 1)
		{
			// The width and height percentage of each scaled pixel within the overall image (e.g. a scaled width of 0.25f is one-quarter the width of the image).
			float dx = finalPixelScale * (1.0f / _SourceRenderWidth);
			float dy = finalPixelScale * (1.0f / _SourceRenderHeight);
			float2 coord = float2(dx * floor(uv.x / dx), dy * floor(uv.y / dy)); // The texture coordinate from which to sample the original source image color for this pixel.
			outputColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, coord);
		}

		// Dithering:
		float2 screenPos = uv * float2(_SourceRenderWidth, _SourceRenderHeight); // The position of the current fragment/pixel on the screen.
		float2 ditherCoordinate = screenPos * _DitherPattern_TexelSize.xy / (_DitherPatternScale * max(finalPixelScale, 1.0f)); // The associated texture coordinate of the dither pattern texture to sample.
		float ditherValue = SAMPLE_TEXTURE2D(_DitherPattern, sampler_DitherPattern, ditherCoordinate).r;
		outputColor = outputColor + ditherValue * _DitherThreshold * _DitherIntensity;

		// Posterization:
		outputColor.rgb = floor(outputColor.rgb * (_ColorDepth * 256)) / (_ColorDepth * 256); // Quantizes the color value by multiplying it by the color depth value * 256, rounding it down, and re-dividing it.

		return outputColor;
	}

	ENDHLSL

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Name "RetroPostProcess"

			HLSLPROGRAM

			#pragma vertex Vert // The fullscreen triangle vertex function from Blit.hlsl.
			#pragma fragment Frag

			ENDHLSL
		}
	}
}
