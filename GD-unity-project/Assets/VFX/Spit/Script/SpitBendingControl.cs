using UnityEngine;

public class SpitBenderControl : MonoBehaviour
{
    [SerializeField] private bool update;
    [SerializeField] SpitBendingObject spitPrefab;

    // Update is called once per frame
    void Update()
    {
        if (!update) 
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) 
            {
                Attack(hit.point);
            }
        }

        
        
    }

    public void Attack(Vector3 target)
    {
        SpitBendingObject spit = Instantiate(spitPrefab, transform.position, Quaternion.identity);
        spit.SpitBend(target);
    }
}
