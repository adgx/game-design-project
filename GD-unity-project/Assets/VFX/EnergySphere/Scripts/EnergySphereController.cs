using System;
using UnityEngine;

public class EnergySphereController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _energyPS;
    //acceleration
    private float _a = 10f;
    //velocity max
    private float _vMax = 15f;

    [SerializeField] private Transform _targetPos;
    private Vector3 _destPos;
    private float _currentV;
    private float _t = 0;
    public float playerBulletDamage = 50f;

    void Start()
    {
        _currentV = 0f;
        gameObject.SetActive(false);
        _destPos = _targetPos.position;
        transform.LookAt((_destPos- transform.position).normalized);
        gameObject.SetActive(true);
        _energyPS.Play();

    }
    void Update()
    {
        if (_currentV < _vMax)
        {
            _t += Time.deltaTime * _a/_vMax;
            _currentV = Mathf.Lerp(0f, _vMax, _t);
        }

        Vector3 vDir = transform.forward * _currentV * Time.deltaTime;
        transform.position += vDir;  
            
    }


    void OnParticleCollision(GameObject other)
    {

        Debug.Log("Collision Detected");
        if (other.CompareTag("Enemy"))
        {
            //other.GetComponent<Enemy.EnemyManager.IEnemy>().TakeDamage(playerBulletDamage, "Energy");
            Destroy(other);
        }
        Destroy(gameObject);
    }
}
