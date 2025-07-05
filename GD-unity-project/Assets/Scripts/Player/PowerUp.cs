using UnityEngine;
using System.Collections.Generic;

public class PowerUp : MonoBehaviour
{
    public static PowerUp Instance { get; private set; }

    // Sphere PowerUps
    public enum SpherePowerUpTypes
    {
        DistanceAttackPowerUp,
        CloseAttackPowerUp,
        DefensePowerUp,
    }

    // Player Powerups
    public enum PlayerPowerUpTypes
    {
        HealthBoost,
        DamageReduction
    }

    // This lists contain the Sphere and Player PowerUps that have not been collected yet. When a Power Up is collected, it is removed from the list
    public List<SpherePowerUpTypes> spherePowerUps = new List<SpherePowerUpTypes>();
    public List<PlayerPowerUpTypes> playerPowerUps = new List<PlayerPowerUpTypes>();

    public Dictionary<object, int> powerUpsObtained = new Dictionary<object, int> { };

    public Dictionary<object, string> powerUpsDescription = new Dictionary<object, string>
    {
        { PlayerPowerUpTypes.HealthBoost, "Increases your health" },
        { PlayerPowerUpTypes.DamageReduction, "Reduces damages taken from enemies" },
        {
            SpherePowerUpTypes.DistanceAttackPowerUp,
            "Allows you to charge your distance attacks, to inflict more damage"
        },
        {
            SpherePowerUpTypes.CloseAttackPowerUp,
            "Allows you to charge your close attacks, to inflict more damage and increase the range"
        },
        { SpherePowerUpTypes.DefensePowerUp, "Allows you to use the shield for longer" }
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        spherePowerUps.Add(SpherePowerUpTypes.DistanceAttackPowerUp);
        spherePowerUps.Add(SpherePowerUpTypes.DistanceAttackPowerUp);
        spherePowerUps.Add(SpherePowerUpTypes.CloseAttackPowerUp);
        spherePowerUps.Add(SpherePowerUpTypes.CloseAttackPowerUp);
        spherePowerUps.Add(SpherePowerUpTypes.DefensePowerUp);
        spherePowerUps.Add(SpherePowerUpTypes.DefensePowerUp);

        playerPowerUps.Add(PlayerPowerUpTypes.HealthBoost);
        playerPowerUps.Add(PlayerPowerUpTypes.HealthBoost);
        playerPowerUps.Add(PlayerPowerUpTypes.HealthBoost);
        playerPowerUps.Add(PlayerPowerUpTypes.DamageReduction);
        playerPowerUps.Add(PlayerPowerUpTypes.DamageReduction);
        playerPowerUps.Add(PlayerPowerUpTypes.DamageReduction);
    }

    public void ObtainPowerUp(object powerUp)
    {
        Debug.Log(powerUp.ToString());

        if (powerUpsObtained.ContainsKey(powerUp))
        {
            powerUpsObtained[powerUp]++;
        }
        else
        {
            powerUpsObtained[powerUp] = 1;
        }
    }
}