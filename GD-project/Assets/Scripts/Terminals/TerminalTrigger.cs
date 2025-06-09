using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using FMODUnity;
using UnityEngine;

// NOTE: this script is attached to each terminal individually
public class TerminalTrigger : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private PowerUp powerUps;

    // You first hack the machine and then you press the button again to receive the snack.
    private bool healthVendingMachineHacked = false;
    // You first hack the machine and then you press the button again to receive the power up (snack/energy drink).
    private bool powerUpVendingMachineHacked = false;
	// This variable is true while I'm hacking the machine or picking up a snack, so that I can not interact with the machine again
	private bool busy = false;
    
    private enum TriggerType {
        None,
        SphereTerminal,
        PowerUpSnackDistributor,
        HealthSnackDistributor
    }
    
    private TriggerType triggerType;
    
    // Audio management
    private GameObject player;

    static System.Random rnd = new System.Random();
    
    private void OnTriggerEnter(Collider other) {
        if (transform.CompareTag(TriggerType.SphereTerminal.ToString())) {
            triggerType = TriggerType.SphereTerminal;
        }
        else {
            if (transform.CompareTag(TriggerType.PowerUpSnackDistributor.ToString())) {
                triggerType = TriggerType.PowerUpSnackDistributor;
            }
            else {
                if (transform.CompareTag(TriggerType.HealthSnackDistributor.ToString())) {
                    triggerType = TriggerType.HealthSnackDistributor;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        triggerType = TriggerType.None;
    }

    async private void ManageVendingMachine() {
		if(!busy) {
			// Audio management
			player = GameObject.Find("Player");
			
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
				case TriggerType.PowerUpSnackDistributor:
					// Give a random power up for the Player
					if(powerUps.playerPowerUps.Count > 0) {
						if (powerUpVendingMachineHacked)
						{
							// Get power up, lose 1 stamina for the Sphere
							// Audio management
							Debug.Log("Getting power up: taking power up from the machine");
							AudioManager.instance.PlayOneShot(FMODEvents.instance.vendingMachineItemPickUp, this.transform.position);
							powerUpVendingMachineHacked = false;

							busy = true;
							await Task.Delay(2000);

							// Generate a random power up
							int powerUpIndexPlayer = rnd.Next(powerUps.playerPowerUps.Count);
							Debug.Log(powerUps.playerPowerUps.Count);
							
							// Audio management
							var obtainedPowerUp = powerUps.playerPowerUps[powerUpIndexPlayer];
							
							if (obtainedPowerUp == PowerUp.PowerUpType.HealthBoost)
							{
								Debug.Log("Using power up: health boost (chips");
								AudioManager.instance.PlayOneShot(FMODEvents.instance.playerEatChips, player.transform.position);
							}
							
							if (obtainedPowerUp == PowerUp.PowerUpType.MovementBoost)
							{
								Debug.Log("Using power up: movement boost (energy drink");
								AudioManager.instance.PlayOneShot(FMODEvents.instance.playerDrink, player.transform.position);
							}
							
							// Insert the power up in the dictionary of the obtained ones
							powerUps.ObtainPowerUp(powerUps.playerPowerUps[powerUpIndexPlayer]);

							// Remove the power up from the list of power ups
							powerUps.playerPowerUps.RemoveAt(powerUpIndexPlayer);

							busy = false;
						}

						else
						{
							Debug.Log("Getting power up: machine activation");
							// Audio management
							AudioManager.instance.PlayOneShot(FMODEvents.instance.vendingMachineActivation, this.transform.position);

							busy = true;
							await Task.Delay(700);
							busy = false;
							powerUpVendingMachineHacked = true;
						}

					}

					break;
				case TriggerType.HealthSnackDistributor:
					if(healthVendingMachineHacked) {
						// Recover health, lose 1 stamina for the Sphere
						Debug.Log("Recovering health: taking snack from the machine");
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.vendingMachineItemPickUp, this.transform.position);
						healthVendingMachineHacked = false;

						busy = true;
						await Task.Delay(2000);
						
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.playerEatChocolate, player.transform.position);
						
						playerShoot.RecoverHealth(playerShoot.maxHealth);
						playerShoot.DecreaseStamina(1);
						busy = false;
					}
					else {
						Debug.Log("Recovering health: machine activation");
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.vendingMachineActivation, this.transform.position);

						busy = true;
						await Task.Delay(700);
						busy = false;
						healthVendingMachineHacked = true;
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
