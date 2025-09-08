using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class OutlineRenderPass : ScriptableRenderPass
{
    private OutlineSettings defaultSettings;
    private Material material;
    private static readonly int minDepthId = Shader.PropertyToID("_MinDepth");
    private static readonly int maxDepthId = Shader.PropertyToID("_MaxDepth");
    private static readonly int thicknessId = Shader.PropertyToID("_Thickness");
    private const string k_OutlineTextureName = "_OutlineTexture";
    private const string k_OutlinePassName = "OutlinePass";
    private TextureDesc outlineTextureDescriptor;

    public OutlineRenderPass(Material material, OutlineSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        TextureHandle srcCamColor = resourceData.activeColorTexture;

        if (resourceData.isActiveTargetBackBuffer)
            return;

        outlineTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
        outlineTextureDescriptor.name = k_OutlineTextureName;
        outlineTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(outlineTextureDescriptor);

        UpdateBlurSettings();

        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraVertical, k_OutlinePassName);

        var copyMaterial = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
        RenderGraphUtils.BlitMaterialParameters backToCamera = new(dst, srcCamColor, copyMaterial, 0);
        renderGraph.AddBlitPass(backToCamera, "_CopyBack");
    }

    public void UpdateBlurSettings()
    {
        if (material == null) return;

        material.SetFloat(minDepthId, defaultSettings.depthMin);
        material.SetFloat(maxDepthId, defaultSettings.depthMax);
        material.SetFloat(thicknessId, defaultSettings.thickness);
    }
}