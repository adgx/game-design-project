using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class OutlineSettings
{
    [Range(0.0f, 10f)] public float thickness = 1f;
    [Range(0, 10f)] public float depthMin = 0f;
    [Range(0, 10f)] public float depthMax = 1f;
}

public class OutlineRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private OutlineSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private OutlineRenderPass outlineRenderPass;

    public override void Create()
    {
        if (shader == null)
            return;

        material = new Material(shader);
        outlineRenderPass = new OutlineRenderPass(material, settings);
        outlineRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (outlineRenderPass == null)
            return;

        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(outlineRenderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }

   
}