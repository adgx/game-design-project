using System.Collections.Generic;
using TMPro;
using UI.Inventory.PowerUpIcons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Audio;

public class PowerUpMenu : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private GameObject powerUpMenu;

	[SerializeField] private GameObject spherePowerUpsRow;
	[SerializeField] private GameObject playerPowerUpsRow;
	[SerializeField] private List<PowerUpIcons> powerUpIcons;
	
	[FormerlySerializedAs("powerUp")] [SerializeField] private PowerUp powerUpScript;
	
	[SerializeField] private TextMeshProUGUI powerUpText;

	[SerializeField] private GameObject firstSelected;
	
	// Audio management
	[SerializeField] private UIAudioManager uiAudioManager;

	public void OpenMenu()
	{
		int spherePowerUps = 0;
		int playerPowerUps = 0;
		
		powerUpText.text = "PowerUps obtained will be shown here";
		
		foreach ((object pu, int level) in powerUpScript.powerUpsObtained)
		{
			object puTemp = pu;
			if (puTemp is PowerUp.SpherePowerUpTypes)
			{
				for (int i = 1; i <= level; i++)
				{
					Button sphereButton = spherePowerUpsRow.transform.GetChild(spherePowerUps).GetComponent<Button>(); // Take the button reference
					sphereButton.onClick.RemoveAllListeners(); // Clean up old listeners
					sphereButton.onClick.AddListener(() => ShowPowerUpDescription(puTemp)); // Add the new one
					
					Image spherePowerUpIcon = spherePowerUpsRow.transform.GetChild(spherePowerUps).GetComponent<Image>();
					spherePowerUpIcon.sprite = powerUpIcons.Find(p => p.iconId == puTemp.ToString() + i.ToString()).icon;
					spherePowerUpIcon.color = Color.white;

					spherePowerUps++;
				}
			}
			else
			{
				for (int i = 1; i <= level; i++)
				{
					Button playerButton = playerPowerUpsRow.transform.GetChild(playerPowerUps).GetComponent<Button>(); // Take the button reference
					playerButton.onClick.RemoveAllListeners(); // Clean up old listeners
					playerButton.onClick.AddListener(() => ShowPowerUpDescription(puTemp)); // Add the new one
					
					Image playerPowerUpIcon = playerPowerUpsRow.transform.GetChild(playerPowerUps).GetComponent<Image>();
					playerPowerUpIcon.sprite = powerUpIcons.Find(p => p.iconId == puTemp + i.ToString()).icon;
					playerPowerUpIcon.color = Color.white;
					
					playerPowerUps++;
				}
			}
		}

		EventSystem.current.SetSelectedGameObject(firstSelected);
		powerUpMenu.gameObject.SetActive(true);
	}

	public void CloseMenu() {
		powerUpMenu.gameObject.SetActive(false);
	}

	private void ShowPowerUpDescription(object powerUp)
	{
		// Audio management
		uiAudioManager.PlayPositiveSelectionSound();
		
		powerUpText.text = powerUp + ": " + powerUpScript.powerUpsDescription[powerUp];
	}
}