using FMOD.Studio;
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
    
    [SerializeField] private Rigidbody player;
    
    private PlayerInput input;
    public bool isFrozen;

	// Audio management
	private EventInstance playerFootsteps;
	private EventInstance sphere;
	private EventInstance sphereRotation;

	public void FreezeMovement(bool freeze)
    {
        isFrozen = freeze;
    }

    public void Awake() {
        Assert.IsNull(Instance);
        Instance = this;

        input = GetComponent<PlayerInput>();
        player.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

	private void Move() {
        currentVerticalSpeed = maxMovementSpeed * Math.Abs(input.Vertical) * Time.fixedDeltaTime;
        currentHorizontalSpeed = maxMovementSpeed * Math.Abs(input.Horizontal) * Time.fixedDeltaTime;

        if (currentVerticalSpeed != 0 && currentHorizontalSpeed != 0) {
            // We need to divide the speed by 2 when the player is moving diagonally to avoid faster movement
            currentVerticalSpeed = currentVerticalSpeed/2;
            currentHorizontalSpeed = currentHorizontalSpeed/2;
        }
        
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

	// Audio management
	private void Start() 
	{
		playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
		playerFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
		sphereRotation = AudioManager.instance.CreateInstance(FMODEvents.instance.sphereRotation);
		sphereRotation.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
	}

	// FixedUpdate is called once per frame
	void FixedUpdate()
    {
        if(!isFrozen) {
            Move();
            RotatePlayer();
        }

		// Audio management
		UpdateSound();
	}

	// Audio management
	private void UpdateSound() 
	{
		playerFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
		sphereRotation.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(this.transform));

		// Start footsteps event if the player is moving
		if((Mathf.Abs(input.Horizontal) > 0.01f || Mathf.Abs(input.Vertical) > 0.01f) && !isFrozen)
		{
			// Get the playback state for the footsteps event
			PLAYBACK_STATE footstepsPlaybackState;
			playerFootsteps.getPlaybackState(out footstepsPlaybackState);
			if(footstepsPlaybackState.Equals(PLAYBACK_STATE.STOPPED)) 
			{
				playerFootsteps.start();
			}
		}
		// Otherwise, stop the footsteps event
		else
		{
			playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
		}

		// Get the playback state for the rotation event
		PLAYBACK_STATE rotationPlaybackState;
		sphereRotation.getPlaybackState(out rotationPlaybackState);
		if(rotationPlaybackState.Equals(PLAYBACK_STATE.STOPPED)) 
		{
			sphereRotation.start();
		}
	}

}
