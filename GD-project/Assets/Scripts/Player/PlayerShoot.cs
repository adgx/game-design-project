using System;
using System.Collections;
using System.Threading.Tasks;
using FMOD.Studio;
using TMPro;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
	// Audio management 
	private EventInstance distanceAttackLoading;
	private EventInstance closeAttackLoading;
	private bool shieldActivated = false;
	private bool isShieldCoroutineRunning = false;
	
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

	// The player has 2 attacks he can choose. He can change them by using the mouse scroll wheel
	private int attackNumber = 1;
	[SerializeField] TextMeshProUGUI selectedAttackText;
	private bool loadingAttack = false;
	private int attackStamina = 0;
	
	// Defense
	[SerializeField] private GameObject magneticShieldPrefab;
    GameObject magneticShield;
	private bool magneticShieldOpen = false;
	
	// Health
	public float maxHealth = 120;
	public float health;
	[SerializeField] private HealthBar healthBar;

	// Stamina for the attacks
	private int maxSphereStamina = 5;
	private bool increaseStamina = false, increasingStamina = false;
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
		distanceAttackLoading = AudioManager.instance.CreateInstance(FMODEvents.instance.distanceAttackLoad);
		distanceAttackLoading.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		
		closeAttackLoading = AudioManager.instance.CreateInstance(FMODEvents.instance.closeAttackLoad);
		closeAttackLoading.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
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
		AudioManager.instance.PlayOneShot(FMODEvents.instance.sphereDischarge, rotatingSphere.transform.position);
		return false;
	}

	public void DecreaseStamina(int amount) {
		sphereStamina -= amount;
		increaseStamina = false;
	}

	async void RecoverStamina() {
		increasingStamina = true;
		while(sphereStamina < maxSphereStamina && increaseStamina) {
			await Task.Delay(700);
			if(increaseStamina) {
				sphereStamina += 1;
			}
		}
		increasingStamina = false;
	}

	void ChangeAttack(int direction) {
		attackNumber += direction;
		if(attackNumber > 2) {
			attackNumber = 1;
		}
		else if(attackNumber < 1) {
			attackNumber = 2;
		}

		selectedAttackText.text = "Attack " + attackNumber;
	}

	void SetAttack(int n) {
		attackNumber = n;
		selectedAttackText.text = "Attack " + attackNumber;
	}
	
	async void LoadDistanceAttack() {
		// If we are here the stamina is at least 1
		rotateSphere.positionSphere(new Vector3(0, 0, 1), RotateSphere.Animation.RotateAround);

		attackStamina = 0;
		int maxStamina = 0;
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.DistanceAttackPowerUp))
		{
			if (powerUp.powerUpsObtained[PowerUp.PowerUpType.DistanceAttackPowerUp] == 1)
			{
				maxStamina = Math.Min(sphereStamina, 3);
			}
			else
			{
				if (powerUp.powerUpsObtained[PowerUp.PowerUpType.DistanceAttackPowerUp] == 2)
				{
					maxStamina = Math.Min(sphereStamina, 5);
				}
			}
		}
		
		while (attackStamina < maxStamina && powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.DistanceAttackPowerUp) && loadingAttack)
		{
			attackStamina++;
				
			Debug.Log(attackStamina);
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
		AudioManager.instance.PlayOneShot(FMODEvents.instance.distanceAttackShoot, rotatingSphere.transform.position);
		
		await Task.Delay(300);
		rotateSphere.rotateSphere = true;
		player.isFrozen = false;
	}
	
	async void LoadCloseAttack() {
		// If we are here the stamina is at least 1
		rotateSphere.positionSphere(new Vector3(0, 1, 0), RotateSphere.Animation.Linear);
		
		attackStamina = 0;
		int maxStamina = 0;
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.CloseAttackPowerUp))
		{
			if (powerUp.powerUpsObtained[PowerUp.PowerUpType.CloseAttackPowerUp] == 1)
			{
				maxStamina = Math.Min(sphereStamina, 3);
			}
			else
			{
				if (powerUp.powerUpsObtained[PowerUp.PowerUpType.CloseAttackPowerUp] == 2)
				{
					maxStamina = Math.Min(sphereStamina, 5);
				}
			}
		}
		
		while (attackStamina < maxStamina && powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.CloseAttackPowerUp) && loadingAttack) {
			attackStamina++;
			Debug.Log(attackStamina);

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
		AudioManager.instance.PlayOneShot(FMODEvents.instance.closeAttackShoot, rotatingSphere.transform.position);

		SpawnAttackArea();

		CheckForEnemies();
	}

	async void SpawnAttackArea() {
		
		GameObject attackArea = Instantiate(attackAreaPrefab, transform.position, Quaternion.identity);
		attackArea.transform.parent = transform;
		attackArea.transform.localScale = new Vector3(2 * damageRadius, 0, 2 * damageRadius);
		player.isFrozen = true;

		await Task.Delay(500);

		Destroy(attackArea);
		player.isFrozen = false;

		rotateSphere.positionSphere(new Vector3(1, 0, 0), RotateSphere.Animation.Linear);
		await Task.Delay(300);
		rotateSphere.rotateSphere = true;

		// Set values back to default
		closeAttackDamage = defaultCloseAttackDamage;
		damageRadius = defaultDamageRadius;
	}

	// Checks if there are enemies in the attack area and, if so, damages them
	void CheckForEnemies() {
		Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
		foreach(Collider c in colliders) {
			// Checks if the collider is an enemy
			if(c.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyMaynardMovement>()) {
				c.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyMaynardMovement>().TakeDamage(closeAttackDamage);
			}
			else {
				if(c.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyDrakeMovement>()) {
					c.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyDrakeMovement>().TakeDamage(closeAttackDamage);
				}
				else {
					if(c.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyIncognitoMovement>()) {
						c.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyIncognitoMovement>().TakeDamage(closeAttackDamage);
					}
				}
			}
		}
	}

	async void SpawnMagneticShield() {
		// Without this check, if the button for activating/deactivating the shield is pushed and released more than once in a very fast way, then
		// the function is called multiple times, creating a race condition among multiple concurrent instances of it (buggy code)
		if (isShieldCoroutineRunning)
		{
			return;
		}
		
		isShieldCoroutineRunning = true;
		
		if(!magneticShieldOpen && !shieldActivated) {
			if(powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.DefensePowerUp)) {
				// Audio management
				AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldActivation, rotatingSphere.transform.position);
				shieldActivated = true;
				
				if(powerUp.powerUpsObtained[PowerUp.PowerUpType.DefensePowerUp] == 1) {
					// Spawn level 1 shield
				}
				else {
					magneticShield = Instantiate(magneticShieldPrefab, new Vector3(player.transform.position.x, player.transform.position.y - 1f, player.transform.position.z), Quaternion.identity);
					magneticShield.transform.parent = transform;
					magneticShieldOpen = true;
				}
			}
		}
		else {
			if(magneticShield != null && shieldActivated) {
				// Audio management
				AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldDeactivation, rotatingSphere.transform.position);
				shieldActivated = false;
				await Task.Delay(500);
				
				Destroy(magneticShield);
			}
			magneticShieldOpen = false;
		}
		
		isShieldCoroutineRunning = false;
	}

	public void TakeDamage(int damage) {
		health -= damage;
		healthBar.SetHealth(health);

		StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));

		if(health <= 0)
			Invoke(nameof(DestroyPlayer), 0.05f);
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
		// The attack is shot only on "Fire1" up
		if (Input.GetButtonDown("Fire1")) {
			
			// Audio management
			loadingAttack = true;
			
			if(!magneticShield && CheckStamina(1)) {
				switch(attackNumber) {
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
		
		if (Input.GetButtonUp("Fire1")) {
			if(!magneticShield && CheckStamina(1)) {
				switch(attackNumber) {
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

		if (Input.GetButtonDown("Fire2") && !loadingAttack) {
			SpawnMagneticShield();
		}

		// Selecting a different attack
		if(Input.GetAxis("Mouse ScrollWheel") > 0 && !loadingAttack) {
			ChangeAttack(1);
		}
		else if(Input.GetAxis("Mouse ScrollWheel") < 0 && !loadingAttack) {
			ChangeAttack(-1);
		}
		if(Input.GetKeyDown(KeyCode.Alpha1) && !loadingAttack) {
			SetAttack(1);
		}
		if(Input.GetKeyDown(KeyCode.Alpha2) && !loadingAttack) {
			SetAttack(2);
		}

		// I check the stamina every frame since it is possible that it is = 0 when I am not attacking (thanks asyncronous processes)
		// Not that good, but I don't have better ways to manage it
		if(sphereStamina <= 0) {
			if(!increasingStamina) {
				increaseStamina = true;
				RecoverStamina();
			}
		}
	}
	
	// Audio management
	private void UpdateSound() 
	{
		distanceAttackLoading.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphere.transform));
		closeAttackLoading.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotateSphere.transform));
	
		// Start distance attack loading event if the player is using distance attack
		if (attackNumber == 1)
		{
			if(loadingAttack && powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.DistanceAttackPowerUp))
			{
				// Get the playback state for the distance attack loading event 
				PLAYBACK_STATE distanceAttackPlaybackState;
				distanceAttackLoading.getPlaybackState(out distanceAttackPlaybackState);
				if(distanceAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED)) 
				{
					distanceAttackLoading.start();
				}
			}
			// Otherwise, stop the distance attack loading event 
			else
			{
				distanceAttackLoading.stop(STOP_MODE.ALLOWFADEOUT);
			}
		}

		else
		{
			if(loadingAttack && powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.CloseAttackPowerUp))
			{
				// Get the playback state for the close attack loading event 
				PLAYBACK_STATE closeAttackPlaybackState;
				closeAttackLoading.getPlaybackState(out closeAttackPlaybackState);
				if(closeAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED)) 
				{
					closeAttackLoading.start();
				}
			}
			// Otherwise, stop the close attack loading event 
			else
			{
				closeAttackLoading.stop(STOP_MODE.ALLOWFADEOUT);
			}
		}
	}
}