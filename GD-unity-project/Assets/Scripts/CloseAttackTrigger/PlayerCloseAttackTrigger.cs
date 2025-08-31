using UnityEngine;
using Audio;

public class PlayerCloseAttackTrigger : MonoBehaviour
{ 
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object he collided with is an enemy projectile
        if (other.tag.Contains("EnemyAttack"))
        {
            // Optional: Play a sound effect for deflecting/destroying enemy projectile
            // GamePlayAudioManager.instance.PlayManagedOneShot(FMODEvents.Instance.PlayerShieldHit, transform.position);
            Destroy(other.gameObject); // Destroy enemy's projectile
        }
    }
}   