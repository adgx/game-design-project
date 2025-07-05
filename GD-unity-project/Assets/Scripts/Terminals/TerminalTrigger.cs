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
	private Player playerScript;
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

	// For PowerUps
	[SerializeField] private GameObject EnergyDrinkMesh;
	[SerializeField] private GameObject SnackMesh;
	// For Health
	[SerializeField] private GameObject SpecialSnackMesh;

	private GameObject energyDrink;
	private GameObject snack;
	private GameObject specialSnack;

	private GameObject LeftHand;
	private GameObject RightHand;

	private void Start() {
		player = Player.Instance.gameObject;
		playerInput = player.GetComponent<PlayerInput>();
		playerShoot = player.GetComponent<PlayerShoot>();
		playerScript = player.GetComponent<Player>();
		powerUps = player.GetComponent<PowerUp>();

		LeftHand = GameObject.Find("Player/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand");
		RightHand = GameObject.Find("Player/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand");

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
			StartCoroutine(RotatePlayerTowards(transform, 0.2f));
			AnimationManager.Instance.Idle();

			helpText.text = "";
			helpTextContainer.SetActive(false);

			switch(triggerType) {
				case TriggerType.SphereTerminal:
					// Give a random power up for the Sphere
					if(powerUps.spherePowerUps.Count > 0) {
						// Audio management
						rotateSphere.positionSphere(new Vector3(rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);
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


							playerShoot.DisableAttacks(true);
							playerScript.FreezeMovement(true);
							busy = true;

							// Generate a random power up
							int powerUpIndexPlayer = rnd.Next(powerUps.playerPowerUps.Count);
							Debug.Log(powerUps.playerPowerUps.Count);

							// Audio management
							var obtainedPowerUp = powerUps.playerPowerUps[powerUpIndexPlayer];
							
							if (obtainedPowerUp == PowerUp.PlayerPowerUpTypes.HealthBoost)
							{
								Debug.Log("Using power up: health boost (chips");
								AnimationManager.Instance.EatChips();
								//GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChips, player.transform.position);

								await Task.Delay(1000);
								PlaceSnackInHand();

								await Task.Delay(5000);

								playerShoot.maxHealth += 20;
								playerShoot.health += 20;
								Destroy(snack);
							}
							
							if (obtainedPowerUp == PowerUp.PlayerPowerUpTypes.DamageReduction)
							{
								Debug.Log("Using power up: damage reduction (energy drink");
								AnimationManager.Instance.Drink();
								//GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDrink, player.transform.position);

								//NOTE: this await is needed and I can not use the events provided by the animation, since I would not have a way to know which is the right TerminalTrigger that I need to reference
								await Task.Delay(1000);
								PlaceDrinkInHand();

								await Task.Delay(5000);

								playerShoot.damageReduction -= 0.2f;
								Destroy(energyDrink);
							}

							// Show a message to the player
							helpText.text = "You obtained a " + powerUps.playerPowerUps[powerUpIndexPlayer] + "!";
							helpTextContainer.SetActive(true);

							// Insert the power up in the dictionary of the obtained ones
							powerUps.ObtainPowerUp(powerUps.playerPowerUps[powerUpIndexPlayer]);

							// Remove the power up from the list of power ups
							powerUps.playerPowerUps.RemoveAt(powerUpIndexPlayer);
							
							busy = false;
							playerShoot.DisableAttacks(false);
							playerScript.FreezeMovement(false);
						}

						else
						{
							Debug.Log("Getting power up: machine activation");
							// Audio management
							GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineActivation, this.transform.position);
							rotateSphere.positionSphere(new Vector3(rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);
							
							busy = true;
							playerShoot.DisableAttacks(true);
							playerScript.FreezeMovement(true);

							// Audio manangement
							await Task.Delay(4200);
							playerShoot.DecreaseStamina(1);
							rotateSphere.isRotating = true;

							powerUpVendingMachineHacked = true;
							helpText.text = "Press E again to take a snack from the machine";
							helpTextContainer.SetActive(true);

							busy = false;
							playerShoot.DisableAttacks(false);
							playerScript.FreezeMovement(false);
						}

					}

					break;
				case TriggerType.HealthSnackDistributor:
					if(healthVendingMachineHacked) {
						// Recover health, lose 1 stamina for the Sphere
						Debug.Log("Recovering health: taking snack from the machine");
						
						busy = true;
						playerShoot.DisableAttacks(true);
						playerScript.FreezeMovement(true);

						// Audio management
						GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp, this.transform.position);

						healthVendingMachineHacked = false;

						AnimationManager.Instance.EatSnack();
						await Task.Delay(1000);
						PlaceSpecialSnackInHand();

						await Task.Delay(5000);
						
						// Show a message to the player
						helpText.text = "Your health was recovered!";
						helpTextContainer.SetActive(true);
						Destroy(specialSnack);

						// Audio management
						//GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChocolate, player.transform.position);
						
						playerShoot.RecoverHealth(playerShoot.maxHealth);
						busy = false;
						playerShoot.DisableAttacks(false);
						playerScript.FreezeMovement(false);
					}
					else {
						Debug.Log("Recovering health: machine activation");
						// Audio management
						GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineActivation, this.transform.position);
						rotateSphere.positionSphere(new Vector3(rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);

						busy = true;
						playerShoot.DisableAttacks(true);
						playerScript.FreezeMovement(true);

						// Audio manangement
						await Task.Delay(4200);
						playerShoot.DecreaseStamina(1);
						rotateSphere.isRotating = true;

						healthVendingMachineHacked = true;
						helpText.text = "Press E again to take a snack from the machine";
						helpTextContainer.SetActive(true);

						busy = false;
						playerShoot.DisableAttacks(false);
						playerScript.FreezeMovement(false);
					}

					break;
				default:
					break;
			}
		}
	}

	private IEnumerator RotatePlayerTowards(Transform target, float duration) {
		Quaternion startRotation = player.transform.rotation;
		Quaternion endRotation = Quaternion.LookRotation(target.forward, Vector3.up);
		float elapsed = 0f;

		while(elapsed < duration) {
			player.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
			elapsed += Time.deltaTime;
			yield return null;
		}

		player.transform.rotation = endRotation; // Ensure final alignment
	}

	private void Update() {
        if (triggerType != TriggerType.None && playerInput.InteractionPressed()) {
            ManageVendingMachine();
        }
    }

	public void PlaceDrinkInHand() {
		energyDrink = Instantiate(EnergyDrinkMesh);
		energyDrink.transform.SetParent(LeftHand.transform);
		energyDrink.transform.SetLocalPositionAndRotation(new Vector3(-7.91e-06f, 7.33e-06f, 3.93e-06f), new Quaternion(-3.593f, 99.3f, 0, 99.3f));
		energyDrink.transform.localScale = new Vector3(0.0002f, 0.0002f, 0.0002f);
	}

	public void PlaceSnackInHand() {
		snack = Instantiate(SnackMesh);
		snack.transform.SetParent(RightHand.transform);
		snack.transform.SetLocalPositionAndRotation(new Vector3(3.59e-06f, 7.72e-06f, 1.022e-05f), new Quaternion(-85.337f, 90, 0, 90));
		snack.transform.localScale = new Vector3(1.6e-05f, 1.6e-05f, 1.6e-05f);
	}

	public void PlaceSpecialSnackInHand() {
		specialSnack = Instantiate(SpecialSnackMesh);
		specialSnack.transform.SetParent(LeftHand.transform);
		specialSnack.transform.SetLocalPositionAndRotation(new Vector3(-5.51e-06f, 1.01e-05f, 3.32e-06f), Quaternion.Euler(-5.322f, 77.962f, -32.059f));
		specialSnack.transform.localScale = new Vector3(0.00015f, 0.00015f, 0.00015f);
	}
}
