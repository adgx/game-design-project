using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using Audio;
using FMODUnity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// NOTE: this script is attached to each terminal individually
public class TerminalTrigger : MonoBehaviour
{
	private PlayerShoot playerShoot;
    private PowerUp powerUps;
    
    private TextMeshProUGUI helpText;
	private GameObject helpTextContainer;

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
    private RotateSphere rotateSphere;

    static System.Random rnd = new System.Random();

	private PlayerInput playerInput;

	private void Start() {
		playerInput = Player.Instance.GetComponent<PlayerInput>();
		playerShoot = Player.Instance.GetComponent<PlayerShoot>();
		powerUps = Player.Instance.GetComponent<PowerUp>();
		helpTextContainer = GameObject.Find("CanvasGroup/HUD/HelpTextContainer");
		helpText = helpTextContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		rotateSphere = GameObject.Find("Player/rotatingSphere").GetComponent<RotateSphere>();
	}

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
			    if(!powerUpVendingMachineHacked)
					helpText.text = "Press E to interact with the snack distributor";
			    else
				    helpText.text = "Press E again to take a snack from the machine";
		    }
		    else {
			    if (transform.CompareTag(TriggerType.HealthSnackDistributor.ToString())) {
				    triggerType = TriggerType.HealthSnackDistributor;
				    if (!healthVendingMachineHacked)
						helpText.text = "Press E to interact with the snack distributor";
				    else
					    helpText.text = "Press E again to take a snack from the machine";
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
			helpTextContainer.SetActive(false);

			switch(triggerType) {
				case TriggerType.SphereTerminal:
					// Give a random power up for the Sphere
					if(powerUps.spherePowerUps.Count > 0) {
						// Audio management
						rotateSphere.positionSphere(new Vector3(0.7f, 1f, 0), RotateSphere.Animation.Linear);
						GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerTerminalInteraction, this.transform.position);

						busy = true;
						await Task.Delay(2000);

						// Generate a random power up
						int powerUpIndexSphere = rnd.Next(powerUps.spherePowerUps.Count);
						Debug.Log(powerUps.spherePowerUps.Count);
						
						// Show a message to the player
						helpText.text = "You obtained a " + powerUps.spherePowerUps[powerUpIndexSphere] + "!";
						helpTextContainer.SetActive(true);

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
							GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp, this.transform.position);
							powerUpVendingMachineHacked = false;

							busy = true;
							await Task.Delay(2000);

							// Generate a random power up
							int powerUpIndexPlayer = rnd.Next(powerUps.playerPowerUps.Count);
							Debug.Log(powerUps.playerPowerUps.Count);
							
							// Show a message to the player
							helpText.text = "You obtained a " + powerUps.playerPowerUps[powerUpIndexPlayer] + "!";
							helpTextContainer.SetActive(true);

							// Audio management
							var obtainedPowerUp = powerUps.playerPowerUps[powerUpIndexPlayer];
							
							if (obtainedPowerUp == PowerUp.PlayerPowerUpTypes.HealthBoost)
							{
								Debug.Log("Using power up: health boost (chips");
								GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChips, player.transform.position);
								
								playerShoot.maxHealth += 20;
								playerShoot.health += 20;
							}
							
							if (obtainedPowerUp == PowerUp.PlayerPowerUpTypes.DamageReduction)
							{
								Debug.Log("Using power up: damage reduction (energy drink");
								GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDrink, player.transform.position);
								
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
							GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineActivation, this.transform.position);
							rotateSphere.positionSphere(new Vector3(0.7f, 1f, 0), RotateSphere.Animation.Linear);
							
							busy = true;
							await Task.Delay(700);
							busy = false;
							powerUpVendingMachineHacked = true;
							helpText.text = "Press E again to take a snack from the machine";
							helpTextContainer.SetActive(true);

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
						GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp, this.transform.position);
						healthVendingMachineHacked = false;

						busy = true;
						await Task.Delay(2000);
						
						// Show a message to the player
						helpText.text = "Your health was recovered!";
						helpTextContainer.SetActive(true);

						// Audio management
						GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChocolate, player.transform.position);
						
						playerShoot.RecoverHealth(playerShoot.maxHealth);
						busy = false;
					}
					else {
						Debug.Log("Recovering health: machine activation");
						// Audio management
						GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineActivation, this.transform.position);
						rotateSphere.positionSphere(new Vector3(0.7f, 1f, 0), RotateSphere.Animation.Linear);

						busy = true;
						await Task.Delay(700);
						busy = false;
						healthVendingMachineHacked = true;
						helpText.text = "Press E again to take a snack from the machine";
						helpTextContainer.SetActive(true);

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
        if (triggerType != TriggerType.None && playerInput.InteractionPressed()) {
            ManageVendingMachine();
        }
    }
}
