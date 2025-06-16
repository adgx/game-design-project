using UnityEngine;

public class RickAnimationsEvents : MonoBehaviour
{
    //wrapper to lunch shield VFX
    void SpawnMagneticShieldVFX()
    {
        Debug.Log("Event SpawMagneticShieldVFX");
        AnimationManager.Instance.DefenseVFX(transform.position);
    }
}
