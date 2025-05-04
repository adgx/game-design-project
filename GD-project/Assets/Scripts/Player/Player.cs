using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float currentVerticalSpeed;
    [SerializeField] private float currentHorizontalSpeed;
	[SerializeField] private float maxMovementSpeed = 10f;
	[SerializeField] private float maxRotationSpeed = 15f;
    
    [SerializeField] Rigidbody player;
    
    private PlayerInput input;

    public void Awake() {
        Assert.IsNull(Instance);
        Instance = this;

        input = GetComponent<PlayerInput>();
    }

    private void Move() {
        currentVerticalSpeed = maxMovementSpeed * Math.Abs(input.Vertical) * Time.fixedDeltaTime;
        currentHorizontalSpeed = maxMovementSpeed * Math.Abs(input.Horizontal) * Time.fixedDeltaTime;
        
        player.MovePosition(player.position + currentVerticalSpeed * transform.forward);
		player.MovePosition(player.position + currentHorizontalSpeed * transform.forward);
        
        // Ho bisogno di questo constraint per evitare che il giocatore si ribalti quando tocca un muro o che si giri quando è attaccato da un nemico
        player.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void RotatePlayer() {
		currentVerticalSpeed = maxMovementSpeed * input.Vertical * Time.fixedDeltaTime;
		currentHorizontalSpeed = maxMovementSpeed * input.Horizontal * Time.fixedDeltaTime;

        if(currentVerticalSpeed != 0 || currentHorizontalSpeed != 0) {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(input.Horizontal, 0f, input.Vertical));
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.fixedDeltaTime);

            player.MoveRotation(Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * maxRotationSpeed));
        }
	}

    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        Move();
        RotatePlayer();
    }
}
