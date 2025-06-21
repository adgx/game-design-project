using UnityEngine;

public class RickAnimationsEvents : MonoBehaviour
{
    [SerializeField] private GameObject rotatingSphere;
    
    //wrapper to lunch shield VFX
    void SpawnMagneticShieldVFX()
    {
        Debug.Log("Event SpawMagneticShieldVFX");
        AnimationManager.Instance.DefenseVFX(transform.position);
        
        // Audio management
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerShieldActivation, rotatingSphere.transform.position);
    }
}