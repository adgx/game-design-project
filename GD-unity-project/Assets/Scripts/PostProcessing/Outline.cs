/*using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public Material outlineMaterial;
        public LayerMask outlineLayer;
    }

    public OutlineSettings settings = new OutlineSettings();

    class OutlinePass : ScriptableRenderPass
    {
        private Material outlineMat;
        private RTHandle tempRT;
        private LayerMask layerMask;
        private string profilerTag = "OutlinePass";

        public OutlinePass(Material material, LayerMask mask)
        {
            outlineMat = material;
            layerMask = mask;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        [System.Obsolete]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Allocate RTHandle with GraphicsFormat
            tempRT = RTHandles.Alloc(
                cameraTextureDescriptor.width,
                cameraTextureDescriptor.height,
                depthBufferBits: DepthBits.None,
                colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,
                filterMode: FilterMode.Bilinear,
                wrapMode: TextureWrapMode.Clamp,
                name: "_TempOutlineRT"
            );
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (outlineMat == null) return;

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            // Get camera color target handle
            RTHandle cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // Create RendererListDesc for objects on outline layer
            var rendererListDesc = new UnityEngine.Rendering.RendererUtils.RendererListDesc(
                new ShaderTagId("UniversalForward"),
                renderingData.cullResults,
                renderingData.cameraData.camera
            )
            {
                sortingCriteria = SortingCriteria.CommonTransparent,
                layerMask = layerMask
            };

            RendererList rendererList = context.CreateRendererList(rendererListDesc);

            // Draw objects to tempRT
            cmd.SetRenderTarget(tempRT);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.DrawRendererList(rendererList);

            // Blit tempRT to camera with outline material
            cmd.Blit(tempRT, cameraTarget, outlineMat, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (tempRT != null)
            {
                tempRT.Release();
                tempRT = null;
            }
        }
    }

    private OutlinePass outlinePass;

    public override void Create()
    {
        if (settings.outlineMaterial != null)
        {
            outlinePass = new OutlinePass(settings.outlineMaterial, settings.outlineLayer);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial != null)
        {
            renderer.EnqueuePass(outlinePass);
        }
    }
}
*/