using UnityEngine;
using UnityEngine.VFX;


public class AreaAttackController : MonoBehaviour
{

    private VisualEffect _areaVFX;
    public float startSize = 0.5f;
    private float _endSize;
    private bool _attack;
    //time frame = 15, to achivie the max size
    private float _velocity;
    private float _currentSize;
    private float _t;

    public void Awake()
    {
        _areaVFX = GetComponent<VisualEffect>();

    }

    public void Start()
    {
        _currentSize = startSize;
        _attack = false;
        _velocity = 0.5f; 
    }


    public void Update()
    {
        if (_attack && _currentSize < _endSize)
        {
            float ratio = (_t / 0.5f);
            _currentSize += (_endSize - startSize) * ratio;
            if (_areaVFX.HasFloat("Size"))
                _areaVFX.SetFloat("Size", _currentSize);
            if (_areaVFX.HasFloat("ratio"))
                _areaVFX.SetFloat("ratio", ratio);
            _t += Time.deltaTime;
            
        }
    }

    public void SetDestSize(float size)
    {
        _endSize = size;
        _attack = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        
    }

}