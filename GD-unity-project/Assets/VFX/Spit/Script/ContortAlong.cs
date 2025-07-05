using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SplineMesh {
   
    [ExecuteAlways]
    [RequireComponent(typeof(Spline))]
    public class ContortAlong : MonoBehaviour {
        private Spline spline;
        private float rate = 0;
        //deform the mesh for a specific spline's range
        public MeshBender meshBender;

        [HideInInspector]
        public GameObject generated;

        //mesh property
        public Mesh mesh;
        public Material material;
        public Vector3 rotation;
        public Vector3 scale;

        public float DurationInSecond;

        public float targetScale;
        [Range(0, 1)]
        [SerializeField] float lerp;

        public void ScaleMesh(Vector3 scale)
        {
            meshBender.Source = meshBender.Source.Scale(scale.x, scale.y, scale.z);
        }
        //used to translate the mesh along the spline curve
        public void Contort(float lerp)
        {
            if (generated != null)
            {
                meshBender.SetInterval(spline, spline.Length * lerp);
                meshBender.ComputeIfNeeded();
            }
        }

        public void Init() {
            string generatedName = GetType().Name;
            var generatedTranform = transform.Find(generatedName);
            generated = generatedTranform != null ? generatedTranform.gameObject : UOUtility.Create(generatedName, gameObject,
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(MeshBender));

            generated.GetComponent<MeshRenderer>().material = material;

            meshBender = generated.GetComponent<MeshBender>();
            spline = GetComponent<Spline>();

            meshBender.Source = SourceMesh.Build(mesh)
                .Rotate(Quaternion.Euler(rotation))
                .Scale(scale);
            meshBender.Mode = MeshBender.FillingMode.Once;
            meshBender.SetInterval(spline, 0);
        }
    }
}
