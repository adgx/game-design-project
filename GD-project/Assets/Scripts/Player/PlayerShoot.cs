using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public float bulletSpeed;
    public float bulletDamage;
    
    public Transform bulletSpawnTransform;
    public GameObject bulletPrefab;

    void Shoot()
    {
        Rigidbody rbBullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity).GetComponent<Rigidbody>();
		rbBullet.AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);
		rbBullet.AddForce(bulletSpawnTransform.up * 2f, ForceMode.Impulse);
	}

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }   
    }
}