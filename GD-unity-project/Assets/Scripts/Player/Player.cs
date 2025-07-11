using FMOD.Studio;
using Audio;
using UnityEngine;

	public class Player : MonoBehaviour
	{
        public static Player Instance { get; private set; }
        //player attributes
        [Header("Player Attributes")]
        [Tooltip("Max speed")]
        [SerializeField] private float maxMovementSpeed = 7f;
        
		[Tooltip("Acceleration and deceleration")]
		[SerializeField] private float speedChangeRate = 10.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        private float rotationSmoothTime = 0.12f;

        private float targetRotation = 0.0f;
		private float currentVerticalSpeed;
        private float currentHorizontalSpeed = 0;
		private float rotationVelocity;
        private float speed = 0;

		//Rigidbody
		[SerializeField] private Rigidbody player;
		//cinemachine
		[SerializeField] private Camera mainCamera;

		private PlayerInput input;
		public bool isFrozen;
		
		// Audio management
		private PlayerShoot playerShoot;
		private EventInstance sphere;
		private EventInstance sphereRotation;
		[SerializeField] private GameObject rotatingSphere;

		public void FreezeMovement(bool freeze)
		{
			isFrozen = freeze;
		}

		public void Awake()
		{
			if (Instance != null)
			{
				Debug.LogError("Found more than one Player in the scene.");
			}
			Instance = this;
			
			input = GetComponent<PlayerInput>();
			player.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Move()
		{
            //this could be integrated in input class
            Vector2 move = new Vector2(input.Horizontal, input.Vertical);

			//set target speed to the maxMovementSpeed
			float targetSpeed = move == Vector2.zero ? 0f : maxMovementSpeed;


            //currentHorizontalSpeed = new Vector3(player.linearVelocity.x, 0f, player.linearVelocity.z).magnitude;
            currentHorizontalSpeed = speed;
            
			//acceleration stuff
            float speedOffset = 0.1f;
			float inputMagnitude = move.magnitude;

			//accelerate or decelerate
			if (currentHorizontalSpeed < targetSpeed - speedOffset ||
				currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.fixedDeltaTime * speedChangeRate);
			}
            else
            {
				speed = targetSpeed;
            }


			Vector3 direction = input.Vertical * (new Vector3(mainCamera.transform.forward.x, 0f, mainCamera.transform.forward.z)) + input.Horizontal * (new Vector3(mainCamera.transform.right.x, 0f, mainCamera.transform.right.z));
			direction.Normalize();
			
			//animation stuff
			float runBlendVal = ORF.Utils.Math.NormalizeValueByRage(0f, maxMovementSpeed, speed);
			AnimationManager.Instance.SetRunBledingAnim(runBlendVal);

			if (runBlendVal == 0f && AnimationManager.Instance.rickState.Equals(RickStates.Run))
			{
				AnimationManager.Instance.Idle();
			}
			else if (runBlendVal != 0f && AnimationManager.Instance.rickState.Equals(RickStates.Idle))
			{
				AnimationManager.Instance.Run();
			}

			Vector3 inputDirection = new Vector3(move.x, 0f, move.y).normalized;

			if (move != Vector2.zero)
			{
				targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
				transform.rotation = Quaternion.Euler(0f, rotation, 0f);
			}

			Vector3 targerDirection = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
			player.MovePosition(player.position + targerDirection.normalized * (speed * Time.fixedDeltaTime));

			// I need this constraint to avoid that the player turns upside down when it touches another collider
			player.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
		}
		
		// Audio management
		private void Start()
		{
			playerShoot = GetComponent<PlayerShoot>();
			sphereRotation = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerSphereRotation);
			sphereRotation.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		}

		// FixedUpdate is called once per frame
		void FixedUpdate()
		{
			if (!isFrozen)
			{
				Move();
			}
			
			// Audio management
			UpdateSound();
		}
		
		// Audio management
		private void UpdateSound()
		{
			sphereRotation.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
			
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