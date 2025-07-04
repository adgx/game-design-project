using UnityEngine;

public class CollisionManager : MonoBehaviour
{

    [SerializeField] ParticleSystem splash;

    void Awake()
    {
        splash.gameObject.SetActive(false);
        splash.Stop();       
    }


void OnCollisionEnter(Collision collision)
{
    Debug.Log("Detect");
    //foreach (ContactPoint contact in collision.contacts)
    //{
    //    if (contact.thisCollider.CompareTag("SpitEnemyAttack"))
    //    {
    //        Debug.Log("Splash");
    //        splash.transform.position = contact.point;
    //        splash.transform.rotation = Quaternion.FromToRotation(splash.transform.up, contact.normal);
    //        splash.gameObject.SetActive(true);
    //        splash.Play();
    //    }
    //}
}
}