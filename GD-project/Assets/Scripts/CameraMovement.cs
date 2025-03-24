using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Transform target;
    
    [SerializeField] private float cameraMovementSpeed = 10f;
    
    [SerializeField] private float xOffset = 0f;
    [SerializeField] private float yOffset = 10f;
    [SerializeField] private float zOffset = -10f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //target = PlayerMov
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
