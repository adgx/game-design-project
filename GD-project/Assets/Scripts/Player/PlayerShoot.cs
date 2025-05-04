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

    void Shoot() {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
        bullet.tag = "PlayerProjectile";

		Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
		rbBullet.AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);
		rbBullet.AddForce(bulletSpawnTransform.up * 1f, ForceMode.Impulse);
	}

    void SpawnMagneticSphere() {
        // TODO: La sfera magnetica deve essere disegnata e, quando lo si fa, bisogna aggiungere i collider in modo che possa inserire il player all'interno senza che questo venga buttato fuori
		if(!magneticSphereOpen) {
            magneticSphere = Instantiate(magneticSpherePrefab, transform.position, Quaternion.identity);
            magneticSphere.transform.parent = transform;
			magneticSphereOpen = true;
        }
        else {
            if(magneticSphere != null) {
                Destroy(magneticSphere);
            }
            magneticSphereOpen = false;
        }
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