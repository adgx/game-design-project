using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float currentMovementSpeed;
    [SerializeField] private float maxMovementSpeed = 10f;
    [SerializeField] private float currentRotationSpeed;
    [SerializeField] private float maxRotationSpeed = 45f;
    
    [SerializeField] Rigidbody player;
    
    private PlayerInput input;

    public void Awake() {
        Assert.IsNull(Instance);
        Instance = this;

        input = GetComponent<PlayerInput>();
    }

    private void Move() {
        currentMovementSpeed = maxMovementSpeed * input.Horizontal * Time.fixedDeltaTime;
        currentRotationSpeed = maxRotationSpeed * input.Rotation * Time.fixedDeltaTime;
        
        player.MovePosition(player.position + currentMovementSpeed * transform.forward);
        player.transform.Rotate(Vector3.up * currentRotationSpeed);
        
        // Ho bisogno di questo constraint per evitare che il giocatore si ribalti quando tocca un muro
        player.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        Move();
    }
}
