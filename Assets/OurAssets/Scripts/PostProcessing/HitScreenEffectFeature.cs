using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public sealed class HitScreenEffectFeature : ScriptableRendererFeature
{
    [Tooltip("Optional - the effect shader. Leave empty to use 'Hive Mind/Hit Screen Effect'.")]
    [SerializeField] private Shader shader;
    [Tooltip("When the effect is drawn.")]
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    private Material material;
    private HitScreenEffectPass pass;

    public override void Create()
    {
        CoreUtils.Destroy(material);
        material = null;
        pass = null;

        Shader effectShader = shader != null ? shader : Shader.Find("Hive Mind/Hit Screen Effect");
        if (effectShader == null)
        {
            Debug.LogWarning("HitScreenEffectFeature: could not find the 'Hive Mind/Hit Screen Effect' shader.");
            return;
        }

        material = CoreUtils.CreateEngineMaterial(effectShader);
        pass = new HitScreenEffectPass(material) { renderPassEvent = renderPassEvent };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null)
            return;

        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
            return;

        HitScreenEffect settings = VolumeManager.instance.stack.GetComponent<HitScreenEffect>();
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


    private sealed class HitScreenEffectPass : ScriptableRenderPass
    {
        private class PassData
        {
            public TextureHandle source;
            public Material material;
        }

        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int HitColorId = Shader.PropertyToID("_HitColor");
        private static readonly int ClearCentreId = Shader.PropertyToID("_ClearCentre");
        private static readonly int SoftnessId = Shader.PropertyToID("_Softness");
        private static readonly int CentreTintId = Shader.PropertyToID("_CentreTint");

        private const string PassName = "Hit Screen Effect";

        private readonly Material material;
        private HitScreenEffect settings;

        public HitScreenEffectPass(Material material)
        {
            this.material = material;
        }

        public void Setup(HitScreenEffect settings)
        {
            this.settings = settings;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // The active color target can't be sampled while it is the back buffer (i.e. post processing is disabled on the camera).
            if (resourceData.isActiveTargetBackBuffer)
                return;

            material.SetFloat(IntensityId, settings.Intensity.value);
            material.SetColor(HitColorId, settings.Color.value);
            material.SetFloat(ClearCentreId, settings.ClearCentre.value);
            material.SetFloat(SoftnessId, settings.Softness.value);
            material.SetFloat(CentreTintId, settings.CentreTint.value);

            TextureHandle source = resourceData.activeColorTexture;
            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_HitScreenEffectTexture", false);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(PassName, out PassData passData))
            {
                passData.source = source;
                passData.material = material;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            resourceData.cameraColor = destination;
        }
    }
}
