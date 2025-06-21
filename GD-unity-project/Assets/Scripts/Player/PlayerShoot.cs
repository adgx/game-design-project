using System;
using System.Collections;
using System.Threading.Tasks;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using ORF;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour
{
	// Audio management 
	public bool IsSphereRotating => rotateSphere.isRotating;
	private EventInstance distanceAttackLoading;
	private EventInstance closeAttackLoading;
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

	// DistantAttackDamage
	public GetCollisions getCollisions;

	private Player player;
	[SerializeField] private RotateSphere rotateSphere;
	[SerializeField] private GameObject rotatingSphere;

	private void Start() {
		healthBar.SetMaxHealth(health);
		player = GetComponent<Player>();
		
		// Audio management
		distanceAttackLoading = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerDistanceAttackLoad);
		distanceAttackLoading.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		
		closeAttackLoading = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerCloseAttackLoad);
		closeAttackLoading.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
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
			await Task.Delay(700);
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
		rotateSphere.positionSphere(new Vector3(0, 0.5f, 0.7f), RotateSphere.Animation.RotateAround);
		
		// Audio management
		bool playDistanceAttackSound = false;
		await Task.Delay(50);
		
		if (Input.GetButton("Fire1") && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp))
		{
			playDistanceAttackSound = true;
		}
		
		if (playDistanceAttackSound)
		{
			distanceAttackLoading.stop(STOP_MODE.IMMEDIATE); // reset
			distanceAttackLoading.start(); // start
			_ = StopLoadingSoundAfterDelay(distanceAttackLoading, 2500);
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

		await Task.Delay(100);
		attacking = false;
		
		await Task.Delay(200);
		rotateSphere.isRotating = true;
		player.isFrozen = false;
		
		// Audio management
		distanceAttackLoading.stop(STOP_MODE.ALLOWFADEOUT);
	}
	
	async void LoadCloseAttack() {
		// If we are here the stamina is at least 1
		loadingAttack = true;
		rotateSphere.positionSphere(new Vector3(0, 1.3f, 0), RotateSphere.Animation.Linear);
		
		// Audio management
		bool playCloseAttackSound = false;
		await Task.Delay(50);
		
		if (Input.GetButton("Fire1") && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
		{
			playCloseAttackSound = true;
		}
		
		if (playCloseAttackSound)
		{
			closeAttackLoading.stop(STOP_MODE.IMMEDIATE); // reset loading SFX 
			closeAttackLoading.start(); // start loading SFX
			_ = StopLoadingSoundAfterDelay(closeAttackLoading, 2500);
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
	
	void FireCloseAttack() {
		loadingAttack = false;
		DecreaseStamina(attackStamina);
		
		// Audio management
		GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerCloseAttackShoot, rotatingSphere.transform.position);

		closeAttackLoadingBar.fillAmount = 0;

		SpawnAttackArea();

		CheckForEnemies();
		
		// Audio management
		closeAttackLoading.stop(STOP_MODE.ALLOWFADEOUT);
	}

	async void SpawnAttackArea() {
		
		GameObject attackArea = Instantiate(attackAreaPrefab, transform.position, Quaternion.identity);
		attackArea.transform.parent = transform;
		attackArea.transform.localScale = new Vector3(2 * damageRadius, 0, 2 * damageRadius);
		player.isFrozen = true;

		await Task.Delay(500);

		Destroy(attackArea);
		player.isFrozen = false;

		rotateSphere.positionSphere(new Vector3(0.7f, 0.5f, 0), RotateSphere.Animation.Linear);
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
			if(c.transform.tag.Contains("Enemy")) {
				c.GetComponent<Enemy.EnemyManager.IEnemy>().TakeDamage(closeAttackDamage);
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

	public void TakeDamage(int damage) {
     		health -= damage * damageReduction;
     		healthBar.SetHealth(health);
     
     		StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));
     
     		if (health <= 0)
     		{
     			// Audio management
     			GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDie, player.transform.position);
     			
     			Invoke(nameof(DestroyPlayer), 0.05f);
     		}
     			
     		else
     		{
     			// Audio management
     			GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHit, player.transform.position);
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
	private void DestroyPlayer() {
		Destroy(gameObject);
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
			if ((Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(increaseAttackController)) && !loadingAttack)
			{
				ChangeAttack(1);
			}
			else if ((Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(decreaseAttackController)) && !loadingAttack)
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
	
	// Audio management
	private void UpdateSound() 
	{
		distanceAttackLoading.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		closeAttackLoading.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotateSphere.transform));
	}
}