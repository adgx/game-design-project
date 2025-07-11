using System;
using UnityEngine;
using System.Collections.Generic;


public class ParticleAttackController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _attackPS;
    //acceleration
    private float _a = 13f;
    private float _g = -9.81f;
    private float _vyInit = 0.2f;
    //velocity max
    private float _vMax = 15f;

    public Transform targetPos;
    private Vector3 _destPos;
    private float _currentVF;
    private float _currentVUp;
    private float _t = 0;
    public float initialPlayerBulletDamage = 40, enemyBulletDamage = 20;
    public float playerBulletDamage;

    void Start()
    {
        _currentVF = 0f;
        _currentVUp = _vyInit;
        gameObject.SetActive(false);
        _destPos = targetPos.position;
        //offset
        _destPos.y += 1f;

        transform.LookAt(_destPos);
        gameObject.SetActive(true);
        _attackPS.Play();

    }
    void Update()
    {
        if (_currentVF < _vMax)
        {
            _t += Time.deltaTime * _a / _vMax;
            _currentVF = Mathf.Lerp(0f, _vMax, _t);
        }

        _currentVUp = _g * Time.deltaTime;


        Vector3 sDirF = transform.forward * _currentVF * Time.deltaTime;
        Vector3 sDirUp = transform.up * _currentVUp * Time.deltaTime;
        transform.position += sDirF + sDirUp;

    }


    void OnParticleCollision(GameObject other)
    {
        List<ParticleCollisionEvent> ce = new();
        _attackPS.GetCollisionEvents(other, ce);
        Debug.Log($"Collision Detected :{other.gameObject.tag}");
        if (other.CompareTag("Player"))
        {
            PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();
            playerShoot.TakeDamage(enemyBulletDamage, PlayerShoot.DamageTypes.Spit, Math.Sign(ce[0].normal.x), Math.Sign(ce[0].normal.z));
            Destroy(gameObject);
        }
        else if (other.CompareTag("Shield"))
        {
            Debug.Log("Shield");
            GamePlayAudioManager.instance.PlayOneShot(Audio.FMODEvents.Instance.PlayerShieldHit, transform.position);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("EnemyIncognito"))
            Destroy(gameObject);
    }


}
