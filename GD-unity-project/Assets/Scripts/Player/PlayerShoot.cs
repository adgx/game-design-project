using System;
using System.Collections;
using System.Threading.Tasks;
using Audio;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Helper;

public class PlayerShoot : MonoBehaviour
{
	// Audio management 
	public bool IsSphereRotating => rotateSphere.isRotating;
	private EventInstance distanceAttackLoadingWithPowerUp1;
	private EventInstance closeAttackLoadingWithPowerUp1;
	private EventInstance distanceAttackLoadingWithPowerUp2;
	private EventInstance closeAttackLoadingWithPowerUp2;
	private bool isShieldCoroutineRunning;
	
	// Attack1
	[SerializeField] private float bulletSpeed;
	// The point in which the bullet spawns
	[SerializeField] private Transform bulletSpawnTransform;
	[SerializeField] private GameObject bulletPrefab;

	// Attack2
	[SerializeField] private GameObject attackAreaPrefab;
	private int defaultCloseAttackDamage = 50;
	private int closeAttackDamage = 50;
	private float defaultDamageRadius = 2.5f;
	private float damageRadius = 2.5f;

	private bool cannotAttack = false;

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
	
	// Defense
	[SerializeField] private GameObject magneticShieldPrefab;
	GameObject magneticShield;
	private bool magneticShieldOpen = false;
	
	// Health
	public float maxHealth = 120;
	public float health;
	public float damageReduction = 1f;
	[SerializeField] private HealthBar healthBar;

	// Stamina for the attacks
	public int maxSphereStamina = 5;
	public bool increaseStamina = false, increasingStamina = false;
	public int sphereStamina = 5;

	// PowerUps
	public PowerUp powerUp;

	// Needed to set DistantAttackDamage
	public GetCollisions getCollisions;

	private Player player;
	[SerializeField] private RotateSphere rotateSphere;
	[SerializeField] private GameObject rotatingSphere;

	[SerializeField] private string respawnSceneName = "RespawnScene";

	public enum DamageTypes {
		Spit,
		MaynardDistanceAttack,
		CloseAttack,
		DrakeBiteAttack
	}

	private void Start() {
		healthBar.SetMaxHealth(health);
		player = GetComponent<Player>();
		
		// Audio management
		distanceAttackLoadingWithPowerUp1 = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerDistanceAttackLoadWithPowerUp1);
		distanceAttackLoadingWithPowerUp1.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		
		closeAttackLoadingWithPowerUp1 = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerCloseAttackLoadWithPowerUp1);
		closeAttackLoadingWithPowerUp1.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		
		distanceAttackLoadingWithPowerUp2 = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerDistanceAttackLoadWithPowerUp2);
		distanceAttackLoadingWithPowerUp2.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		
		closeAttackLoadingWithPowerUp2 = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerCloseAttackLoadWithPowerUp2);
		closeAttackLoadingWithPowerUp2.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
	}
	
	// Audio management
	private async Task StopLoadingSoundAfterDelay(EventInstance instance, int delayMs)
	{
		await Task.Delay(delayMs);

		if (!instance.isValid())
			return;

		PLAYBACK_STATE state;
		var result = instance.getPlaybackState(out state);
		if (result != FMOD.RESULT.OK)
			return;

		if (state != PLAYBACK_STATE.STOPPED)
			instance.stop(STOP_MODE.ALLOWFADEOUT);
	}
	
	void ChangeSphereColor() {
		// TODO: chiedere ad Antonino come cambiare il colore della sfera
		switch(sphereStamina) {
			case 5:
				// rotatingSphere.ledColor = Color.blue;
				break;
			case 4:
				// rotatingSphere.ledColor = Color.green;
				break;
			case 3:
				// rotatingSphere.ledColor = Color.yellow;
				break;
			case 2:
				// rotatingSphere.ledColor = Color.orange;
				break;
			case 1:
				// rotatingSphere.ledColor = Color.red;
				break;
			case 0:
				// rotatingSphere.ledColor = Color.red;
				break;
			default:
				break;
		}
	}

	public void DisableAttacks(bool value)
	{
		cannotAttack = value;
	}

	private bool CheckStamina(int value) {
		// The Sphere still has stamina
		if (sphereStamina >= value) {
			return true;
		}
		
		// The Sphere has finished the stamina
		// Audio management
		GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerSphereDischarge, rotatingSphere.transform.position);
		return false;
	}

	public void DecreaseStamina(int amount) {
		sphereStamina -= amount;
		increaseStamina = false;
	}

	public async Task RecoverStamina() {
		increasingStamina = true;
		while(sphereStamina < maxSphereStamina && increaseStamina && !loadingAttack) {
			await Task.Delay(500);
			if(increaseStamina && !loadingAttack) {
				sphereStamina += 1;
				
				// Audio management
				if (sphereStamina == 5)
				{
					GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerSphereFullRecharge, rotatingSphere.transform.position);
				}
			}
		}
		increasingStamina = false;
	}

	void setSelectedAttackImage() {
		if(attackNumber == 1) {
			// Distance attack selected
			distanceAttackImage.transform.localScale = new Vector3(1, 1, 1);
			distanceAttackImage.color = new Color(255 / 255f, 255 / 255f, 255 / 255f);

			closeAttackImage.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			closeAttackImage.color = new Color(87 / 255f, 87 / 255f, 87 / 255f);

			closeAttackLoadingBar.fillAmount = 0;
		}
		else {
			// Close attack selected
			closeAttackImage.transform.localScale = new Vector3(1, 1, 1);
			closeAttackImage.color = new Color(255 / 255f, 255 / 255f, 255 / 255f);

			distanceAttackImage.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			distanceAttackImage.color = new (87 / 255f, 87 / 255f, 87 / 255f);

			distanceAttackLoadingBar.fillAmount = 0;
		}
	}

	void ChangeAttack(int direction) {
		attackNumber += direction;
		if(attackNumber > 2) {
			attackNumber = 1;
		}
		else if(attackNumber < 1) {
			attackNumber = 2;
		}

		setSelectedAttackImage();
	}

	void SetAttack(int n) {
		attackNumber = n;

		setSelectedAttackImage();
	}
	
	async void LoadDistanceAttack() {
		// If we are here the stamina is at least 1
		loadingAttack = true;
		rotateSphere.positionSphere(new Vector3(0, 1f, 0.7f), RotateSphere.Animation.RotateAround);
		AnimationManager.Instance.Idle();
		AnimationManager.Instance.Attack();
		
		// Audio management
		bool playDistanceAttackSound = false;
		await Task.Delay(50);
		
		if (Input.GetButton("Fire1") && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp))
		{
			playDistanceAttackSound = true;
		}
		
		if (playDistanceAttackSound)
		{
			// TODO: modify the delay based on the available power ups

			if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 1)
			{
				distanceAttackLoadingWithPowerUp1.stop(STOP_MODE.IMMEDIATE); // reset
				distanceAttackLoadingWithPowerUp1.start(); // start
				_ = StopLoadingSoundAfterDelay(distanceAttackLoadingWithPowerUp1, 1500);
			}
			else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 2)
			{
				distanceAttackLoadingWithPowerUp2.stop(STOP_MODE.IMMEDIATE); // reset
				distanceAttackLoadingWithPowerUp2.start(); // start
				_ = StopLoadingSoundAfterDelay(distanceAttackLoadingWithPowerUp2, 2500);	
			}
		}

		attackStamina = 0;
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
		
		while (attackStamina < maxStamina && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp) && loadingAttack)
		{
			attackStamina++;

			distanceAttackLoadingBar.fillAmount = (float)attackStamina / maxSphereStamina;

			if (attackStamina > 1) {
				getCollisions.playerBulletDamage += 10;
			}
			
			await Task.Delay(500);
		}
	}
	
	async void FireDistanceAttack() {
		loadingAttack = false;
		AnimationManager.Instance.EndAttack();
		
		if (attackStamina == 0)
			await Task.Delay(700);
		else
			await Task.Delay(500);
		
		GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
		bullet.tag = "PlayerProjectile";
		
		Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
		rbBullet.AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);
		rbBullet.AddForce(bulletSpawnTransform.up * 2f, ForceMode.Impulse);

		getCollisions.playerBulletDamage = getCollisions.initialPlayerBulletDamage;

		if (attackStamina == 0) {
			DecreaseStamina(1);
		}
		else {
			DecreaseStamina(attackStamina);
		}
		
		// Audio management
		GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDistanceAttackShoot, rotatingSphere.transform.position);

		distanceAttackLoadingBar.fillAmount = 0;

		await Task.Delay(500);
		attacking = false;
		
		rotateSphere.isRotating = true;
		player.isFrozen = false;
		
		// Audio management
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp))
		{
			if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 1)
			{
				distanceAttackLoadingWithPowerUp1.stop(STOP_MODE.ALLOWFADEOUT);	
			}
			else
			{
				distanceAttackLoadingWithPowerUp2.stop(STOP_MODE.ALLOWFADEOUT);	
			}
		}
	}
	
	async void LoadCloseAttack() {
		// If we are here the stamina is at least 1
		loadingAttack = true;
		rotateSphere.positionSphere(new Vector3(0, 1.8f, 0), RotateSphere.Animation.Linear);
		AnimationManager.Instance.Idle();
		AnimationManager.Instance.AreaAttack();
		
		// Audio management
		bool playCloseAttackSound = false;
		await Task.Delay(50);
		
		if (Input.GetButton("Fire1") && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
		{
			playCloseAttackSound = true;
		}
		
		if (playCloseAttackSound)
		{
			// TODO: modify the delay based on the available power ups 
			
			if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 1)
			{
				closeAttackLoadingWithPowerUp1.stop(STOP_MODE.IMMEDIATE); // reset loading SFX 
				closeAttackLoadingWithPowerUp1.start(); // start loading SFX
				_ = StopLoadingSoundAfterDelay(closeAttackLoadingWithPowerUp1, 1500);
			}
			else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 2)
			{
				closeAttackLoadingWithPowerUp2.stop(STOP_MODE.IMMEDIATE); // reset loading SFX 
				closeAttackLoadingWithPowerUp2.start(); // start loading SFX
				_ = StopLoadingSoundAfterDelay(closeAttackLoadingWithPowerUp2, 2500);	
			}
		}
		
		attackStamina = 0;
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
		
		while (attackStamina < maxStamina && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp) && loadingAttack) {
			attackStamina++;

			closeAttackLoadingBar.fillAmount = (float)attackStamina / maxSphereStamina;

			if (attackStamina > 1) {
				damageRadius += 1f;
				closeAttackDamage += 20;
			}
			
			await Task.Delay(500);
		}
	}
	
	async void FireCloseAttack() {
		loadingAttack = false;
		player.isFrozen = true;
		AnimationManager.Instance.EndAreaAttack();
		
		if (attackStamina == 0)
			await Task.Delay(900);
		else
			await Task.Delay(500);
		
		
		DecreaseStamina(attackStamina);
		
		// Audio management
		GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerCloseAttackShoot, rotatingSphere.transform.position);

		closeAttackLoadingBar.fillAmount = 0;

		SpawnAttackArea();
		
		// Audio management
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
		{
			if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 1)
			{
				closeAttackLoadingWithPowerUp1.stop(STOP_MODE.ALLOWFADEOUT);	
			}
			else
			{
				closeAttackLoadingWithPowerUp2.stop(STOP_MODE.ALLOWFADEOUT);	
			}	
		}
	}

	async void SpawnAttackArea() {
		
		GameObject attackArea = Instantiate(attackAreaPrefab, transform.position, Quaternion.identity);
		attackArea.transform.parent = transform;
		attackArea.transform.localScale = new Vector3(2 * damageRadius, 0, 2 * damageRadius);
		
		CheckForEnemies();

		await Task.Delay(500);

		Destroy(attackArea);
		player.isFrozen = false;

		rotateSphere.positionSphere(new Vector3(0.7f, 1f, 0), RotateSphere.Animation.Linear);
		await Task.Delay(300);
		rotateSphere.isRotating = true;

		// Set values back to default
		closeAttackDamage = defaultCloseAttackDamage;
		damageRadius = defaultDamageRadius;
		
		attacking = false;
	}

	// Checks if there are enemies in the attack area and, if so, damages them
	void CheckForEnemies() {
		Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
		foreach(Collider c in colliders) {
			// Checks if the collider is an enemy
			if(c.transform.tag.Contains("Enemy") && !c.transform.tag.Contains("EnemyAttack")) {
				c.GetComponent<Enemy.EnemyManager.IEnemy>().TakeDamage(closeAttackDamage, "c");
			}
		}
	}
	
	async Task<bool> WaitUntilOrTimeout(Func<bool> condition, int timeoutMs, int checkIntervalMs = 25)
	{
		var stopwatch = System.Diagnostics.Stopwatch.StartNew();

		while (stopwatch.ElapsedMilliseconds < timeoutMs)
		{
			if (condition())
				return true;

			await Task.Delay(checkIntervalMs);
		}

		return false; // Timeout scaduto
	}
	
	async Task ManageShieldTime(int shieldTime)
	{
		bool shieldClosed = await WaitUntilOrTimeout(() => !magneticShieldOpen, shieldTime * 1000);
		if (magneticShield != null && !shieldClosed)
		{
			// Audio management
			GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldDeactivation, rotatingSphere.transform.position);
			Destroy(magneticShield);
			await Task.Delay(500);
		}
		player.isFrozen = false;
		magneticShieldOpen = false;
	}

	async void SpawnMagneticShield() {
		// Without this check, if the button for activating/deactivating the shield is pushed and released more than once in a very fast way, then
		// the function is called multiple times, creating a race condition among multiple concurrent instances of it (buggy code)
		if (isShieldCoroutineRunning)
		{
			return;
		}
		
		int shieldTime = 2;
		isShieldCoroutineRunning = true;
		
		if(!magneticShieldOpen) 
		{
			// Audio management
			GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldActivation, rotatingSphere.transform.position);

			// to modify for the instantiate the vfx and lunch the animation character
			//luch defense animation
			AnimationManager.Instance.Defense();
			//magneticShield = Instantiate(magneticShieldPrefab, new Vector3(player.transform.position.x, player.transform.position.y - 1f, player.transform.position.z), Quaternion.identity);
			//magneticShield.transform.parent = transform;
			magneticShieldOpen = true;
			player.isFrozen = true;
			
			if(powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DefensePowerUp)) {
				if(powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 1) {
					shieldTime = 5;
				}
				else {
					shieldTime = 10;
				}
			}

			_ = ManageShieldTime(shieldTime);
		}
		else 
		{
			if(magneticShield != null) {
				// Audio management
				GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldDeactivation, rotatingSphere.transform.position);
				Destroy(magneticShield);
				await Task.Delay(500);
			}
			player.isFrozen = false;
			magneticShieldOpen = false;
		}
		
		isShieldCoroutineRunning = false;
	}

	public async void TakeDamage(float damage, DamageTypes damageType, float x, float z) {
     	health -= damage * damageReduction;
     	healthBar.SetHealth(health);
     
     	StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));

		if(health > 0) {
			HitAnimation(damageType, x, z);

			// Audio management
			GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHit, player.transform.position);
		}
		else
     	{
	        DisableAttacks(true);
	        player.FreezeMovement(true);
	        if (damageType == DamageTypes.DrakeBiteAttack)
	        {
		        // Audio management
		        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHit, player.transform.position);
		        
		        AnimationManager.Instance.Bite();
		        await Task.Delay(2000);
	        }
	        
     		// Audio management
     		GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieForwardGrunt, player.transform.position);

			gameObject.layer = 0;
		    AnimationManager.Instance.Death(x, z);
			await Task.Delay(2500);

     		Invoke(nameof(DestroyPlayer), 1f);

			FadeManager.Instance.FadeOutIn(() => {
				StartCoroutine(LoadRespawnSceneAsync());
			});
     	}
	}
	
	// Change player color when hit and change it back to normal after "duration" seconds
	IEnumerator ChangeColor(Renderer renderer, Color dmgColor, float duration, float delay) {
		// Save the original color of the enemy
		Color originColor = renderer.material.color;

		renderer.material.color = dmgColor;

		yield return new WaitForSeconds(delay);

		// Lerp animation with given duration in seconds
		for(float t = 0; t < 1.0f || renderer.material.color != originColor; t += Time.deltaTime / duration) {
			renderer.material.color = Color.Lerp(dmgColor, originColor, t);

			yield return null;
		}

		renderer.material.color = originColor;
	}
	private async void HitAnimation(DamageTypes damageType, float x, float z) {
		if(!cannotAttack && !player.isFrozen) {
			switch(damageType) {
				case DamageTypes.Spit:
					DisableAttacks(true);
					player.FreezeMovement(true);
					AnimationManager.Instance.HitSpit(x, z);
					await Task.Delay(2000);

					DisableAttacks(false);
					player.FreezeMovement(false);
					break;
				case DamageTypes.MaynardDistanceAttack:
					DisableAttacks(true);
					player.FreezeMovement(true);
					AnimationManager.Instance.Hit(x, z);
					await Task.Delay(1000);

					DisableAttacks(false);
					player.FreezeMovement(false);
					break;
				case DamageTypes.CloseAttack:
					DisableAttacks(true);
					player.FreezeMovement(true);
					AnimationManager.Instance.Hit(x, z);
					await Task.Delay(1000);

					DisableAttacks(false);
					player.FreezeMovement(false);
					break;
				case DamageTypes.DrakeBiteAttack:
					DisableAttacks(true);
					player.FreezeMovement(true);
					AnimationManager.Instance.Bite();
					await Task.Delay(1000);

					DisableAttacks(false);
					player.FreezeMovement(false);
					break;
			}
		}
	}
	private void DestroyPlayer() {
		Destroy(gameObject);
	}
	private IEnumerator LoadRespawnSceneAsync() {

		// Inizia il caricamento asincrono della scena
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(respawnSceneName);
		asyncLoad.allowSceneActivation = false;

		// Attendi finch� la scena � quasi pronta (>= 0.9)
		while(asyncLoad.progress < 0.9f) {
			yield return null;
		}

		// Ora attiva effettivamente la scena
		asyncLoad.allowSceneActivation = true;
	}

	public void RecoverHealth(float amount) {
		health += amount;
		if (health > maxHealth) {
			health = maxHealth;
		}
		
		healthBar.SetHealth(health);
	}

	void Update() {
		if (!GameStatus.gamePaused)
		{
			if (!cannotAttack)
			{
				// The attack is shot only on "Fire1" up
				if (Input.GetButtonDown("Fire1"))
				{
					if (!magneticShield && CheckStamina(1) && !attacking)
					{
						// Audio management
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

				// Audio management
				UpdateSound();

				if (Input.GetButtonUp("Fire1"))
				{
					if (!magneticShield && CheckStamina(1) && loadingAttack)
					{
						switch (attackNumber)
						{
							case 1:
								FireDistanceAttack();
								break;
							case 2:
								FireCloseAttack();
								break;
							default:
								break;
						}
					}
				}

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

				// I check the stamina every frame since it is possible that it is = 0 when I am not attacking (thanks asynchronous processes)
				// Not that good, but I don't have better ways to manage it
				if (sphereStamina <= 0)
				{
					if (!increasingStamina)
					{
						increaseStamina = true;
						_ = RecoverStamina();
					}
				}
			}
		}
	}
	
	// Audio management
	private void UpdateSound() 
	{
		distanceAttackLoadingWithPowerUp1.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		closeAttackLoadingWithPowerUp1.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotateSphere.transform));
		
		distanceAttackLoadingWithPowerUp2.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		closeAttackLoadingWithPowerUp2.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotateSphere.transform));
	}
}