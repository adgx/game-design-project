using UnityEngine;

public class SpitController : MonoBehaviour
{
    [SerializeField] private bool update;
    [SerializeField] SpitBendingObjectV2 spit;

    [SerializeField] private Transform _playerTrasform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (!update)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Attack(_playerTrasform.position);
        }



    }
    
    public void Attack(Vector3 target)
    {
        //GameObject spit = Instantiate(spitPrefab, transform.position, Quaternion.identity);
        spit.SpitBend(target);
    }
}
