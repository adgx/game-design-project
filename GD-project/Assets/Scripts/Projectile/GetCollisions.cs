using UnityEngine;

public class GetCollisions : MonoBehaviour
{
	// Audio management
	private GameObject player;
	
	public int initialPlayerBulletDamage = 30, enemyBulletDamage = 20;
	public int playerBulletDamage;

	// This function checks if the projectile shot by the player or by the enemy collides with something and, if so, it destroys the projectile
	void OnCollisionStay(Collision collision) {
		foreach(ContactPoint contact in collision.contacts) {
			// Audio management: collision detection with shield
			if (contact.thisCollider.tag == "EnemyProjectile" && contact.otherCollider.CompareTag("Shield")) {
				player = GameObject.Find("Player");
				Debug.Log("The projectile has hit the shield");
				AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldHit, player.transform.position);
			}
			
			if((contact.thisCollider.tag == "EnemyProjectile" && !contact.otherCollider.tag.Contains("Enemy")) || (contact.thisCollider.tag == "PlayerProjectile" && contact.otherCollider.tag != "Player")) {
				Destroy(contact.thisCollider.gameObject);

				if(contact.thisCollider.tag == "PlayerProjectile" && contact.otherCollider.tag.Contains("Enemy")) {
					if(contact.otherCollider.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyMaynardMovement>()) {
						contact.otherCollider.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyMaynardMovement>().TakeDamage(playerBulletDamage);
					}
					else {
						if(contact.otherCollider.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyDrakeMovement>()) {
							contact.otherCollider.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyDrakeMovement>().TakeDamage(playerBulletDamage);
						}
						else {
							if(contact.otherCollider.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyIncognitoMovement>()) {
								contact.otherCollider.GetComponent<Enemy.EnemyData.EnemyMovement.EnemyIncognitoMovement>().TakeDamage(playerBulletDamage);
							}
						}
					}
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
