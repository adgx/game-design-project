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

    [SerializeField] private AnimationCurve brightnessCurve;

    [SerializeField] private LayerMask whatIsPlayer;

    private Renderer renderer;
    private List<Material> materials = new List<Material>();
    private List<Color> initialColors = new List<Color>();
    private bool pulse = false;

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

            for(int i = 0; i < materials.Count; i++) {

                float brightness = brightnessCurve.Evaluate(scaledTime);

                materials[i].SetColor(EMISSIVE_COLOR_NAME, initialColors[i] * brightness);
            }
        }
        else {
			for(int i = 0; i < materials.Count; i++) {
				materials[i].SetColor(EMISSIVE_COLOR_NAME, initialColors[i] * 0);
			}
		}
    }
}
