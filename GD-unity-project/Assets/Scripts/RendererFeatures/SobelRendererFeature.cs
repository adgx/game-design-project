using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class SobelSettings
{
    [Range(0.0f, 5f)] public float thickness = 1.0f;
    [Range(0, 5f)] public float depthMultiplier = 1.0f;
    [Range(0, 5f)] public float depthBias = 1.0f;
    [Range(0, 5f)] public float normalMultiplier = 1.0f;
    [Range(0, 20f)] public float normalBias = 10.0f;
    public Color outlineColor = Color.white;
}

public class SobelRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private SobelSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private SobelRenderPass sobelRenderPass;

    public override void Create()
    {
        if (shader == null)
            return;

        material = new Material(shader);
        sobelRenderPass = new SobelRenderPass(material, settings);
        sobelRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (sobelRenderPass == null)
            return;

        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(sobelRenderPass);
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