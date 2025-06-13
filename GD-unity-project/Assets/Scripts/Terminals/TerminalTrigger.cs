using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using FMODUnity;
using TMPro;
using UnityEngine;

// NOTE: this script is attached to each terminal individually
public class TerminalTrigger : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private PowerUp powerUps;
    
    [SerializeField] private TextMeshProUGUI helpText;
	[SerializeField] private GameObject helpTextContainer;

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
    [SerializeField] private RotateSphere rotateSphere;

    static System.Random rnd = new System.Random();
    
    private void OnTriggerEnter(Collider other) {
	    if (!other.CompareTag("Player"))
	    {
		    return;
	    }

	    if (transform.CompareTag(TriggerType.SphereTerminal.ToString())) {
		    triggerType = TriggerType.SphereTerminal;
		    helpText.text = "Press E to interact with the terminal";
	    }
	    else {
		    if (transform.CompareTag(TriggerType.PowerUpSnackDistributor.ToString())) {
			    triggerType = TriggerType.PowerUpSnackDistributor;
			    helpText.text = "Press E to interact with the snack distributor";
		    }
		    else {
			    if (transform.CompareTag(TriggerType.HealthSnackDistributor.ToString())) {
				    triggerType = TriggerType.HealthSnackDistributor;
				    helpText.text = "Press E to interact with the snack distributor";
			    }
		    }
	    }
	    
	    helpTextContainer.SetActive(true);
    }

    private void OnTriggerExit(Collider other) {
	    if (!other.CompareTag("Player"))
	    {
		    return;
	    }
	    helpTextContainer.SetActive(false);
        triggerType = TriggerType.None;
    }

    async private void ManageVendingMachine() {
		if(!busy) {
			// Audio management
			player = GameObject.Find("Player");
			helpText.text = "";
			
			switch(triggerType) {
				case TriggerType.SphereTerminal:
					// Give a random power up for the Sphere
					if(powerUps.spherePowerUps.Count > 0) {
						// Audio management
						rotateSphere.positionSphere(new Vector3(1, 0, 0), RotateSphere.Animation.Linear);
						AudioManager.instance.PlayOneShot(FMODEvents.instance.terminalInteraction, this.transform.position);

						busy = true;
						await Task.Delay(2000);

						// Generate a random power up
						int powerUpIndexSphere = rnd.Next(powerUps.spherePowerUps.Count);
						Debug.Log(powerUps.spherePowerUps.Count);
						
						// Show a message to the player
						helpText.text = "You obtained a " + powerUps.spherePowerUps[powerUpIndexSphere] + "!";

						// Insert the power up in the dictionary of the obtained ones
						powerUps.ObtainPowerUp(powerUps.spherePowerUps[powerUpIndexSphere]);

						// Remove the power up from the list of power ups
						powerUps.spherePowerUps.RemoveAt(powerUpIndexSphere);
						busy = false;
						
						// Audio manangement
						await Task.Delay(1500);
						playerShoot.DecreaseStamina(1);
						rotateSphere.isRotating = true;
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
							
							// Show a message to the player
							helpText.text = "You obtained a " + powerUps.playerPowerUps[powerUpIndexPlayer] + "!";
							
							// Audio management
							var obtainedPowerUp = powerUps.playerPowerUps[powerUpIndexPlayer];
							
							if (obtainedPowerUp == PowerUp.PowerUpType.HealthBoost)
							{
								Debug.Log("Using power up: health boost (chips");
								AudioManager.instance.PlayOneShot(FMODEvents.instance.playerEatChips, player.transform.position);
								
								playerShoot.maxHealth += 20;
								playerShoot.health += 20;
							}
							
							if (obtainedPowerUp == PowerUp.PowerUpType.DamageReduction)
							{
								Debug.Log("Using power up: damage reduction (energy drink");
								AudioManager.instance.PlayOneShot(FMODEvents.instance.playerDrink, player.transform.position);
								
								playerShoot.damageReduction -= 0.2f;
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
							rotateSphere.positionSphere(new Vector3(1, 0, 0), RotateSphere.Animation.Linear);
							
							busy = true;
							await Task.Delay(700);
							busy = false;
							powerUpVendingMachineHacked = true;
							helpText.text = "Press E again to take a snack from the machine";
							
							// Audio manangement
							await Task.Delay(3500);
							playerShoot.DecreaseStamina(1);
							rotateSphere.isRotating = true;
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
						
						// Show a message to the player
						helpText.text = "Your health was recovered!";
						
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.playerEatChocolate, player.transform.position);
						
						playerShoot.RecoverHealth(playerShoot.maxHealth);
						busy = false;
					}
					else {
						Debug.Log("Recovering health: machine activation");
						// Audio management
						AudioManager.instance.PlayOneShot(FMODEvents.instance.vendingMachineActivation, this.transform.position);
						rotateSphere.positionSphere(new Vector3(1, 0, 0), RotateSphere.Animation.Linear);

						busy = true;
						await Task.Delay(700);
						busy = false;
						healthVendingMachineHacked = true;
						helpText.text = "Press E again to take a snack from the machine";
						
						// Audio manangement
						await Task.Delay(3500);
						playerShoot.DecreaseStamina(1);
						rotateSphere.isRotating = true;
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
