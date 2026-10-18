using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Hive Mind/Hit Screen Effect")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class HitScreenEffect : VolumeComponent, IPostProcessComponent
{
    [Tooltip("How strongly the effect is applied. PlayerHitScreenEffect drives this at runtime.")]
    public ClampedFloatParameter Intensity = new(0f, 0f, 1f);
    [Tooltip("The colour the screen is tinted towards (alpha scales the strength).")]
    public ColorParameter Color = new(new Color(0.8f, 0f, 0f, 1f), false, true, false);
    [Tooltip("How far from the centre of the screen (0 - 1) the tint starts.")]
    public ClampedFloatParameter ClearCentre = new(0.35f, 0f, 1f);
    [Tooltip("How wide the fade from clear to fully tinted is.")]
    public ClampedFloatParameter Softness = new(0.5f, 0f, 1f);
    [Tooltip("How much of the tint also covers the middle of the screen.")]
    public ClampedFloatParameter CentreTint = new(0.05f, 0f, 1f);

    public bool IsActive() => active && Intensity.value > 0f;
}
