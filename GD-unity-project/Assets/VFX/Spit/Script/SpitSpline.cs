using UnityEngine.Splines;
using UnityEngine;
using Unity.Mathematics;

public class SpitSpline : MonoBehaviour
{
    [SerializeField] private Transform  _targetPos;

    private Spline _spline;
    private BezierKnot _start;
    private BezierKnot _target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _start.Position = new float3(transform.position.x, transform.position.y, transform.position.z);
        //tangent 
        Vector3 startPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 targetPos = new Vector3(_targetPos.position.x, _targetPos.position.y, _targetPos.position.z);
        Vector3 dir = targetPos - startPos;
        _start.TangentIn = -dir.normalized;
        _start.TangentOut = dir.normalized;
        _target.Position = new float3(_targetPos.position.x, _targetPos.position.y, _targetPos.position.z);
        _target.TangentIn = -dir.normalized;
        _target.TangentOut = dir.normalized;
        _spline = GetComponent<SplineContainer>().Spline;
        _spline.Add(_start);
        _spline.Insert(0, _target);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
