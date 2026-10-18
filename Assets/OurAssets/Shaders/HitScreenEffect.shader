// Fullscreen red vignette used for the player hit effect - drawn by HitScreenEffectFeature using Blitter.
Shader "Hive Mind/Hit Screen Effect"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl" // Provides the fullscreen triangle vertex function (Vert), the Varyings struct and _BlitTexture.

    float _Intensity; // 0 = no effect, 1 = full effect.
    float4 _HitColor;
    float _ClearCentre; // Distance from the screen centre (0 - 1) inside which the vignette isn't applied.
    float _Softness; // Width of the fade from clear to fully tinted.
    float _CentreTint; // How much of the effect still applies in the clear centre (0 - 1).

    float4 Frag(Varyings i) : SV_Target
    {
        float2 uv = i.texcoord;
        float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

        float2 fromCentre = (uv - 0.5) * 2.0;
        float dist = length(fromCentre) * 0.7071; // Normalised so the screen corners are 1.
        float vignette = smoothstep(_ClearCentre, _ClearCentre + max(_Softness, 0.0001), dist);
        float amount = saturate(lerp(_CentreTint, 1.0, vignette) * _Intensity);

        color.rgb = lerp(color.rgb, _HitColor.rgb, amount * _HitColor.a);
        return color;
    }

    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "HitScreenEffect"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            ENDHLSL
        }
    }
}
