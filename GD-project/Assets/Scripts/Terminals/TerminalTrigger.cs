using System;
using System.ComponentModel;
using UnityEngine;

public class TerminalTrigger : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    
    private enum TriggerType {
        None,
        SphereTerminal,
        PlayerTerminal,
        SnackDistributor
    }
    
    private bool playerInTrigger = false;
    private TriggerType triggerType;
    
    private void OnTriggerEnter(Collider other) {
        if (transform.CompareTag(TriggerType.SphereTerminal.ToString())) {
            triggerType = TriggerType.SphereTerminal;
        }
        else {
            if (transform.CompareTag(TriggerType.PlayerTerminal.ToString())) {
                triggerType = TriggerType.PlayerTerminal;
            }
            else {
                if (transform.CompareTag(TriggerType.SnackDistributor.ToString())) {
                    triggerType = TriggerType.SnackDistributor;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        triggerType = TriggerType.None;
    }

    private void Update() {
        if (triggerType != TriggerType.None && Input.GetKeyDown(KeyCode.E)) {
            switch (triggerType) {
                case TriggerType.SphereTerminal:
                    // Give a random power up for the Sphere
                    break;
                case TriggerType.PlayerTerminal:
                    // Give a random power up for the Player
                    break;
                case TriggerType.SnackDistributor:
                    // Recover health, lose 1 stamina for the Sphere
                    playerShoot.RecoverHealth(playerShoot.maxHealth);
                    playerShoot.DecreaseStamina(1);

                    break;
                default:
                    break;
            }
        }
    }
}
