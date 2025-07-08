using System;
using UnityEngine;

public class GetCollisions : MonoBehaviour
{
    public float initialPlayerBulletDamage = 40, enemyBulletDamage = 20;
    public float playerBulletDamage;

    // This function checks if the projectile shot by the player or by the enemy collides with something and, if so, it destroys the projectile
    void OnCollisionStay(Collision collision) {
        foreach(ContactPoint contact in collision.contacts) {
			if((contact.thisCollider.tag.Contains("EnemyAttack") && !contact.otherCollider.tag.Contains("Enemy")) || (contact.thisCollider.CompareTag("PlayerProjectile") && !contact.otherCollider.CompareTag("Player"))) {
				Destroy(contact.thisCollider.gameObject);

                if(contact.thisCollider.CompareTag("PlayerProjectile") && contact.otherCollider.tag.Contains("Enemy") && !contact.otherCollider.tag.Contains("EnemyAttack")) {
                    contact.otherCollider.GetComponent<Enemy.EnemyManager.IEnemy>().TakeDamage(playerBulletDamage, "d");
                }
                else {
                    if(contact.thisCollider.tag.Contains("EnemyAttack") && contact.otherCollider.CompareTag("Player")) {
                        PlayerShoot playerShoot = contact.otherCollider.GetComponent<PlayerShoot>();

                        if (contact.thisCollider.tag.Contains("Spit"))
                        {
							playerShoot.TakeDamage(enemyBulletDamage, PlayerShoot.DamageTypes.Spit, Math.Sign(contact.normal.x), Math.Sign(contact.normal.z));
						}
                        else
                        {
							playerShoot.TakeDamage(enemyBulletDamage, PlayerShoot.DamageTypes.MaynardDistanceAttack, Math.Sign(contact.normal.x), Math.Sign(contact.normal.z));
						}
                    }
                }
            }
        }
    }
}