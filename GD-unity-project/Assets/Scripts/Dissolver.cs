/*
using System;
using System.Collections;
using UnityEngine;

public class Dissolver : MonoBehaviour
{
    [SerializeField] private float _dissolveRate = 0.0125f;
    [SerializeField] private float _refreshRate = 0.025f;
    private Renderer _renderer = null; 
    private Material _dissolveMaterial = null;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _dissolveMaterial = _renderer.material;
            if (_dissolveMaterial == null)
                Debug.LogError("Material not found");
        }
        else Debug.LogError("Renderer component not found");

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventSystem.Instance.EventToDissolve += DissolveLuncher;
        Debug.Log($"ID: {GetInstanceID()}, Name:{ToString()}");
    }

    void DissolveLuncher(object sender, EventArgs args)
    {
        StartCoroutine(Dissolve());
    }

    IEnumerator Dissolve()
    {
        float counter = 0;

        while (_dissolveMaterial.GetFloat("_DissolveAmount") < 1)
        {
            counter += _dissolveRate;
            _dissolveMaterial.SetFloat("_DissolveAmount", counter);
            yield return new WaitForSeconds(_refreshRate);
        }
    }
}
*/