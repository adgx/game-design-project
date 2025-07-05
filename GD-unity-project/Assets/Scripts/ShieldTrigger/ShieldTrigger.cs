using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.VFX;

public class ShieldTrigger : MonoBehaviour
{
    //private VisualEffect _shieldVFX;
    //private VFXSpawnerState _ss;
    //private float _amount;
    //private float _frequency;
    private SphereCollider _sc;
    //private float _t = 4.712389f;
    //private float _time = 0;

    void Awake()
    { 
        
    }
    void Start()
    {
        //_sc = GetComponent<SphereCollider>();
        //_shieldVFX = GetComponent<VisualEffect>();
        //_ss = _shieldVFX.GetSpawnSystemInfo(Shader.PropertyToID(_shieldVFX.visualEffectAsset.name));
        //Debug.Log($"ID :{Shader.PropertyToID(_shieldVFX.visualEffectAsset.name)}, ss: {_ss}");
        //_amount = _shieldVFX.GetVector3(Shader.PropertyToID("VertexAmount")).x;
        //Debug.Log($"_amount: {_amount}");
        //_frequency = _shieldVFX.GetFloat(Shader.PropertyToID("VertexFrequency"));
        //Debug.Log($"_freqeuncy: {_frequency}");

        if (_sc == null)
        {
            Debug.LogError("Collider not found");
        }
    }

/*
    void Update()
    {
        Debug.Log($"{_ss.totalTime}");
        _time += Time.deltaTime;
        _t += _frequency * _time % 4.712389f;

        _t = Math.Clamp(_t, 4.712389f, 9.424778f);
         Debug.Log($"_t: {_t}");
        if (_t > 4.712389f && _t < 9.424778f)
        {


            _sc.radius += (float)Math.Sin(_t);
        }       
    }
*/
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("EnemyAttack"))
        {
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldHit, transform.position);
            Destroy(other.gameObject);
        }
    }
}