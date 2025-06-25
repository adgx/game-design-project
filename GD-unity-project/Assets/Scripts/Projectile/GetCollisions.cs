using UnityEngine;

public class GetCollisions : MonoBehaviour
{
    // Audio management
    private GameObject player;
	
    public int initialPlayerBulletDamage = 30, enemyBulletDamage = 20;
    public int playerBulletDamage;

    void Start()
    {
        player = GameObject.Find("Player");
    }

    // This function checks if the projectile shot by the player or by the enemy collides with something and, if so, it destroys the projectile
    void OnCollisionStay(Collision collision) {
        foreach(ContactPoint contact in collision.contacts) {
            if((contact.thisCollider.CompareTag("EnemyProjectile") && !contact.otherCollider.tag.Contains("Enemy")) || (contact.thisCollider.CompareTag("PlayerProjectile") && !contact.otherCollider.CompareTag("Player"))) {
                Destroy(contact.thisCollider.gameObject);

                if(contact.thisCollider.CompareTag("PlayerProjectile") && contact.otherCollider.tag.Contains("Enemy")) {
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