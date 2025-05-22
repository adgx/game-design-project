using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public float bulletSpeed;
    public float bulletDamage;
    
    public Transform bulletSpawnTransform;
    public GameObject bulletPrefab;

    public GameObject magneticSpherePrefab;
    GameObject magneticSphere;
	private bool magneticSphereOpen = false;

    public float health;

	public HealthBar healthBar;

	private void Start() {
		healthBar.SetMaxHealth(health);
	}

	void Shoot() {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
        bullet.tag = "PlayerProjectile";

		Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
		rbBullet.AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);
		rbBullet.AddForce(bulletSpawnTransform.up * 2f, ForceMode.Impulse);
	}

    void SpawnMagneticSphere() {
		if(!magneticSphereOpen) {
            magneticSphere = Instantiate(magneticSpherePrefab, transform.position, Quaternion.identity);
            magneticSphere.transform.parent = transform;
            //magneticSphere.transform.Rotate(new Vector3(0, 0, 90));
			magneticSphereOpen = true;
        }
        else {
            if(magneticSphere != null) {
                Destroy(magneticSphere);
            }
            magneticSphereOpen = false;
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
            if(!magneticSphere) {
                Shoot();
            }
        }

        if (Input.GetButtonDown("Fire2")) {
            SpawnMagneticSphere();
        }
    }
}