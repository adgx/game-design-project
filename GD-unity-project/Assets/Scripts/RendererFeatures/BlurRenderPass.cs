using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class BlurRenderPass : ScriptableRenderPass
{
    private BlurSettings defaultSettings;
    private Material material;
    private static readonly int horizontalBlurId = Shader.PropertyToID("_HorizontalBlur");
    private static readonly int verticalBlurId = Shader.PropertyToID("_VerticalBlur");
    private const string k_BlurTextureName = "_BlurTexture";
    private const string k_VerticalPassName = "VerticalBlurRenderPass";
    private const string k_HorizontalPassName = "HorizontalBlurRenderPass";
    private TextureDesc blurTextureDescriptor;

    public BlurRenderPass(Material material, BlurSettings defaultSettings)
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

        blurTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
        blurTextureDescriptor.name = k_BlurTextureName;
        blurTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(blurTextureDescriptor);

        UpdateBlurSettings();

        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraVertical, k_VerticalPassName);

        RenderGraphUtils.BlitMaterialParameters paraHorizontal = new(dst, srcCamColor,  material, 1);
        renderGraph.AddBlitPass(paraHorizontal, k_HorizontalPassName);
    }

    public void UpdateBlurSettings()
    {
        if (material == null) return;

        material.SetFloat(horizontalBlurId, defaultSettings.horizontalBlur);
        material.SetFloat(verticalBlurId, defaultSettings.verticalBlur); 
    }
}