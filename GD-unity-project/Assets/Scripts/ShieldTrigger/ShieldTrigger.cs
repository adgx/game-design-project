using System;
using Audio;
using UnityEngine;

public class ShieldTrigger : MonoBehaviour
{
    // Audio management
    private GameObject player;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("EnemyAttack")) {
            player = GameObject.Find("Player");
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldHit, player.transform.position);
            Destroy(other.gameObject);
        }
    }
}