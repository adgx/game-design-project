using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
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

	// The player has 3 attacks they can choose. They can change them by using the mouse scroll wheel
	private int attackNumber = 1;
	[SerializeField] TextMeshProUGUI selectedAttackText;


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

	private bool CheckStamina(int attackStamina) {
		if (sphereStamina >= attackStamina) {
			return true;
		}
		else { // The Sphere has finished the stamina
			// Audio management
			AudioManager.instance.PlayOneShot(FMODEvents.instance.sphereDischarge, rotatingSphere.transform.position);
			return false;
		}
			
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
		if(attackNumber > 3) {
			attackNumber = 1;
		}
		else if(attackNumber < 1) {
			attackNumber = 3;
		}

		selectedAttackText.text = "Attack " + attackNumber.ToString();
	}

	void SetAttack(int n) {
		attackNumber = n;
		selectedAttackText.text = "Attack " + attackNumber.ToString();
	}

	async void DistanceAttack1() {
		// If we are here the stamina is at least 1
		rotateSphere.positionSphere(new Vector3(0, 0, 1), RotateSphere.Animation.RotateAround);

		int attackStamina = 0;
		int maxStamina = 0;
		if (powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.AttackPowerUp))
		{
			if (powerUp.powerUpsObtained[PowerUp.PowerUpType.AttackPowerUp] == 1)
			{
				maxStamina = Math.Min(sphereStamina, 3);
			}
			else
			{
				if (powerUp.powerUpsObtained[PowerUp.PowerUpType.AttackPowerUp] == 2)
				{
					maxStamina = Math.Min(sphereStamina, 5);
				}
			}
		}

		while (Input.GetButton("Fire1") && attackStamina < maxStamina && powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.AttackPowerUp))
		{
			attackStamina++;
			getCollisions.playerBulletDamage += 10;
				
			Debug.Log(attackStamina);
			if (attackStamina > 1)
				await Task.Delay(500);
			else
				await Task.Delay(200);
		}

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
	}

	void CloseAttack1() {
		// If we are here the stamina is at least 1
		SpawnAttackArea();

		CheckForEnemies();

		DecreaseStamina(1);
	}

	async void CloseAttack2() {
		// If we are here the stamina is at least 1
		int attackStamina = 0;
		while (Input.GetButton("Fire1") && attackStamina < sphereStamina) {
			damageRadius += 1f;
			closeAttackDamage += 20;
			attackStamina++;
			await Task.Delay(500);
		}

		DecreaseStamina(attackStamina);

		SpawnAttackArea();

		CheckForEnemies();
	}

	async void SpawnAttackArea() {
		rotateSphere.positionSphere(new Vector3(0, 1, 0), RotateSphere.Animation.Linear);
		
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
			if(c.GetComponent<EnemyMaynardMovement>()) {
				c.GetComponent<EnemyMaynardMovement>().TakeDamage(closeAttackDamage);
			}
			else {
				if(c.GetComponent<EnemyDrakeMovement>()) {
					c.GetComponent<EnemyDrakeMovement>().TakeDamage(closeAttackDamage);
				}
				else {
					if(c.GetComponent<EnemyIncognitoMovement>()) {
						c.GetComponent<EnemyIncognitoMovement>().TakeDamage(closeAttackDamage);
					}
				}
			}
		}
	}

	async void SpawnMagneticShield() {
		if(!magneticShieldOpen) {
			if(powerUp.powerUpsObtained.ContainsKey(PowerUp.PowerUpType.DefensePowerUp)) {
				if(powerUp.powerUpsObtained[PowerUp.PowerUpType.DefensePowerUp] == 1) {
					// Spawn level 1 shield
				}
				else {
					if(powerUp.powerUpsObtained[PowerUp.PowerUpType.DefensePowerUp] == 2) {
						// Spawn level 2 shield
					}
					else {
						magneticShield = Instantiate(magneticShieldPrefab, transform.position, Quaternion.identity);
						magneticShield.transform.parent = transform;
						magneticShieldOpen = true;
					}
				}
			}
		}
		else {
			if(magneticShield != null) {
				Destroy(magneticShield);
			}
			magneticShieldOpen = false;
		}
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
		if (Input.GetButtonDown("Fire1")) {
			if(!magneticShield && CheckStamina(1)) {
				switch(attackNumber) {
					case 1:
						DistanceAttack1();
						break;
					case 2:
						CloseAttack1();
						break;
					case 3:
						CloseAttack2();
						break;
					default:
						break;
				}
			}
		}

		if (Input.GetButtonDown("Fire2")) {
			SpawnMagneticShield();
		}

		// Selecting a different attack
		if(Input.GetAxis("Mouse ScrollWheel") > 0) {
			ChangeAttack(1);
		}
		else if(Input.GetAxis("Mouse ScrollWheel") < 0) {
			ChangeAttack(-1);
		}
		if(Input.GetKeyDown(KeyCode.Alpha1)) {
			SetAttack(1);
		}
		if(Input.GetKeyDown(KeyCode.Alpha2)) {
			SetAttack(2);
		}
		if(Input.GetKeyDown(KeyCode.Alpha3)) {
			SetAttack(3);
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
}