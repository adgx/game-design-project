using UnityEngine;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UIElements;


public class SpitBendingObject : MonoBehaviour
{
    [SerializeField] private Vector3 scale;

    [SerializeField] private Spline spline;
    [SerializeField] private ExampleContortAlong contortAlong;

    [SerializeField] private float speedDelta;
    [SerializeField] private float animSpeed;
    //when hit something that is different from the player
    [SerializeField] private ParticleSystem splashParticle;
    [SerializeField] float splashActivationOffset;

    //end point 
    private Vector3 target;

    public void SpitBend(Vector3 target)
    {
        this.target = target;
        StartCoroutine(Coroutine_SpitBend());

    }

    //control the animation
    IEnumerator Coroutine_SpitBend()
    { 
        //disable spline, and the splash particle
        spline.gameObject.SetActive(false);
        splashParticle.gameObject.SetActive(false);

        //config spline: set and create nodes on spline 
        // and the attach the mesh on it
        ConfigureSpline();
        contortAlong.Init();

        //animate contortion
        spline.gameObject.SetActive(true);

        //todo: test the real length
        //mesh length 
        float meshLength = contortAlong.meshBender.Source.Length;
        meshLength = meshLength == 0 ? 1 : meshLength;
        //mesh + spline
        float totalLength = meshLength + spline.Length;

        float speedCurveLerp = 0;//acceleration factor
        //actual length 
        float length = 0;

        Vector3 startScale = scale;
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
                contortAlong.Contort((length - meshLength) / spline.Length);
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

        if (spline.nodes.Count > 2)
        {
            Debug.Log($"nodes: {spline.nodes.Count}");
        }


        //define the direction
        Vector3 targetDirection = target - transform.position;
        transform.forward = new Vector3(targetDirection.x, 0, targetDirection.z).normalized;
        //define the parlabolic path

        float maxHeight = 0.2f;
        //spline.AddNode(new SplineNode(Vector3.zero, Vector3.forward));
        //start node 
        //spline.AddNode(new SplineNode(Vector3.zero, Vector3.forward));
        //from world coordinates to local coordinates
        Vector3 mPos = new Vector3(transform.position.x + targetDirection.x / 2, transform.position.y + maxHeight, transform.position.z + targetDirection.z / 2);
        spline.nodes[0].Position = transform.InverseTransformPoint(transform.position);
        spline.nodes[0].Direction = transform.InverseTransformDirection(Vector3.zero);
        //middle node

        //Vector3 mPos = new Vector3(targetDirection.x / 2, maxHeight, targetDirection.z / 2);
        //spline.InsertNode(1, new SplineNode(transform.InverseTransformPoint(mPos),
        //                                    transform.InverseTransformDirection((target-mPos).normalized)));
        //spline.nodes[1].Position = transform.InverseTransformPoint(mPos);
        //spline.nodes[1].Direction = transform.InverseTransformDirection((target - mPos).normalized)*0.1f;
        //end node
        //spline.AddNode(new SplineNode(transform.InverseTransformPoint(target), transform.InverseTransformDirection(target - mPos).normalized*0.1f));
        spline.nodes[1].Position = transform.InverseTransformPoint(target);
        spline.nodes[1].Direction = transform.InverseTransformDirection(Vector3.zero);
        spline.RefreshCurves();
    }

}
