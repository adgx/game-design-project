using System;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

// NOTE: this script is attached to each terminal individually
public class TerminalTrigger : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private PowerUp powerUps;

    // You first hack the machine and then you press the button again to receive the snack.
    private bool machineHacked = false;
	// This variable is true while I'm hacking the machine or picking up a snack, so that I can not interact with the machine again
	private bool busy = false;
    
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

    async private void ManageVendingMachine() {
		if(!busy) {
			switch(triggerType) {
				case TriggerType.SphereTerminal:
					// Give a random power up for the Sphere
					if(powerUps.spherePowerUps.Count > 0) {
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.terminalInteraction, this.transform.position);

						busy = true;
						await Task.Delay(2000);

						// Generate a random power up
						int powerUpIndexSphere = rnd.Next(powerUps.spherePowerUps.Count);
						Debug.Log(powerUps.spherePowerUps.Count);

						// Insert the power up in the dictionary of the obtained ones
						powerUps.ObtainPowerUp(powerUps.spherePowerUps[powerUpIndexSphere]);

						// Remove the power up from the list of power ups
						powerUps.spherePowerUps.RemoveAt(powerUpIndexSphere);

						busy = false;
					}

					break;
				case TriggerType.PlayerTerminal:
					// Give a random power up for the Player
					if(powerUps.playerPowerUps.Count > 0) {
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.terminalInteraction, this.transform.position);

						busy = true;
						await Task.Delay(2000);

						// Generate a random power up
						int powerUpIndexPlayer = rnd.Next(powerUps.playerPowerUps.Count);
						Debug.Log(powerUps.playerPowerUps.Count);

						// Insert the power up in the dictionary of the obtained ones
						powerUps.ObtainPowerUp(powerUps.playerPowerUps[powerUpIndexPlayer]);

						// Remove the power up from the list of power ups
						powerUps.playerPowerUps.RemoveAt(powerUpIndexPlayer);

						busy = false;
					}

					break;
				case TriggerType.SnackDistributor:

					if(machineHacked) {
						// Recover health, lose 1 stamina for the Sphere
						Debug.Log("Recovering health");
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.vendingMachineItemPickUp, this.transform.position);

						busy = true;
						await Task.Delay(2000);
						playerShoot.RecoverHealth(playerShoot.maxHealth);
						playerShoot.DecreaseStamina(1);
						busy = false;
					}
					else {
						Debug.Log("Hacking the machine");
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.vendingMachineActivation, this.transform.position);

						busy = true;
						await Task.Delay(700);
						busy = false;
						machineHacked = true;
					}

					break;
				default:
					break;
			}
		}
	}


	private void Update() {
        if (triggerType != TriggerType.None && Input.GetKeyDown(KeyCode.E)) {
            ManageVendingMachine();
        }
    }
}
