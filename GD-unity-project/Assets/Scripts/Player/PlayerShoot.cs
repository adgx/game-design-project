using System;
using System.Collections;
using System.Threading.Tasks;
using Animations;
using Audio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Helper;
using Utils;

public class PlayerShoot : MonoBehaviour
{
	public static PlayerShoot Instance { get; private set; }

	// Audio management 
	public bool IsSphereRotating => rotateSphere.isRotating;
	private bool isShieldCoroutineRunning;
	[SerializeField] private RickEvents rickEvents; 
	
	// Attack1
	[SerializeField] private float bulletSpeed;
	// The point in which the bullet spawns
	[SerializeField] private Transform bulletSpawnTransform;
	[SerializeField] private GameObject bulletPrefab;

	// Attack2
	[SerializeField] private GameObject attackAreaPrefab;
	public int defaultCloseAttackDamage = 50;
	public int closeAttackDamage = 50;
	private float defaultDamageRadius = 2.5f;
	private float damageRadius = 2.5f;

	public bool cannotAttack = false;

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
	public bool magneticShieldOpen = false;
	
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
	[SerializeField] private Material sphereMaterial;

	[SerializeField] private string respawnSceneName = "RespawnScene";
	[SerializeField] private GameTimer gameTimer;

	public enum DamageTypes {
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
            return;
        }
    }

	private void Start() {
		healthBar.SetMaxHealth(health);
		player = GetComponent<Player>();

		ChangeSphereColor(maxSphereStamina);
	}
	
	void ChangeSphereColor(int stamina) {
		switch(stamina) {
			case 5:
				sphereMaterial.color = Color.green;
				sphereMaterial.SetColor("_EmissionColor", Color.green);
				break;
			case 4:
				sphereMaterial.color = new Color(0, 1, 1);
				sphereMaterial.SetColor("_EmissionColor", new Color(0, 1, 1));
				break;
			case 3:
				sphereMaterial.color = Color.yellow;
				sphereMaterial.SetColor("_EmissionColor", Color.yellow);
				break;
			case 2:
				sphereMaterial.color = new Color(1, 0.5f, 0);
				sphereMaterial.SetColor("_EmissionColor", new Color(1, 0.1875f, 0));
				break;
			case 1:
				sphereMaterial.color = Color.red;
				sphereMaterial.SetColor("_EmissionColor", Color.red);
				break;
			case 0:
				sphereMaterial.color = Color.red;
				sphereMaterial.SetColor("_EmissionColor", Color.red);
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
		
		// Audio management: the Sphere has finished the stamina
		GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerSphereDischarge, rotatingSphere.transform.position);
		return false;
	}

	public void DecreaseStamina(int amount) {
		sphereStamina -= amount;
		increaseStamina = false;
		ChangeSphereColor(sphereStamina);
	}

	public async Task RecoverStamina() {
		increasingStamina = true;
		while(sphereStamina < maxSphereStamina && increaseStamina && !loadingAttack) {
			await Task.Delay(500);
			if(increaseStamina && !loadingAttack) {
				sphereStamina += 1;
				ChangeSphereColor(sphereStamina);

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
		rotateSphere.positionSphere(new Vector3(0, 0.8f, rotateSphere.DistanceFromPlayer), RotateSphere.Animation.RotateAround);
		//AnimationManager.Instance.Idle();
		AnimationManager.Instance.Attack();
		
		await Task.Delay(50);
		
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

			if (attackStamina > 1) {
				getCollisions.playerBulletDamage += 10;
			}
			
			await Task.Delay(500);
		}
		
		// Audio management: stop the loading sound of the attack when the loading is terminated 
		if (rickEvents != null)
		{
			rickEvents.ShouldPlayChargeSound = false;
		}
	}
	
	private void DistanceAttackAnimation() {
		loadingAttack = false;
		
		// Audio management: stop the loading sound of the attack if the button is released
		if (rickEvents != null)
		{
			rickEvents.ShouldPlayChargeSound = false;
		}

		print("Ciao");
		
		AnimationManager.Instance.EndAttack();
	}

	public async void FireDistanceAttack() {
		bulletPrefab.gameObject.SetActive(false);
		GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
		bullet.tag = "PlayerProjectile";
		ParticleAttackController PAC = bullet.GetComponent<ParticleAttackController>();
		PAC.playerBulletDamage = PAC.initialPlayerBulletDamage;
		bullet.SetActive(true);
		bulletPrefab.gameObject.SetActive(true);
		/*
		Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
		rbBullet.AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);
		rbBullet.AddForce(bulletSpawnTransform.up * 2f, ForceMode.Impulse);
		
		getCollisions.playerBulletDamage = getCollisions.initialPlayerBulletDamage;
		*/
		
		if (attackStamina == 0)
		{
			DecreaseStamina(1);
		}
		else
		{
			DecreaseStamina(attackStamina);
		}

		distanceAttackLoadingBar.fillAmount = 0;

		await Task.Delay(500);
		ResetAttack();
	}
	
	async void LoadCloseAttack() {
		// If we are here the stamina is at least 1
		loadingAttack = true;
		rotateSphere.positionSphere(new Vector3(0, 1.8f, 0), RotateSphere.Animation.Linear);
		player.isFrozen = true;
		AnimationManager.Instance.AreaAttack();
		
		await Task.Delay(50);
		
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
		
		// Audio management: if after the delay we are still charging, start the sound
		if (loadingAttack && rickEvents != null)
		{
			rickEvents.ShouldPlayChargeSound = true;
		}
		
		while (attackStamina < maxStamina && powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp) && loadingAttack) {
			attackStamina++;
			ChangeSphereColor(attackStamina);

			closeAttackLoadingBar.fillAmount = (float)attackStamina / maxSphereStamina;

			if (attackStamina > 1) {
				damageRadius += 1f;
				closeAttackDamage += 20;
			}
			
			await Task.Delay(500);
		}
		
		// Audio management: stop the loading sound of the attack when the loading is terminated 
		if (rickEvents != null)
		{
			rickEvents.ShouldPlayChargeSound = false;
		}
	}
	
	private void CloseAttackAnimation() {
		loadingAttack = false;
		
		// Audio management: stop the loading sound of the attack if the button is released
		if (rickEvents != null)
		{
			rickEvents.ShouldPlayChargeSound = false;
		}
		
		AnimationManager.Instance.EndAreaAttack();
	}

	public void FireCloseAttack() {
		if(attackStamina == 0) {
			DecreaseStamina(1);
		}
		else {
			DecreaseStamina(attackStamina);
		}

		closeAttackLoadingBar.fillAmount = 0;

		SpawnAttackArea();
	}

	async void SpawnAttackArea() {
		
		GameObject attackArea = Instantiate(attackAreaPrefab, transform.position, Quaternion.identity);
		attackArea.transform.parent = transform;
		attackArea.transform.localScale = new Vector3(2 * damageRadius, 0, 2 * damageRadius);
		
		CheckForEnemies();

		await Task.Delay(500);

		Destroy(attackArea);

		rotateSphere.positionSphere(new Vector3(rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);
		await Task.Delay(300);

		// Set values back to default
		closeAttackDamage = defaultCloseAttackDamage;
		damageRadius = defaultDamageRadius;
		
		ResetAttack();
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

	public void ResetAttack() {
		loadingAttack = false;
		attacking = false;
		rotateSphere.isRotating = true;
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

	private void SpawnMagneticShield() {
		// Without this check, if the button for activating/deactivating the shield is pushed and released more than once in a very fast way, then
		// the function is called multiple times, creating a race condition among multiple concurrent instances of it (buggy code)
		if (isShieldCoroutineRunning)
		{
			return;
		}
		
		isShieldCoroutineRunning = true;
		
		if(!magneticShieldOpen) 
		{ 
			// to modify for the instantiate the vfx and lunch the animation character
			//luch defense animation
			AnimationManager.Instance.Defense();
			magneticShieldOpen = true;
			player.isFrozen = true;
		}
		
		isShieldCoroutineRunning = false;
	}

	public void CloseShield() {
		player.isFrozen = false;
		magneticShieldOpen = false;
	}

	public void TakeDamage(float damage, DamageTypes damageType, int x, int z) {
     	health -= damage * damageReduction;
     	healthBar.SetHealth(health);
     
     	StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));

		if(health > 0) {
			HitAnimation(damageType, 0, 1);
		}
		else
     	{
	        DisableAttacks(true);
	        player.FreezeMovement(true);
			gameTimer.isRunning = false;
			if(damageType == DamageTypes.DrakeBiteAttack) {
				AnimationManager.Instance.Bite();
			}
			else {
				DeathAnimation(1, 1);
			}
     	}
	}

	public void DeathAnimation(int x, int z) {
		AnimationManager.Instance.Death(x, z);
	}

	public void SetLayerToZero() {
		gameObject.layer = 0;
	}

	public void LoadRespawnScene() {
		Invoke(nameof(DestroyPlayer), 1f);

		FadeManager.Instance.FadeOutIn(() => {
			StartCoroutine(LoadRespawnSceneAsync());
		});
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
	private void HitAnimation(DamageTypes damageType, int x, int z) {
		if(!cannotAttack && !player.isFrozen) {
			switch(damageType) {
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
	public void FreePlayer() {
		DisableAttacks(false);
		player.FreezeMovement(false);
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
				//  && AnimationManager.Instance.rickState == RickStates.Idle
				if(Input.GetButtonDown("Fire1"))
				{
					if (!magneticShield && CheckStamina(1) && !attacking)
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
					if (!magneticShield && CheckStamina(1) && loadingAttack)
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
				if(Input.GetButtonDown("Fire2") && !loadingAttack)
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
}