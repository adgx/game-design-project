using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public float bulletSpeed;
    public float bulletDamage;
    
    public Transform bulletSpawnTransform;
    public GameObject bulletPrefab;

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        bullet.GetComponent<Rigidbody2D>().AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode2D.Impulse);
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }   
    }
}