//////////////////////////////////////////////////
// Author:              LEAKYFINGERS
// Date created:        27.10.19
// Date last edited:    09.11.19
// URP (Render Graph compatible) port: the effect settings are now a Volume component, rendered by RetroPostProcessFeature.
//////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[Serializable]
[VolumeComponentMenu("Retro 3D/Retro Effects")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class RetroPostProcessEffect : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Whether the pixelation effect uses a fixed vertical resolution or a pixel size multiplier value.")]
    public BoolParameter UsesFixedResolution = new BoolParameter(false);
    [Tooltip("The value used to scale the width and height of each pixel")]
    public MinIntParameter PixelScale = new MinIntParameter(4, 1);
    [Tooltip("The vertical resolution of the image output by the pixelation effect.")]
    public MinIntParameter FixedVerticalResolution = new MinIntParameter(480, 1);

    [Tooltip("Determines the range of the color palette applied to the image.")]
    public ClampedFloatParameter ColorDepth = new ClampedFloatParameter(0.15f, 0.0f, 1.0f);

    [Tooltip("The pattern used to implement ordered dithering.")]
    public TextureParameter DitherPattern = new TextureParameter(null);
    [Tooltip("The scale multiplier for the dither pattern (the dither pattern size is also automatically scaled according to the pixelation effect scaling).")]
    public MinIntParameter DitherPatternScale = new MinIntParameter(1, 1);
    [Tooltip("The threshold used to control the range of colors that are affected by dithering.")]
    public ClampedFloatParameter DitherThreshold = new ClampedFloatParameter(0.75f, 0.0f, 1.0f);
    [Tooltip("The intensity of the dithering effect.")]
    public ClampedFloatParameter DitherIntensity = new ClampedFloatParameter(0.15f, 0.0f, 1.0f);

    // The effect is only rendered once at least one of its parameters is overridden in a Volume.
    public bool IsActive() => active && (UsesFixedResolution.overrideState || PixelScale.overrideState || FixedVerticalResolution.overrideState || ColorDepth.overrideState || DitherPattern.overrideState);
}
