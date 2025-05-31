using System.Threading.Tasks;
using UnityEngine;

public class RotateSphere : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float transitionSpeed = 10f;
    
    // Parameters for the "wave" movement of the Sphere
    private float waveAmplitude = 0.5f;
    private float waveFrequency = 2.5f;

    private Vector3 desiredPosition;
    public bool rotateSphere = true;

    public enum Animation {
        RotateAround,
        Linear
    }

    private Animation animation;

    void Start() {
        transform.localPosition = player.transform.forward * 1f;
    }

    // This function positions the Sphere in the specified position around the Player, moving it with the specified animation
    public void positionSphere(Vector3 position, Animation animationValue) {
        desiredPosition = position;
        rotateSphere = false;
        animation = animationValue;
    }

    // Update is called once per frame
    async void Update()
    {
        if(rotateSphere) {
            transform.RotateAround(player.transform.position, new Vector3(0, 1, 0), rotationSpeed * Time.deltaTime);
            
            float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
            
            Vector3 currentPosition = transform.position;
            transform.position = new Vector3(currentPosition.x, 2.5f + waveOffset, currentPosition.z);
        }
        else {
            switch (animation) {
                case Animation.RotateAround:
                    Player playerClass = player.GetComponent<Player>();

                    Vector3 currentDir = (transform.localPosition).normalized;
                    Vector3 targetDir = (desiredPosition).normalized;

                    float angle = Vector3.Angle(currentDir, targetDir);

                    playerClass.isFrozen = true;

                    transform.RotateAround(player.transform.position, new Vector3(0, 1, 0), 20 * rotationSpeed * Time.deltaTime);

                    if(angle < 12f) {
                        transform.localPosition = desiredPosition;

                        await Task.Delay(500);
                        rotateSphere = true;
                        playerClass.isFrozen = false;
                    }
                    break;
                case Animation.Linear:
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, desiredPosition, transitionSpeed * Time.deltaTime);
                    break;
                default:
                    break;
            }
        }
    }
}
