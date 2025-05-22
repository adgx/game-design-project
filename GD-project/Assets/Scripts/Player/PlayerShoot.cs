using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
	[SerializeField] private float bulletSpeed;
	[SerializeField] private float bulletDamage;

	[SerializeField] private Transform bulletSpawnTransform;
	[SerializeField] private GameObject bulletPrefab;

	[SerializeField] private GameObject magneticShieldPrefab;
    GameObject magneticShield;
	private bool magneticShieldOpen = false;

	[SerializeField] private RotateSphere rotateSphere;

    public float health;

	[SerializeField] private HealthBar healthBar;

	private void Start() {
		healthBar.SetMaxHealth(health);
	}

	void Shoot() {
		rotateSphere.positionSphere(transform.position + transform.forward * 1f);

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
        bullet.tag = "PlayerProjectile";

		Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
		rbBullet.AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);
		rbBullet.AddForce(bulletSpawnTransform.up * 2f, ForceMode.Impulse);
	}

    void SpawnMagneticSphere() {
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
            if(!magneticShield) {
                Shoot();
            }
        }

        if (Input.GetButtonDown("Fire2")) {
            SpawnMagneticSphere();
        }
    }
}