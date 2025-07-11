using UnityEngine;

public class SpitBenderControl : MonoBehaviour
{
    [SerializeField] private bool update;
    [SerializeField] SpitBendingObject spitPrefab;

    [SerializeField] private Transform _playerTrasform;

    // Update is called once per frame
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
        SpitBendingObject spit = Instantiate(spitPrefab, transform.position, Quaternion.identity);
        spit.SpitBend(target);
    }
}
