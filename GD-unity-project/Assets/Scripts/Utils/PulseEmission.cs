using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class PulseEmission : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    private float flickerSpeed = 1.0f;
    private float intensityRate = 1.5f; 

    [SerializeField] private AnimationCurve brightnessCurve;

    [SerializeField] private LayerMask whatIsPlayer;

    private new Renderer renderer;
    private List<Material> materials = new List<Material>();
    private List<Color> initialColors = new List<Color>();

    private const string EMISSIVE_COLOR_NAME = "_EmissionColor";
	private const string EMISSIVE_KEYWORD = "_EMISSION";

	private void Awake() {
		renderer = GetComponent<Renderer>();
        brightnessCurve.postWrapMode = WrapMode.Loop;

        foreach(Material material in renderer.materials) {
            if(renderer.material.enabledKeywords.Any(item => item.name == EMISSIVE_KEYWORD)
                && renderer.material.HasColor(EMISSIVE_COLOR_NAME)) {
                materials.Add(material);
                initialColors.Add(material.GetColor(EMISSIVE_COLOR_NAME));
            }
            else {
                Debug.LogWarning($"{material.name} is not configured to be emissive, so it can not be animated");
            }
        }
	}

	// Update is called once per frame
	void Update()
    {
        if(renderer.isVisible && Physics.CheckSphere(transform.position, 6f, whatIsPlayer) && !Physics.CheckSphere(transform.position, 2f, whatIsPlayer)) {
            float scaledTime = Time.time * flickerSpeed;

            for (int i = 0; i < materials.Count; i++)
            {

                float brightness = brightnessCurve.Evaluate(scaledTime);
                float intensityHDR = Mathf.Pow(2, brightness * intensityRate);
                materials[i].SetColor(EMISSIVE_COLOR_NAME, Color.white * intensityHDR);
                materials[i].EnableKeyword(EMISSIVE_KEYWORD);
            }
        }
        else {
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetColor(EMISSIVE_COLOR_NAME, Color.white * 0);
                materials[i].EnableKeyword(EMISSIVE_KEYWORD);
			}
		}
    }
}
