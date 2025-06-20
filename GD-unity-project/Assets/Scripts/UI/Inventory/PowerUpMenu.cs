using System.Collections.Generic;
using TMPro;
using UI.Inventory.PowerUpIcons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PowerUpMenu : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private GameObject powerUpMenu;

	[SerializeField] private GameObject spherePowerUpsRow;
	[SerializeField] private GameObject playerPowerUpsRow;
	[SerializeField] private List<PowerUpIcons> powerUpIcons;
	
	[FormerlySerializedAs("powerUp")] [SerializeField] private PowerUp powerUpScript;
	
	[SerializeField] private TextMeshProUGUI powerUpText;

	[SerializeField] private GameObject firstSelected;

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
					spherePowerUpsRow.transform.GetChild(spherePowerUps).GetComponent<Button>().onClick.AddListener(() => ShowPowerUpDescription(puTemp));
					
					Image spherePowerUpIcon = spherePowerUpsRow.transform.GetChild(spherePowerUps).GetComponent<Image>();
					spherePowerUpIcon.sprite = powerUpIcons.Find(p => p.iconId == puTemp + i.ToString()).icon;
					spherePowerUpIcon.color = Color.white;

					spherePowerUps++;
				}
			}
			else
			{
				for (int i = 1; i <= level; i++)
				{
					playerPowerUpsRow.transform.GetChild(playerPowerUps).GetComponent<Button>().onClick.AddListener(() => ShowPowerUpDescription(puTemp));
					
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
		powerUpText.text = powerUp + ": " + powerUpScript.powerUpsDescription[powerUp];
	}
}