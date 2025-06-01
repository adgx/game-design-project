using System;
using System.ComponentModel;
using UnityEngine;

public class TerminalTrigger : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private PowerUp powerUps;
    
    private enum TriggerType {
        None,
        SphereTerminal,
        PlayerTerminal,
        SnackDistributor
    }
    
    private TriggerType triggerType;

    static System.Random rnd = new System.Random();
    
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

                    if(powerUps.spherePowerUps.Count > 0) {
                        // Generate a random power up
                        int powerUpIndexSphere = rnd.Next(powerUps.spherePowerUps.Count);
						Debug.Log(powerUps.spherePowerUps.Count);

						// Insert the power up in the dictionary of the obtained ones
						powerUps.ObtainPowerUp(powerUps.spherePowerUps[powerUpIndexSphere]);

                        // Remove the power up from the list of power ups
                        powerUps.spherePowerUps.RemoveAt(powerUpIndexSphere);
                    }

                    break;
                case TriggerType.PlayerTerminal:
                    // Give a random power up for the Player

                    if(powerUps.playerPowerUps.Count > 0) {
                        // Generate a random power up
                        int powerUpIndexPlayer = rnd.Next(powerUps.playerPowerUps.Count);
						Debug.Log(powerUps.playerPowerUps.Count);

						// Insert the power up in the dictionary of the obtained ones
						powerUps.ObtainPowerUp(powerUps.playerPowerUps[powerUpIndexPlayer]);

                        // Remove the power up from the list of power ups
                        powerUps.playerPowerUps.RemoveAt(powerUpIndexPlayer);
                    }

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
