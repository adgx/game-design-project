using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class SobelRenderPass : ScriptableRenderPass
{
    private SobelSettings defaultSettings;
    private Material material;
    private static readonly int outlineThicknessId = Shader.PropertyToID("_OutlineThickness");
    private static readonly int outlineDepthMultiplierId = Shader.PropertyToID("_OutlineDepthMultiplier");
    private static readonly int outlineDepthBiasId = Shader.PropertyToID("_OutlineDepthBias");
    private static readonly int outlineNormalMultiplierId = Shader.PropertyToID("_OutlineNormalMultiplier");
    private static readonly int outlineNormalBiasId = Shader.PropertyToID("_OutlineNormalBias");
    private static readonly int outlineColorId = Shader.PropertyToID("_OutlineColor");
    private const string k_SobelTextureName = "_SobelTexture";
    private const string k_SobelPassName = "SobelRenderPass";
    private TextureDesc sobelTextureDescriptor;

    public SobelRenderPass(Material material, SobelSettings defaultSettings)
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

        sobelTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
        sobelTextureDescriptor.name = k_SobelTextureName;
        sobelTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(sobelTextureDescriptor);

        UpdateBlurSettings();

        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

         // --- request depth and normal textures explicitly ---
    TextureHandle depthTexture = resourceData.cameraDepthTexture;  // URP depth
    TextureHandle normalsTexture = resourceData.cameraNormalsTexture; // URP normals

    if (!depthTexture.IsValid() || !normalsTexture.IsValid())
    {
        Debug.LogWarning("SobelRenderPass: Depth or Normals texture not available.");
    }
        RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, srcCamColor, material, 0);
        renderGraph.AddBlitPass(paraVertical, k_SobelPassName);

        var copyMaterial = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
        RenderGraphUtils.BlitMaterialParameters backToCamera = new(dst, srcCamColor, copyMaterial, 0);
        renderGraph.AddBlitPass(backToCamera, k_SobelPassName + "_CopyBack");
    }

    public void UpdateBlurSettings()
    {
        if (material == null) return;

        material.SetFloat(outlineThicknessId, defaultSettings.thickness);
        material.SetFloat(outlineDepthMultiplierId, defaultSettings.depthMultiplier);
        material.SetFloat(outlineDepthBiasId, defaultSettings.depthBias);
        material.SetFloat(outlineNormalMultiplierId, defaultSettings.normalMultiplier);
        material.SetFloat(outlineNormalBiasId, defaultSettings.normalBias);
        material.SetColor(outlineColorId, defaultSettings.outlineColor);
    }
}