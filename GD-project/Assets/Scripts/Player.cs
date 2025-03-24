using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float currentMovementSpeed;
    [SerializeField] private float maxMovementSpeed = 10f;
    [SerializeField] private float currentRotationSpeed;
    [SerializeField] private float maxRotationSpeed = 45f;
    
    private PlayerInput input;

    private float moveAmount;

    public void Awake() {
        Assert.IsNull(Instance);
        Instance = this;

        input = GetComponent<PlayerInput>();
    }

    private void Move() {
        currentMovementSpeed = maxMovementSpeed * input.Horizontal * Time.deltaTime;
        currentRotationSpeed = maxRotationSpeed * input.Rotation * Time.deltaTime;
        moveAmount += currentMovementSpeed;

        this.transform.position += moveAmount * this.transform.forward;
        this.transform.Rotate(this.transform.up, currentRotationSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
}
