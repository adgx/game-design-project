using UnityEngine;
using System.Collections.Generic;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType {
        AttackPowerUp,
        DefensePowerUp,
        DamageBoost,
        HealthBoost,
    }

    public Dictionary<PowerUpType, int> powerUpsObtained = new Dictionary<PowerUpType, int> {
        { PowerUpType.DefensePowerUp, 3 },
        { PowerUpType.AttackPowerUp, 2 },
    };
}
