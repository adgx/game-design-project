using UnityEngine;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UIElements;

public class SpitBendingObject : MonoBehaviour
{
    [SerializeField] private float pointCount;
    [SerializeField] private float radius;
    [SerializeField] private float heightDelta;
    [SerializeField] private Vector3 scale;

    [SerializeField] private Spline spline;
    [SerializeField] private ContortAlong contortAlong;

    [SerializeField] private float speedDelta;
    [SerializeField] private float animSpeed;
    [SerializeField] private ParticleSystem splashParticle;
    [SerializeField] float splashActivationOffset;

    private Vector3 target;

    public void SpitBend(Vector3 target)
    {
        this.target = target;
        StartCoroutine(Coroutine_SpitBend());

    }

    //control the animation
    IEnumerator Coroutine_SpitBend()
    { 
        spline.gameObject.SetActive(false);
        splashParticle.gameObject.SetActive(false);

        //config spline
        ConfigureSpline();
        contortAlong.Init();

        //animate contortion
        spline.gameObject.SetActive(true);

        //todo: test the real length
        float meshLength = contortAlong.meshBender.Source.Length;
        meshLength = meshLength == 0 ? 1 : meshLength;
        float totalLength = meshLength + spline.Length;

        float speedCurveLerp = 0;
        float length = 0;

        Vector3 startScale = scale;
        startScale.x = 0;
        Vector3 targetScale = scale;

        while (length < totalLength)
        {
            // animate scale
            if (length < meshLength)
            {
                contortAlong.ScaleMesh(Vector3.Lerp(startScale, targetScale, length / meshLength));
            }
            else //Animate movement
            {
                contortAlong.Contort((length - meshLength)/ spline.Length);
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


        

         
    }

    private void ConfigureSpline()
    {
        //todo: rewrite to avoid the spirals
        //usee the first two nodes
        List<SplineNode> nodes = new List<SplineNode>(spline.nodes);
        
        for (int i = 2; i < nodes.Count; i++) 
        {
            spline.RemoveNode(nodes[i]);
        }
        //define the direction
        Vector3 targetDirection = (target - transform.position);
        transform.forward = new Vector3(targetDirection.x, 0, targetDirection.z).normalized;

        //define the spline nodes
        int sign = Random.Range(0, 2) == 0 ? 1 : -1;
        float angle = 90 *sign;
        float height = 0;

        for (int i = 0; i < pointCount; i++)
        {
            if (spline.nodes.Count <= i)
            {
                spline.AddNode(new SplineNode(Vector3.zero, Vector3.forward));
            }

            Vector3 normal = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 pos = transform.position + normal * radius;
            pos.y  = height;
            Vector3 direction = pos + Quaternion.Euler(Random.Range(-30, 30), Random.Range(60, 120) * sign, Random.Range(-30, 30)) * normal * radius / 2f;

            if (i == 0)
            {
                direction = pos + Vector3.up * radius;
            }

            //from world coordinates to local coordinates
            spline.nodes[i].Position = transform.InverseTransformPoint(pos);
            spline.nodes[i].Direction = transform.InverseTransformPoint(direction);

            height += heightDelta;
            angle += 90 * sign;
        }

        Vector3 targetNodePosition = transform.InverseTransformPoint(target);

        Quaternion randomRotation = Quaternion.Euler(Random.Range(0, 90), Random.Range(-40, 40), 0);
        Vector3 targetNodeDirection = target + randomRotation * (transform.forward * (target-transform.position).magnitude * Random.Range(0.2f, 1f));

        targetNodeDirection = transform.InverseTransformPoint(targetNodeDirection);
        SplineNode node = new SplineNode(targetNodePosition, targetNodeDirection);  
        spline.AddNode(node);


    }

}
