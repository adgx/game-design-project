using UnityEngine;

public class GetCollisions : MonoBehaviour
{
	// This function checks if the projectile shot by the player or by the enemy collides with somethinga nd, if so, it destroys the projectilewawadsa
	void OnCollisionStay(Collision collision) {
		foreach(ContactPoint contact in collision.contacts) {
			if((contact.thisCollider.tag == "EnemyProjectile" && contact.otherCollider.tag != "EnemyTag") || (contact.thisCollider.tag == "PlayerProjectile" && contact.otherCollider.tag != "PlayerTag")) {
				Destroy(contact.thisCollider.gameObject);
			}
		}
	}
}
