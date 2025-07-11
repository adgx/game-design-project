using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace SplineMesh {
    using UESpline = UnityEngine.Splines.Spline;
    [RequireComponent(typeof(SplineContainer))]
    public class SpitContortAlong : MonoBehaviour {
        private UESpline spline;
        //deform the mesh for a specific spline's range
        public SpitMeshBender meshBender;

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


        public void Awake()
        {
            spline = GetComponent<SplineContainer>().Spline;
        }

        public void Start()
        {

        }
        public void ScaleMesh(Vector3 scale)
        {
            meshBender.Source = meshBender.Source.Scale(scale.x, scale.y, scale.z);
        }
        //used to translate the mesh along the spline curve
        public void Contort(float lerp)
        {
                meshBender.SetInterval(spline, spline.GetLength() * lerp);
                meshBender.ComputeIfNeeded();
        }

        public void Init() {
            if (meshBender == null)
            {
                string generatedName = GetType().Name;
                var generatedTranform = transform.Find(generatedName);
                generated = generatedTranform != null ? generatedTranform.gameObject : UOUtility.Create(generatedName, gameObject,
                    typeof(MeshFilter),
                    typeof(MeshRenderer),
                    typeof(SpitMeshBender));

                generated.GetComponent<MeshRenderer>().material = material;

                meshBender = generated.GetComponent<SpitMeshBender>();
            }

            meshBender.Source = SourceMesh.Build(mesh)
                .Rotate(Quaternion.Euler(rotation))
                .Scale(scale);
            meshBender.Mode = SpitMeshBender.FillingMode.Once;
            meshBender.SetInterval(spline, 0);
        }
    }
}
