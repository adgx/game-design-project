using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using ORF;

public class RotateSphere : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float transitionSpeed = 10f;
    
    // Parameters for the "wave" movement of the Sphere
    private float waveAmplitude = 0.5f;
    private float waveFrequency = 2.5f;

    private Vector3 desiredPosition;
    public bool isRotating = true;

    public enum Animation {
        RotateAround,
        Linear
    }

    private new Animation animation;

    void Start() {
        transform.localPosition = player.transform.forward * 1f;
    }

    // This function positions the Sphere in the specified position around the Player, moving it with the specified animation
    public void positionSphere(Vector3 position, Animation animationValue) {
        desiredPosition = position;
        isRotating = false;
        animation = animationValue;
    }

    // Update is called once per frame
    void Update()
    {
        // Needed to avoid the sphere deforming when player collides with enemies
        if (transform.rotation[0] != 0 || transform.rotation[2] != 0) {
            transform.rotation = new Quaternion(0, transform.rotation[1], 0, transform.rotation[3]);
        }
        
        if(isRotating) {
            transform.RotateAround(player.transform.position, new Vector3(0, 1, 0), rotationSpeed * Time.deltaTime);
            
            float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
            
            Vector3 currentPosition = transform.position;
            transform.position = new Vector3(currentPosition.x, 2.5f + waveOffset, currentPosition.z);
        }
        else {
            switch (animation) {
                case Animation.RotateAround:
                    if(transform.localPosition != desiredPosition) {
                        Player playerClass = player.GetComponent<Player>();
                        transform.localPosition = Vector3.MoveTowards(transform.localPosition, new Vector3(transform.localPosition.x, 0, transform.localPosition.z), transitionSpeed * Time.deltaTime);

                        Vector3 currentDir = transform.localPosition.normalized;
                        Vector3 targetDir = desiredPosition.normalized;

                        float angle = Vector3.Angle(currentDir, targetDir);

                        playerClass.isFrozen = true;

                        transform.RotateAround(player.transform.position, new Vector3(0, 1, 0), 15 * rotationSpeed * Time.deltaTime);

                        if(angle <= 40f) {
                            transform.localPosition = desiredPosition;
                        }
                    }
                    break;
                case Animation.Linear:
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, desiredPosition, transitionSpeed * Time.deltaTime);
                    break;
            }
        }
    }
}