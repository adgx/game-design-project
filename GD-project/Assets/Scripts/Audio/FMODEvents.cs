using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference doorOpen { get; private set; }
    [field: SerializeField] public EventReference doorClose { get; private set; }
    [field: SerializeField] public EventReference paperInteraction { get; private set; }
    [field: SerializeField] public EventReference terminalInteraction { get; private set; }
    [field: SerializeField] public EventReference vendingMachineActivation { get; private set; }
    [field: SerializeField] public EventReference vendingMachineItemPickUp { get; private set; }
    [field: SerializeField] public EventReference sphereDischarge { get; private set; }
    [field: SerializeField] public EventReference playerWakeUp { get; private set; }
    
    [field: Header("Sphere SFX")]
    [field: SerializeField] public EventReference sphereRotation { get; private set; }
    
    [field: Header("Laboratory SFX")]
    [field: SerializeField] public EventReference laboratoryAlarm { get; private set;  }
    
    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }
    
    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;
    }
}