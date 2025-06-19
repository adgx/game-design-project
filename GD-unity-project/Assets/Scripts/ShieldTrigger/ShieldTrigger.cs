using System;
using UnityEngine;

public class ShieldTrigger : MonoBehaviour
{
    // Audio management
    private GameObject player;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyProjectile")) {
            player = GameObject.Find("Player");
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.shieldHit, player.transform.position);
            Destroy(other.gameObject);
        }
    }
}