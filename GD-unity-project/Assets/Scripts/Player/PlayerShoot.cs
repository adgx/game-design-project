using System;
using System.Collections;
using System.Threading.Tasks;
using Animations;
using Audio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Helper;
using PlayerInteraction;
using Utils;

public class PlayerShoot : MonoBehaviour
{
	public static PlayerShoot Instance { get; private set; }
	public float LastStaminaUseTime { get; set; }
	[HideInInspector] public bool isInCombat = false;
	private float recoveryDelay = 3f;

	// Audio management 
	public bool IsSphereRotating => rotateSphere.isRotating;
	private bool isShieldCoroutineRunning;
	[SerializeField] private RickEvents rickEvents;
	private const float LOW_HEALTH_PERCENTAGE = 0.30f; // 30%

	// Attack1
	[SerializeField] private float bulletSpeed;
	[SerializeField] private Transform bulletSpawnTransform; // The point in which the bullet spawns
	[SerializeField] private GameObject bulletPrefab;
	public float baseDistanceAttackDamage = 50f; // Distance attack damage with 0 power-ups (1 stamina)
	public float distanceAttackPowerUp1Damage = 75f; // Distance attack damage with 1 power-up (3 stamina)
	public float distanceAttackPowerUp2Damage = 100f; // Distance attack damage with 2 power-ups (5 stamina)

	// Attack2
	[SerializeField] private GameObject attackAreaPrefab;
	[HideInInspector]
	public GameObject attackAreaInstance;
	public GameObject attackAreaVFXPrefab;
	public int defaultCloseAttackDamage = 40;
	private readonly float defaultDamageRadius = 2.5f;
	[HideInInspector]
	public float damageRadius = 2f;
	public float finalCloseAttackDamage;
	public float baseCloseAttackDamage = 40f; // Close attack damage with 0 power-ups (1 stamina)
	public float closeAttackPowerUp1Damage = 60f; // Close attack damage with 1 power-up (3 stamina)
	public float closeAttackPowerUp2Damage = 80f; // Close attack damage with 2 power-up1 (5 stamina)

	public bool cannotAttack = false;
	private bool isDying = false;

	// The player has 2 attacks he can choose. He can change them by using the mouse scroll wheel or the back buttons on the controller
	private int attackNumber = 1;
	[SerializeField] private KeyCode increaseAttackController, decreaseAttackController;
	[SerializeField] Image distanceAttackImage;
	[SerializeField] Image closeAttackImage;
	[SerializeField] Image distanceAttackLoadingBar;
	[SerializeField] Image closeAttackLoadingBar;
	private bool loadingAttack = false;
	// This flag is true if an attack is being executed. While executing it, I can not start another attack
	private bool attacking = false;
	private int attackStamina = 0;
	private bool isStaminaRecoveryInterruptible = true;
	private bool isStaminaRecoveryInterrupted = false;
	private Coroutine staminaRecoveryCoroutine = null;
	public bool shieldIsActive = false;
	public bool isInteracting = false;

	// Health
	public float maxHealth = 120;
	public float health;
	public float damageReduction = 1f;
	[SerializeField] private HealthBar healthBar;

	// Stamina for the attacks
	public int maxSphereStamina = 5;
	public bool increasingStamina = false;
	public int sphereStamina = 5;
	public bool sphereIsDischarged = false;

	// PowerUps
	public PowerUp powerUp;

	private Player player;
	[SerializeField] private RotateSphere rotateSphere;
	[SerializeField] private GameObject rotatingSphere;
	[SerializeField] private Material sphereMaterial;

	[SerializeField] private string respawnSceneName = "RespawnScene";
	[SerializeField] private GameTimer gameTimer;
	private bool _debug = false;

	public enum DamageTypes
	{
		Spit,
		MaynardDistanceAttack,
		CloseAttack,
		DrakeBiteAttack
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		if (!_debug)
			healthBar.SetMaxHealth(health);
		player = GetComponent<Player>();
		ChangeSphereColor(maxSphereStamina);
		finalCloseAttackDamage = defaultCloseAttackDamage;
		damageRadius = defaultDamageRadius;
		
		// We initialize the timer to the current time to prevent charging from starting immediately
		// at the beginning of the game if the stamina is not full for some reason
		LastStaminaUseTime = Time.time;
	}
	
	private void Update()
	{
		HandleStaminaRecovery();
		ProcessPlayerInput();
	}
	
	/// <summary>
	/// It contains all the logic to decide whether to start stamina recovery
	/// </summary>
	private void HandleStaminaRecovery()
	{
		// Conditions for doing nothing: charging already in progress or full stamina
		if (increasingStamina || sphereStamina >= maxSphereStamina || isInteracting)
		{
			return;
		}

		// At this point, the stamina is not full and is not recharging 
		if (Time.time - LastStaminaUseTime >= recoveryDelay)
		{
			// Logic in combat: starts only after the delay if the sphere is completely discharged
			if (isInCombat)
			{
				if (sphereIsDischarged)
				{
					StartStaminaRecovery();
				}
			}
			// Logic out of combat: starts only after the delay
			else
			{
				StartStaminaRecovery();
			}
		}
	}

	void ChangeSphereColor(int stamina)
	{
		float intensityRate = 3.0f;
		float intensityHDR = Mathf.Pow(2,intensityRate);

		switch (stamina)
		{
			case 5:
				sphereMaterial.SetColor("_EmissionColor", new Color(0, 1, 1) * intensityHDR);
				break;
			case 4:
				sphereMaterial.SetColor("_EmissionColor", Color.green * intensityHDR);
				break;
			case 3:
				sphereMaterial.SetColor("_EmissionColor", Color.yellow * intensityHDR);
				break;
			case 2:
				sphereMaterial.SetColor("_EmissionColor", new Color(1, 0.1875f, 0) * intensityHDR);
				break;
			case 1:
				sphereMaterial.SetColor("_EmissionColor", Color.red * intensityHDR);
				break;
			case 0:
				sphereMaterial.SetColor("_EmissionColor", Color.white * intensityHDR);
				// Debug.Log("Stamina is 0, Time = " + DateTime.Now);
				break;
			default:
				break;
		}
		sphereMaterial.EnableKeyword("_EMISSION");
	}

	public void DisableAttacks(bool value)
	{
		cannotAttack = value;
	}

	public bool CheckStamina(int value)
	{
		if (sphereIsDischarged || (increasingStamina && !isStaminaRecoveryInterruptible))
		{
			// Audio management: the sphere has finished the stamina or is loading after having been completely discharged
			GamePlayAudioManager.instance.PlayManagedOneShot(FMODEvents.Instance.PlayerSphereDischarge, rotatingSphere.transform.position);
			return false;
		}

		// The sphere still has stamina
		if (sphereStamina >= value && !sphereIsDischarged)
		{
			// If the stamina recovery process can still be interrupted, and it's currently running, then stop it
			if (increasingStamina)
			{
				InterruptStaminaRecovery();
			}
			
			return true;
		}

		return false;
	}

	public void DecreaseStamina(int amount)
	{
		// We update the timestamp every time the stamina is consumed
		LastStaminaUseTime = Time.time;
		sphereStamina -= amount;
		increasingStamina = false;

		// Let's make sure the stamina doesn't go below zero
		if (sphereStamina <= 0)
		{
			sphereStamina = 0;
			sphereIsDischarged = true;
		}
		ChangeSphereColor(sphereStamina);
	}

	public void StartStaminaRecovery()
	{
		// Check if a charge is already in progress to avoid starting multiple coroutines, moreover check if
		// the shield is active or not 
		if (!increasingStamina && !shieldIsActive && staminaRecoveryCoroutine == null)
		{
			increasingStamina = true; 
			isStaminaRecoveryInterruptible = true;
			staminaRecoveryCoroutine = StartCoroutine(RecoverStaminaCoroutine());
		}
	}

	private void InterruptStaminaRecovery()
	{
		// Stop the recovery process only if the coroutine is active and the process is interruptible
		if (staminaRecoveryCoroutine != null && isStaminaRecoveryInterruptible && !isStaminaRecoveryInterrupted)
		{
			isStaminaRecoveryInterrupted = true;
			// Debug.Log("Stamina recovery process was interrupted, Time = " + DateTime.Now);
			StopCoroutine(staminaRecoveryCoroutine);
			staminaRecoveryCoroutine = null;
			increasingStamina = false;
			isStaminaRecoveryInterrupted = false;
		}
		else
		{
			Debug.LogWarning("Stamina recovery process was NOT interrupted");
		}
	}

	private IEnumerator RecoverStaminaCoroutine()
	{
		// Debug.Log("Recovering stamina, Time = " + DateTime.Now);
		
		sphereIsDischarged = false;
		bool firstIteration = true;
		
		while (sphereStamina < maxSphereStamina && !loadingAttack)
		{
			yield return new WaitForSeconds(0.5f); // 500ms
			
			// Passed this checkpoint, if stamina is at least 1, the recovering process can't be interrupted anymore
			if (sphereStamina >= 0 && firstIteration)
			{
				isStaminaRecoveryInterruptible = false;
				firstIteration = false;
				
				// Audio management: start sphere charging sound
				if (rickEvents != null)
				{
					// Debug.Log("Starting sphere charging SFX");
					rickEvents.SetSphereChargingState(true); 
				}
			}
			
			if (!isStaminaRecoveryInterrupted)
			{
				sphereStamina += 1;
				// Debug.Log("Current stamina = " + sphereStamina + ", Time = " + DateTime.Now);
				ChangeSphereColor(sphereStamina);

				if (sphereStamina == maxSphereStamina)
				{
					// Audio management: stop sphere charging sound
					if (rickEvents != null)
					{
						// Debug.Log("Stopping sphere charging SFX");
						rickEvents.SetSphereChargingState(false);
					}
					
					// Audio management: play sphere full charge sound
					GamePlayAudioManager.instance.PlayManagedOneShot(FMODEvents.Instance.PlayerSphereFullCharge, rotatingSphere.transform.position);
				}
			}
		}
		increasingStamina = false;
		isStaminaRecoveryInterruptible = true;
		staminaRecoveryCoroutine = null;
	}

	private void SetSelectedAttackImage()
	{
		if (attackNumber == 1)
		{
			// Distance attack selected
			distanceAttackImage.transform.localScale = new Vector3(1, 1, 1);
			distanceAttackImage.color = new Color(255 / 255f, 255 / 255f, 255 / 255f);

			closeAttackImage.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			closeAttackImage.color = new Color(87 / 255f, 87 / 255f, 87 / 255f);

			closeAttackLoadingBar.fillAmount = 0;
		}
		else
		{
			// Close attack selected
			closeAttackImage.transform.localScale = new Vector3(1, 1, 1);
			closeAttackImage.color = new Color(255 / 255f, 255 / 255f, 255 / 255f);

			distanceAttackImage.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			distanceAttackImage.color = new(87 / 255f, 87 / 255f, 87 / 255f);

			distanceAttackLoadingBar.fillAmount = 0;
		}
	}

	void ChangeAttack(int direction)
	{
		attackNumber += direction;
		if (attackNumber > 2)
		{
			attackNumber = 1;
		}
		else if (attackNumber < 1)
		{
			attackNumber = 2;
		}

		SetSelectedAttackImage();
	}

	void SetAttack(int n)
	{
		attackNumber = n;

		SetSelectedAttackImage();
	}

	async void LoadDistanceAttack()
	{
		// If we are here the stamina is at least 1
		loadingAttack = true;

		rotateSphere.positionSphere(new Vector3(0, 0.8f, rotateSphere.DistanceFromPlayer), RotateSphere.Animation.RotateAround);
		AnimationManager.Instance.Attack();
		await Task.Delay(50);
		attackStamina = 0;

		// Let's check if the player has the power-up for the loaded attack
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp))
		{
			int maxStamina = 0;
			if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp))
			{
				if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 1)
				{
					maxStamina = Math.Min(sphereStamina, 3);
				}
				else
				{
					if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 2)
					{
						maxStamina = Math.Min(sphereStamina, 5);
					}
				}
			}

			// This delay is necessary to avoid the activation of the loading bar whenever the player press and released the attack
			// button in a very fast way (as for the loading sound)
			await Task.Delay(50);

			// Audio management: if after the delay we are still charging, start the sound
			if (loadingAttack && rickEvents != null)
			{
				rickEvents.ShouldPlayChargeSound = true;
			}

			while (attackStamina < maxStamina && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp) && loadingAttack)
			{
				attackStamina++;
				ChangeSphereColor(attackStamina);
				distanceAttackLoadingBar.fillAmount = (float)attackStamina / maxSphereStamina;
				await Task.Delay(500);
			}

			// Audio management: stop the loading sound of the attack when the loading is terminated 
			if (rickEvents != null)
			{
				rickEvents.ShouldPlayChargeSound = false;
			}
		}

		else
		{
			// The player doesn't have the power-up, so we immediately fire a normal (not loaded) distance attack
			DistanceAttackAnimation();
		}
	}

	private void DistanceAttackAnimation()
	{
		loadingAttack = false;

		// Audio management: stop the loading sound of the attack if the button is released
		if (rickEvents != null)
		{
			rickEvents.ShouldPlayChargeSound = false;
		}

		AnimationManager.Instance.EndAttack();
	}

	public async void FireDistanceAttack()
	{
		bulletPrefab.gameObject.SetActive(false);
		GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
		bullet.tag = "PlayerProjectile";
		ParticleAttackController PAC = bullet.GetComponent<ParticleAttackController>();
		
		// Compute bullet's damage
		float finalBulletDamage = PAC.initialPlayerBulletDamage;
		int staminaConsumed = (attackStamina == 0) ? 1 : attackStamina; // If not loaded, the attack consumes 1 stamina
		
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp))
		{
			int powerUpLevel = powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp];
			if (powerUpLevel == 1) // First power-up, max 3 stamina
			{
				float maxDamageForPowerUp = distanceAttackPowerUp1Damage;
				float damageRange = maxDamageForPowerUp - baseDistanceAttackDamage;
				finalBulletDamage = baseDistanceAttackDamage + (damageRange * ((float)(staminaConsumed - 1) / 2));
			}
			else if (powerUpLevel == 2) // Second power-up, max 5 stamina
			{
				float maxDamageForPowerUp = distanceAttackPowerUp2Damage;
				float damageRange = maxDamageForPowerUp - baseDistanceAttackDamage;
				finalBulletDamage = baseDistanceAttackDamage + (damageRange * ((float)(staminaConsumed - 1) / 4));
			}
			else // No power-up
			{
				finalBulletDamage = baseDistanceAttackDamage;
			}
		}
		else
		{
			finalBulletDamage = baseDistanceAttackDamage;
		}
		
		// Debug.Log("Distance attack damage = " + finalBulletDamage + ", Consumed stamina = " + staminaConsumed);
		
		PAC.playerBulletDamage = finalBulletDamage;
		PAC.targetPos = bulletSpawnTransform;
		bullet.SetActive(true);
		bulletPrefab.gameObject.SetActive(true);

		if (attackStamina == 0)
		{
			DecreaseStamina(1);
		}
		else
		{
			DecreaseStamina(attackStamina);
		}

		distanceAttackLoadingBar.fillAmount = 0;
		ResetAttack();
		await Task.Delay(500);
	}

	async void LoadCloseAttack()
	{
		// If we are here the stamina is at least 1
		loadingAttack = true;

		rotateSphere.positionSphere(new Vector3(0, 1.8f, 0), RotateSphere.Animation.Linear);
		FreezePlayer();
		AnimationManager.Instance.AreaAttack();
		await Task.Delay(50);
		attackStamina = 0;

		// Let's check if the player has the power-up for the loaded attack
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
		{
			int maxStamina = 0;
			if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
			{
				if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 1)
				{
					maxStamina = Math.Min(sphereStamina, 3);
				}
				else
				{
					if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 2)
					{
						maxStamina = Math.Min(sphereStamina, 5);
					}
				}
			}

			// This delay is necessary to avoid the activation of the loading bar whenever the player press and released the attack
			// button in a very fast way (as for the loading sound)
			await Task.Delay(50);

			// Audio management: if after the delay we are still charging, start the sound
			if (loadingAttack && rickEvents != null)
			{
				rickEvents.ShouldPlayChargeSound = true;
			}

			while (attackStamina < maxStamina && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp) && loadingAttack)
			{
				attackStamina++;
				ChangeSphereColor(attackStamina);
				closeAttackLoadingBar.fillAmount = (float)attackStamina / maxSphereStamina;
				await Task.Delay(500);
			}

			// Audio management: stop the loading sound of the attack when the loading is terminated 
			if (rickEvents != null)
			{
				rickEvents.ShouldPlayChargeSound = false;
			}
		}

		else
		{
			// The player does not have the power-up, we immediately carry out the normal (not loaded) attack
			CloseAttackAnimation();
		}

	}

	private void CloseAttackAnimation()
	{
		loadingAttack = false;

		// Audio management: stop the loading sound of the attack if the button is released
		if (rickEvents != null)
		{
			rickEvents.ShouldPlayChargeSound = false;
		}

		AnimationManager.Instance.EndAreaAttack();
	}

	public void FireCloseAttack()
	{
		// Compute close attack's damage
		finalCloseAttackDamage = defaultCloseAttackDamage;
		damageRadius = defaultDamageRadius;
		
		int staminaConsumed = (attackStamina == 0) ? 1 : attackStamina; // If not loaded, the attack consumes 1 stamina
		
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
		{
			int powerUpLevel = powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp];
			if (powerUpLevel == 1) // First power-up, max 3 stamina
			{
				float maxDamageForPowerUp = closeAttackPowerUp1Damage;
				float damageRange = maxDamageForPowerUp - baseCloseAttackDamage;
				finalCloseAttackDamage = baseCloseAttackDamage + (damageRange * ((float)(staminaConsumed - 1) / 2));
				damageRadius += ((float)(staminaConsumed - 1) / 2) * 1f; // Increase the radius proportionally to stamina
			}
			else if (powerUpLevel == 2) // Second power-up, max 5 stamina
			{
				float maxDamageForPowerUp = closeAttackPowerUp2Damage;
				float damageRange = maxDamageForPowerUp - baseCloseAttackDamage;
				finalCloseAttackDamage = baseCloseAttackDamage + (damageRange * ((float)(staminaConsumed - 1) / 4));
				damageRadius += ((float)(staminaConsumed - 1) / 4) * 1f; // Increase the radius proportionally to stamina
			}
			else // No power-up
			{
				finalCloseAttackDamage = baseCloseAttackDamage;
			}
		}
		else
		{
			finalCloseAttackDamage = baseCloseAttackDamage;
		}
		
		// Debug.Log("Close attack damage = " + finalCloseAttackDamage + ", Consumed stamina = " + staminaConsumed);
		
		if (attackStamina == 0)
		{
			DecreaseStamina(1);
		}
		else
		{
			DecreaseStamina(attackStamina);
		}

		closeAttackLoadingBar.fillAmount = 0;
	}

	public async void ResetCloseAttackValues()
	{
		// Returns the sphere to its default position
		rotateSphere.positionSphere(new Vector3(rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);

		// Wait for the ball return animation to have had time to complete
		await Task.Delay(300);

		// Restore free spin and status flags
		ResetAttack();
	}

	public void ResetAttack()
	{
		loadingAttack = false;
		attacking = false;
		rotateSphere.isRotating = true;
		attackStamina = 0;
	}

	private void SpawnMagneticShield()
	{
		if (!CheckStamina(1))
		{
			return; // Exits the function if the shield cannot be activated
		}

		// Without this check, if the button for activating/deactivating the shield is pushed and released more than once in a very fast way, then
		// the function is called multiple times, creating a race condition among multiple concurrent instances of it (buggy code)
		if (isShieldCoroutineRunning)
		{
			return;
		}

		isShieldCoroutineRunning = true;

		if (!shieldIsActive)
		{
			// Decrease sphere's stamina and disable player's attacks until the shield is closed  
			DecreaseStamina(1);

			// To modify for the instantiation of the vfx and launch defense animation
			AnimationManager.Instance.Defense();
			SetShieldIsActive(true);
			FreezePlayer();
		}

		isShieldCoroutineRunning = false;
	}

	public void SetShieldIsActive(bool value)
	{
		shieldIsActive = value;
	}

	public void FreezePlayer()
	{
		player.isFrozen = true;
	}

	public void UnfreezePlayer()
	{
		player.isFrozen = false;
	}

	public void TakeDamage(float damage, DamageTypes damageType, Transform enemyTransform = null)
	{
		if (isDying)
		{
			return; // If the player is already dying, ignore any further damage
		}
		
		// If the shield is active then no damage is taken by the player and his hit animation does not start
		if (shieldIsActive)
		{
			// Debug.Log("Damage blocked by the shield!");
			GamePlayAudioManager.instance.PlayManagedOneShot(FMODEvents.Instance.PlayerShieldHit, transform.position);
			return; // Exits the function, canceling the damage
		}

		// Debug.Log("Take damage");
		health -= damage * damageReduction;
		if (health < 0) health = 0; // To prevent health from going below zero in the UI

		healthBar.SetHealth(health);

		StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));

		// Audio management: call the new method to update the audio status
		UpdateHealthState();

		if (health > 0)
		{
			// Audio management: he notifies RickEvents that damage has occurred and that he must handle the sound
			if (rickEvents != null)
			{
				rickEvents.RequestHitSound(damageType);
			}

			HitAnimation(damageType, 0, 1);
		}
		else
		{
			isDying = true;
			DisableAttacks(true);
			player.FreezeMovement(true);
			gameTimer.isRunning = false;

			// Audio management: make sure your heartbeat stops at death
			if (rickEvents != null)
			{
				rickEvents.SetHeartbeatStatus(false);
			}

			if (damageType == DamageTypes.DrakeBiteAttack)
			{
				AnimationManager.Instance.Bite();
			}
			else
			{
				DeathAnimation(1, 1);
			}
		}
	}

	public void DeathAnimation(int x, int z)
	{
		AnimationManager.Instance.Death(x, z);
	}

	public void SetLayerToZero()
	{
		gameObject.layer = 0;
	}

	public void LoadRespawnScene()
	{
		Invoke(nameof(DestroyPlayer), 1f);

		FadeManager.Instance.FadeOutIn(() =>
		{
			StartCoroutine(LoadRespawnSceneAsync());
		});
	}

	// Change player color when hit and change it back to normal after "duration" seconds
	IEnumerator ChangeColor(Renderer renderer, Color dmgColor, float duration, float delay)
	{
		// Save the original color of the enemy
		Color originColor = renderer.material.color;

		renderer.material.color = dmgColor;

		yield return new WaitForSeconds(delay);

		// Lerp animation with given duration in seconds
		for (float t = 0; t < 1.0f || renderer.material.color != originColor; t += Time.deltaTime / duration)
		{
			renderer.material.color = Color.Lerp(dmgColor, originColor, t);

			yield return null;
		}

		renderer.material.color = originColor;
	}
	private void HitAnimation(DamageTypes damageType, int x, int z)
	{
		if (!cannotAttack && !player.isFrozen)
		{
			switch (damageType)
			{
				case DamageTypes.Spit:
					DisableAttacks(true);
					player.FreezeMovement(true);
					AnimationManager.Instance.HitSpit(x, z);
					break;
				case DamageTypes.MaynardDistanceAttack:
					DisableAttacks(true);
					player.FreezeMovement(true);
					AnimationManager.Instance.Hit(x, z);
					break;
				case DamageTypes.CloseAttack:
					DisableAttacks(true);
					player.FreezeMovement(true);
					AnimationManager.Instance.Hit(x, z);
					break;
				case DamageTypes.DrakeBiteAttack:
					DisableAttacks(true);
					player.FreezeMovement(true);
					AnimationManager.Instance.Bite();
					break;
			}
		}
	}
	public void FreePlayer()
	{
		DisableAttacks(false);
		player.FreezeMovement(false);
	}
	private void DestroyPlayer()
	{
		Destroy(gameObject);
	}
	private IEnumerator LoadRespawnSceneAsync()
	{
		// Asynchronous loading of scene starts
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(respawnSceneName);
		if (asyncLoad != null)
		{
			asyncLoad.allowSceneActivation = false;

			// Wait until the scene is almost ready (>= 0.9)
			while (asyncLoad.progress < 0.9f)
			{
				yield return null;
			}

			// Now actually activates the scene
			asyncLoad.allowSceneActivation = true;
		}
	}

	public void RecoverHealth(float amount)
	{
		health += amount;
		if (health > maxHealth)
		{
			health = maxHealth;
		}

		healthBar.SetHealth(health);
		// Audio management: call the update method even when recovering life
		UpdateHealthState();
	}

	void ProcessPlayerInput()
	{
		//debug Rick State
		//Debug.Log($"Rick state: {AnimationManager.Instance.rickState}");
		
		if (!GameStatus.gamePaused)
		{
			if (!cannotAttack)
			{
				// The attack is shot only on "Fire1" up && AnimationManager.Instance.rickState == RickStates.Idle
				if (Input.GetButtonDown("Fire1"))
				{
					if (shieldIsActive)
					{
						// If the player has enough stamina for the attack, then deactivate the shield immediately and
						// play the deactivation sound
						if (rickEvents != null && CheckStamina(1))
						{
							rickEvents.InstantShieldDeactivation();
						}
						SetShieldIsActive(false); // Set shield as inactive
						// At this point, the player can proceed with the attack
						if (!attacking && CheckStamina(1)) // Check the stamina even after the shield is deactivated
						{
							loadingAttack = true;
							attacking = true;

							switch (attackNumber)
							{
								case 1:
									LoadDistanceAttack();
									break;
								case 2:
									LoadCloseAttack();
									break;
								default:
									break;
							}
						}
					}
					else if (!attacking && CheckStamina(1))
					{
						loadingAttack = true;
						attacking = true;

						switch (attackNumber)
						{
							case 1:
								LoadDistanceAttack();
								break;
							case 2:
								LoadCloseAttack();
								break;
							default:
								break;
						}
					}
				}

				if (Input.GetButtonUp("Fire1"))
				{
					if (!shieldIsActive && loadingAttack && CheckStamina(1))
					{
						switch (attackNumber)
						{
							case 1:
								DistanceAttackAnimation();
								break;
							case 2:
								CloseAttackAnimation();
								break;
							default:
								break;
						}
					}
				}

				//&& AnimationManager.Instance.rickState == RickStates.Idle
				if (Input.GetButtonDown("Fire2") && !loadingAttack)
				{
					SpawnMagneticShield();
				}

				// Selecting a different attack
				if ((Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(increaseAttackController)) &&
					!loadingAttack)
				{
					ChangeAttack(1);
				}
				else if ((Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(decreaseAttackController)) &&
						 !loadingAttack)
				{
					ChangeAttack(-1);
				}

				if (Input.GetKeyDown(KeyCode.Alpha1) && !loadingAttack)
				{
					SetAttack(1);
				}

				if (Input.GetKeyDown(KeyCode.Alpha2) && !loadingAttack)
				{
					SetAttack(2);
				}
			}
		}
	}

	public void UpdateHealthState()
	{
		if (rickEvents == null) return;

		// Calculate whether health is below the threshold (but the player is still alive)
		bool isHealthLow = (health <= maxHealth * LOW_HEALTH_PERCENTAGE && health > 0);

		// Audio management: communicate status to RickEvents
		rickEvents.SetHeartbeatStatus(isHealthLow);

		// Notify HealthBar of the status for flashing
		if (healthBar != null)
		{
			healthBar.SetFlashing(isHealthLow);
		}
	}
}