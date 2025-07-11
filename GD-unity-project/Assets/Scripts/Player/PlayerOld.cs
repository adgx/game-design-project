/*
using FMOD.Studio;
using System;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerOld : MonoBehaviour
{
    public static PlayerOld Instance { get; private set; }

    [SerializeField] private float currentVerticalSpeed;
    [SerializeField] private float currentHorizontalSpeed;
	[SerializeField] private float maxMovementSpeed = 5f;
	[SerializeField] private float maxRotationSpeed = 15f;
    
    [SerializeField] private Rigidbody player;
	[SerializeField] private Camera mainCamera;
    
    private PlayerInput input;
    public bool isFrozen;

	// Audio management
	private PlayerShoot playerShoot;
	private EventInstance playerFootsteps;
	private EventInstance sphere;
	private EventInstance sphereRotation;
	[SerializeField] private GameObject rotatingSphere;

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
		RaycastHit hit, hitBack;

		//this could be integrated in input class
		Vector2 move = new Vector2(input.Horizontal, input.Vertical);
		Vector3 currentHorizontalSpeedV3 = new Vector3(player.linearVelocity.x, 0f, player.linearVelocity.z);
		//this is ds not speed
        currentVerticalSpeed = maxMovementSpeed * input.Vertical * Time.fixedDeltaTime;
        currentHorizontalSpeed = maxMovementSpeed * input.Horizontal * Time.fixedDeltaTime;

        Vector3 direction = input.Vertical * (new Vector3(mainCamera.transform.forward.x, 0f, mainCamera.transform.forward.z)) + input.Horizontal * (new Vector3(mainCamera.transform.right.x, 0f, mainCamera.transform.right.z));
		direction.Normalize();

		player.MovePosition(player.position + direction * maxMovementSpeed * Time.fixedDeltaTime);
        
		//animation stuff
		//now there are no acceleration property 
		//costant velocity 
		//this line is usefull if the acceleration is handled
		//float runBlendVal = ORF.Utils.Math.NormalizeValueByRage(0f, maxMovementSpeed, Math.Max(Math.Abs(currentVerticalSpeed), Math.Abs(currentHorizontalSpeed)));
		float runBlendVal = Math.Max(Math.Abs(input.Vertical), Math.Abs(input.Horizontal));
		AnimationManager.Instance.SetRunBledingAnim(runBlendVal);
		//rickAnim.SetRunBledingAnim(runBlendVal);
		if(runBlendVal == 0f && !AnimationManager.Instance.rickState.Equals(RickStates.Idle))
		{
			AnimationManager.Instance.Idle();
		}
		else if (runBlendVal != 0f && AnimationManager.Instance.rickState.Equals(RickStates.Idle)) 
		{

			AnimationManager.Instance.Run();
		}

		player.MovePosition(player.position + direction * maxMovementSpeed * Time.fixedDeltaTime);
		
		// I need this constraint to avoid that the player turns upside down when it touches another collider
        player.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void RotatePlayer() {
		currentVerticalSpeed = maxMovementSpeed * input.Vertical * Time.fixedDeltaTime;
		currentHorizontalSpeed = maxMovementSpeed * input.Horizontal * Time.fixedDeltaTime;

        if(currentVerticalSpeed != 0 || currentHorizontalSpeed != 0) {
			Quaternion targetRotation;
			if(currentVerticalSpeed != 0 && currentHorizontalSpeed != 0) {
				// Managing the player's diagonal movement
				Vector3 forward = new Vector3(mainCamera.transform.forward.x, 0f, mainCamera.transform.forward.z).normalized;
				Vector3 right = new Vector3(mainCamera.transform.right.x, 0f, mainCamera.transform.right.z).normalized;

				targetRotation = Quaternion.LookRotation(forward * currentVerticalSpeed + right * currentHorizontalSpeed);
			}
			else {
				if(currentVerticalSpeed != 0) {
					targetRotation = Quaternion.LookRotation(new Vector3(mainCamera.transform.forward.x * input.Vertical, 0f, mainCamera.transform.forward.z * input.Vertical));
				}
				else {
					targetRotation = Quaternion.LookRotation(new Vector3(mainCamera.transform.right.x * input.Horizontal, 0f, mainCamera.transform.right.z * input.Horizontal));
				}
			}
			Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.fixedDeltaTime);

            player.MoveRotation(Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * maxRotationSpeed));
        }
	}

	// Audio management
	private void Start() 
	{
		playerShoot = GetComponent<PlayerShoot>();
		playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
		playerFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
		sphereRotation = AudioManager.instance.CreateInstance(FMODEvents.instance.sphereRotation);
		sphereRotation.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
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
		sphereRotation.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));

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

		if (playerShoot != null && playerShoot.IsSphereRotating)
		{
			// If the sphere is rotating, then start the sound
			if (rotationPlaybackState == PLAYBACK_STATE.STOPPED)
				sphereRotation.start();
		}
		else
		{
			// If the sphere is not rotating, then stop the sound
			if (rotationPlaybackState != PLAYBACK_STATE.STOPPED)
				sphereRotation.stop(STOP_MODE.ALLOWFADEOUT);
		}

	}
}
*/