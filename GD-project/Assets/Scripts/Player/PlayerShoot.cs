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
    public float health;
	[SerializeField] private HealthBar healthBar;

	// Stamina for the attacks
	private int maxSphereStamina = 5;
	private int sphereStamina = 5;

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

	async void RecoverStamina() {
		while(sphereStamina < maxSphereStamina) {
			await Task.Delay(400);
			sphereStamina += 1;
		}
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

	void DistanceAttack1() {
		rotateSphere.positionSphere(transform.position + transform.forward * 1f);

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
        bullet.tag = "PlayerProjectile";

		Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
		rbBullet.AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);
		rbBullet.AddForce(bulletSpawnTransform.up * 2f, ForceMode.Impulse);

		sphereStamina--;
		if(sphereStamina <= 0) {
			RecoverStamina();
		}
	}

	void CloseAttack1() {
		SpawnAttackArea();
		sphereStamina--;
		if(sphereStamina <= 0) {
			RecoverStamina();
		}

		CheckForEnemies();
	}

	async void CloseAttack2() {
		int attackStamina = 0;
		while(Input.GetButton("Fire1") && attackStamina < sphereStamina) {
			damageRadius += 1f;
			closeAttackDamage += 20;
			attackStamina++;
			Debug.Log(attackStamina);
			await Task.Delay(500);
		}
		sphereStamina -= attackStamina;
		if(sphereStamina <= 0) {
			RecoverStamina();
		}

		SpawnAttackArea();

		CheckForEnemies();
	}

	async void SpawnAttackArea() {
		GameObject attackArea = Instantiate(attackAreaPrefab, transform.position, Quaternion.identity);
		attackArea.transform.localScale = new Vector3(2 * damageRadius, 0, 2 * damageRadius);
		player.isFrozen = true;

		await Task.Delay(500);

		Destroy(attackArea);
		player.isFrozen = false;

		// Set values back to default
		closeAttackDamage = defaultCloseAttackDamage;
		damageRadius = defaultDamageRadius;
	}

	// Checks if there are enemies in the attack area and, if so, damages them
	void CheckForEnemies() {
		Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
		foreach(Collider c in colliders) {
			// Checks if the collider is an enemy
			if(c.GetComponent<EnemyMovement>()) {
				c.GetComponent<EnemyMovement>().TakeDamage(closeAttackDamage);
			}
		}
	}

    void SpawnMagneticShield() {
		if(!magneticShieldOpen) {
            magneticShield = Instantiate(magneticShieldPrefab, transform.position, Quaternion.identity);
			magneticShield.transform.parent = transform;
			magneticShieldOpen = true;
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
		for(float t = 0; t < 1.0f; t += Time.deltaTime / duration) {
			renderer.material.color = Color.Lerp(dmgColor, originColor, t);

			yield return null;
		}

		renderer.material.color = originColor;
	}
	private void DestroyPlayer() {
		Destroy(gameObject);
	}

	void Update() {
        if (Input.GetButtonDown("Fire1")) {
            if(!magneticShield && sphereStamina > 0) {
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

	}
}