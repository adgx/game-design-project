using System.Threading.Tasks;
using UnityEngine;

public class RotateSphere : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float rotationSpeed = 100f;

    private Vector3 desiredPosition;
    private bool rotateSphere = true;

    // This function position the Sphere in the specified position related to the Player
    public void positionSphere(Vector3 position) {
        desiredPosition = position;
        rotateSphere = false;
    }

    // Update is called once per frame
    async void Update()
    {
        if(rotateSphere) {
            transform.RotateAround(player.transform.position, new Vector3(0, 1, 0), rotationSpeed * Time.deltaTime);
        }
        else {
            Vector3 currentDir = (transform.position - player.transform.position).normalized;
            Vector3 targetDir = (desiredPosition - player.transform.position).normalized;

            float angle = Vector3.Angle(currentDir, targetDir);

            transform.RotateAround(player.transform.position, new Vector3(0, 1, 0), 20 * rotationSpeed * Time.deltaTime);

            if(angle < 12f) {
				transform.position = desiredPosition;

                await Task.Delay(1000);
				rotateSphere = true;
			}
        }
    }
}
