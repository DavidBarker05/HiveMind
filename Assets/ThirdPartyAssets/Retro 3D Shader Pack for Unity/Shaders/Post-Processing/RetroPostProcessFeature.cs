//////////////////////////////////////////////////
// URP Renderer Feature that draws the Retro Effects volume component using the Render Graph.
// Add it to your URP Renderer asset (Add Renderer Feature > Retro Post Process Feature) and enable Post Processing on the camera.
//////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;


public sealed class RetroPostProcessFeature : ScriptableRendererFeature
{
    [Tooltip("Optional - the post-process shader. Leave empty to use 'Retro 3D Shader Pack/Post-Process' from the Resources folder.")]
    [SerializeField] private Shader shader;
    [Tooltip("When the effect is drawn. Anything before 'After Rendering Post Processing' is applied before tonemapping.")]
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    private Material material;
    private RetroPostProcessPass pass;

    public override void Create()
    {
        CoreUtils.Destroy(material);
        material = null;
        pass = null;

        Shader postProcessShader = shader != null ? shader : Shader.Find("Retro 3D Shader Pack/Post-Process");
        if (postProcessShader == null)
        {
            Debug.LogWarning("RetroPostProcessFeature: could not find the 'Retro 3D Shader Pack/Post-Process' shader.");
            return;
        }

        material = CoreUtils.CreateEngineMaterial(postProcessShader);
        pass = new RetroPostProcessPass(material) { renderPassEvent = renderPassEvent };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null)
            return;

        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
            return;

        RetroPostProcessEffect settings = VolumeManager.instance.stack.GetComponent<RetroPostProcessEffect>();
        if (settings == null || !settings.IsActive())
            return;

        pass.Setup(settings);
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
        material = null;
        pass = null;
    }


    private sealed class RetroPostProcessPass : ScriptableRenderPass
    {
        private class PassData
        {
            public TextureHandle source;
            public Material material;
        }

        private static readonly int PixelScaleId = Shader.PropertyToID("_PixelScale");
        private static readonly int FixedVerticalResolutionId = Shader.PropertyToID("_FixedVerticalResolution");
        private static readonly int SourceRenderWidthId = Shader.PropertyToID("_SourceRenderWidth");
        private static readonly int SourceRenderHeightId = Shader.PropertyToID("_SourceRenderHeight");
        private static readonly int ColorDepthId = Shader.PropertyToID("_ColorDepth");
        private static readonly int DitherPatternId = Shader.PropertyToID("_DitherPattern");
        private static readonly int DitherPatternScaleId = Shader.PropertyToID("_DitherPatternScale");
        private static readonly int DitherThresholdId = Shader.PropertyToID("_DitherThreshold");
        private static readonly int DitherIntensityId = Shader.PropertyToID("_DitherIntensity");

        private const string PassName = "Retro Post Process";

        private readonly Material material;
        private RetroPostProcessEffect settings;

        public RetroPostProcessPass(Material material)
        {
            this.material = material;
        }

        public void Setup(RetroPostProcessEffect settings)
        {
            this.settings = settings;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // The active color target can't be sampled while it is the back buffer (i.e. nothing rendered to an intermediate texture, such as when post processing is disabled on the camera).
            if (resourceData.isActiveTargetBackBuffer)
                return;

            UpdateMaterial(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);

            TextureHandle source = resourceData.activeColorTexture;
            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_RetroPostProcessTexture", false);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(PassName, out PassData passData))
            {
                passData.source = source;
                passData.material = material;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0); // Applies the shader to the source image and outputs the result to the destination.
                });
            }

            resourceData.cameraColor = destination;
        }

        // Passes the volume settings to the post-process shader.
        private void UpdateMaterial(int width, int height)
        {
            // Depending on whether fixed resolution mode is being used, passes either the resolution or scaling value and sets the other to -1.
            if (settings.UsesFixedResolution.value)
            {
                material.SetFloat(PixelScaleId, -1);
                material.SetFloat(FixedVerticalResolutionId, settings.FixedVerticalResolution.value);
            }
            else
            {
                material.SetFloat(PixelScaleId, settings.PixelScale.value);
                material.SetFloat(FixedVerticalResolutionId, -1);
            }
            material.SetFloat(SourceRenderWidthId, width);
            material.SetFloat(SourceRenderHeightId, height);

            material.SetFloat(ColorDepthId, settings.ColorDepth.value);

            Texture ditherPattern = settings.DitherPattern.value;
            material.SetTexture(DitherPatternId, ditherPattern);
            material.SetFloat(DitherPatternScaleId, settings.DitherPatternScale.value);
            material.SetFloat(DitherThresholdId, settings.DitherThreshold.value);
            material.SetFloat(DitherIntensityId, ditherPattern != null ? settings.DitherIntensity.value : 0.0f); // No dithering without a pattern.
        }
    }
}
