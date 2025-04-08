using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Transform target;
    
    [SerializeField] private float cameraMovementSpeed = 10f;
    
    [SerializeField] private float xOffset = 0f;
    [SerializeField] private float yOffset = 4;
    [SerializeField] private float zOffset = -6f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = Player.Instance.transform;
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y + yOffset, target.position.z + zOffset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * cameraMovementSpeed);
    }

    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        FollowTarget();
    }
}
