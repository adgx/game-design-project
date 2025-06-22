using UnityEngine;
using ORF;

public class CameraMovement : MonoBehaviour
{
    private Transform target;
    
    [SerializeField] private float cameraMovementSpeed = 10f;
    
    [SerializeField] private float xOffset = 0f;
    [SerializeField] private float yOffset = 10;
    [SerializeField] private float zOffset = -11f;

    // Needed to rotate the camera around the player
    [SerializeField] private float mouseSensitivity = 3.0f;
	[SerializeField] private float rotationX = 5f;
	[SerializeField] private float rotationY = 20f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        target = Player.Instance.transform;
    }

    private void FollowTarget() {
        if(target != null) {
            Vector3 targetPosition = new Vector3(target.position.x + xOffset, target.position.y + yOffset, target.position.z + zOffset);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * cameraMovementSpeed);
        }
    }

    private void RotateCamera() {
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		// Ocio: be aware that the rotation and the mouse axis are inverted
		rotationY += mouseX;
		rotationX += mouseY;

		rotationX = Mathf.Clamp(rotationX, -20, 7);

		transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);

        transform.position = target.position - transform.forward * 7.0f + transform.up * 2.0f;
	}

    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        //FollowTarget();

        RotateCamera();
    }
}
