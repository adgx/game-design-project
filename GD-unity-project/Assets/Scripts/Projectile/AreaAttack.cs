using UnityEngine;
using UnityEngine.VFX;

public class AreaAttackController : MonoBehaviour
{

    private VisualEffect _areaVFX;
    public float startSize = 0.5f;
    private float _endSize;
    private bool _attack;
    // Time frame = 15, to achieve the max size
    private float _currentSize;
    private float _t;
    private float _closeAttackDamage;
    private SphereCollider _sphereCol;

    public void Awake()
    {
        _areaVFX = GetComponent<VisualEffect>();
        _sphereCol = GetComponent<SphereCollider>();
    }

    public void Start()
    {
        _currentSize = startSize;
        _attack = false;
        _sphereCol.radius = startSize;
        if (_areaVFX.HasFloat("Size"))
                _areaVFX.SetFloat("Size", _currentSize);
        if (_areaVFX.HasFloat("Rate"))
            _areaVFX.SetFloat("Rate", 0f);
    }
    
    public void Update()
    {
        if (_attack && _currentSize < _endSize)
        {
            float ratio = _t / 0.5f;
            float deltaSize = (_endSize - startSize) * ratio;
            _currentSize = deltaSize + startSize;
            _sphereCol.radius = _currentSize;
            if (_areaVFX.HasFloat("Size"))
                _areaVFX.SetFloat("Size", _currentSize);
            if (_areaVFX.HasFloat("Rate"))
            {
                _areaVFX.SetFloat("Rate", ratio);
                Debug.Log($"Rate: {_areaVFX.GetFloat("Rate")}");
            }
            _t += Time.deltaTime;
        }
    }
    
    public void Initialize(float damage)
    {
        _closeAttackDamage = damage;
    }

    public void SetDestSize(float size)
    {
        _endSize = size;
        _attack = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.tag.Contains("Enemy") && !other.tag.Contains("EnemyAttack")) {
				other.GetComponent<Enemy.EnemyManager.IEnemy>().TakeDamage(_closeAttackDamage, "c");
        }   
    }

}