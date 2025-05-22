using UnityEngine;

public class GetCollisions : MonoBehaviour
{
	[SerializeField] int playerBulletDamage = 30, enemyBulletDamage = 20;

	// This function checks if the projectile shot by the player or by the enemy collides with somethinga nd, if so, it destroys the projectilewawadsa
	void OnCollisionStay(Collision collision) {
		foreach(ContactPoint contact in collision.contacts) {
			if((contact.thisCollider.tag == "EnemyProjectile" && contact.otherCollider.tag != "Enemy") || (contact.thisCollider.tag == "PlayerProjectile" && contact.otherCollider.tag != "Player")) {
				Destroy(contact.thisCollider.gameObject);

				if(contact.thisCollider.tag == "PlayerProjectile" && contact.otherCollider.tag == "Enemy") {
					EnemyMovement enemyMovement = contact.otherCollider.GetComponent<EnemyMovement>();
					enemyMovement.TakeDamage(playerBulletDamage);
				}
				else {
					if(contact.thisCollider.tag == "EnemyProjectile" && contact.otherCollider.tag == "Player") {
						PlayerShoot playerShoot = contact.otherCollider.GetComponent<PlayerShoot>();
						playerShoot.TakeDamage(enemyBulletDamage);
					}
				}
			}
		}
	}
}
