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
        }
    }
}
