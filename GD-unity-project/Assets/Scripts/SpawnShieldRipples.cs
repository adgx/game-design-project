using UnityEngine;
using UnityEngine.VFX;

public class SpawnShieldRipples : MonoBehaviour
{
    public GameObject ShieldRipples;
    private VisualEffect _shieldRipplesVFX;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            var ripples = Instantiate(ShieldRipples, transform) as GameObject;
            _shieldRipplesVFX = ripples.GetComponent<VisualEffect>();
            _shieldRipplesVFX.SetVector3("SphereCenter", collision.contacts[0].point);

            Destroy( ripples, 2 );
        }
    }
}
