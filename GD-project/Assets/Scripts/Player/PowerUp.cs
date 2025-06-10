using UnityEngine;
using System.Collections.Generic;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType {
		// Sphere
        DistanceAttackPowerUp,
        CloseAttackPowerUp,
        DefensePowerUp,
        DamageBoost,

		// Player
        HealthBoost,
		MovementBoost
    }

	// This lists contain the Sphere and Player PowerUps that have not been collected yet. When a Power Up is collected, it is removed from the list
	public List<PowerUpType> spherePowerUps = new List<PowerUpType>();
	public List<PowerUpType> playerPowerUps = new List<PowerUpType>();

	public Dictionary<PowerUpType, int> powerUpsObtained = new Dictionary<PowerUpType, int> {};

	private void Start() {
		spherePowerUps.Add(PowerUpType.DistanceAttackPowerUp);
		spherePowerUps.Add(PowerUpType.DistanceAttackPowerUp);
		spherePowerUps.Add(PowerUpType.DefensePowerUp);
		spherePowerUps.Add(PowerUpType.DefensePowerUp);
		spherePowerUps.Add(PowerUpType.DefensePowerUp);
		spherePowerUps.Add(PowerUpType.DamageBoost);

		playerPowerUps.Add(PowerUpType.HealthBoost);
		playerPowerUps.Add(PowerUpType.HealthBoost);
		playerPowerUps.Add(PowerUpType.HealthBoost);
		playerPowerUps.Add(PowerUpType.MovementBoost);
		playerPowerUps.Add(PowerUpType.MovementBoost);
		playerPowerUps.Add(PowerUpType.MovementBoost);
	}

	public void ObtainPowerUp(PowerUpType powerUp) {
		Debug.Log(powerUp.ToString());

		if(powerUpsObtained.ContainsKey(powerUp)) {
			powerUpsObtained[powerUp]++;
		}
		else {
			powerUpsObtained[powerUp] = 1;
		}
	}
}
