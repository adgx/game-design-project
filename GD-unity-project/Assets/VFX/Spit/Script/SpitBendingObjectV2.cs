using UnityEngine;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UIElements;
using UnityEngine.Splines;
using UESpline = UnityEngine.Splines.Spline;
using Unity.Mathematics;

public class SpitBendingObjectV2 : MonoBehaviour
{
    [SerializeField] private Vector3 scale;
    [SerializeField] private SpitContortAlong contortAlong;
    [SerializeField] private UESpline spline;

    [SerializeField] private float speedDelta;
    [SerializeField] private float animSpeed;
    //when hit something that is different from the player
    [SerializeField] private ParticleSystem splashParticle;
    [SerializeField] float splashActivationOffset;

    //bezier points
    private BezierKnot _start;
    private BezierKnot _middle;
    private BezierKnot _target;

    //end point 
    private Vector3 target;

    public void Awake()
    {
        spline = GetComponent<SplineContainer>().Spline;
        //init point for the spline

        
    }

    public void Start()
    {   
        
        //SpitBend(Vector3.zero);   
    }

    public void SpitBend(Vector3 target)
    {
        this.target = target;
        StartCoroutine(Coroutine_SpitBend());

    }

    //control the animation
    IEnumerator Coroutine_SpitBend()
    { 
        splashParticle.gameObject.SetActive(false);

        //config spline: set and create nodes on spline 
        // and the attach the mesh on it
        ConfigureSpline();
        contortAlong.Init();

        //animate contortion

        //todo: test the real length
        //mesh length 
        float meshLength = contortAlong.meshBender.Source.Length;
        meshLength = meshLength == 0 ? 1 : meshLength;
        Debug.Log($"Mesh lengh: {meshLength}");
        //mesh + spline
        float totalLength = meshLength + spline.GetLength();
        Debug.Log($"Total lengh: {totalLength}");
        float speedCurveLerp = 0;//acceleration factor
        //actual length 
        float length = 0;

        Vector3 startScale = Vector3.zero;
        startScale.x = 0;
        Vector3 targetScale = scale;

        while (length < totalLength)
        {
            // animate scale
            // in this way the spit appear
            if (length < meshLength)
            {
                contortAlong.ScaleMesh(Vector3.Lerp(startScale, targetScale, length / meshLength));
            }
            else //Animate movement
            {
                contortAlong.Contort((length - meshLength) / spline.GetLength());
                if (length + meshLength > totalLength + splashActivationOffset)
                {
                    if (!splashParticle.isPlaying)
                    {
                        splashParticle.gameObject.SetActive(true);
                        splashParticle.transform.position = target;
                        splashParticle.Play();
                    }
                }
            }

            length += Time.deltaTime * animSpeed * speedCurveLerp;
            speedCurveLerp += speedDelta * Time.deltaTime;

            yield return null;
        }

        Destroy(this, 0.1f);
        

         
    }

    private void ConfigureSpline()
    {
        Vector3 middlePos = new Vector3((target.x - transform.position.x) / 2, (target.y - transform.position.y) / 2, (target.z - transform.position.z) / 2);
        float deltaHight = 0.1f;
        middlePos.y += deltaHight;
        //start node
        _start.Position = Vector3.zero;
        _start.TangentOut = transform.InverseTransformDirection(middlePos - Vector3.zero).normalized;
        //middle node
        _middle.Position = middlePos;
        Vector3 midDir = transform.InverseTransformDirection(target - transform.position).normalized;
        _middle.TangentIn = -midDir;
        _middle.TangentOut = midDir;
        //end node
        Vector3 endDir = transform.InverseTransformDirection(target - middlePos).normalized;
        _target.Position = transform.InverseTransformPoint(target);
        _target.TangentIn = -endDir;
        _target.TangentOut = endDir;

        spline.Add(_start);
        spline.Add(_middle);
        spline.Add(_target);
    }

}
