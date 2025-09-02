using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    [SerializeField] private float maxRotationSpeed = 200f;
    [SerializeField] private float sightRange = 20;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private Camera mainCamera;
    private float playerPivotOffset;
    private float heightY;
    private bool isActiveLock = false;
    private PlayerInput input;
    private PlayerShoot playerShoot;


    public void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerShoot = GetComponent<PlayerShoot>();
        if (mainCamera = null)
        {
            Debug.Log("Main camera: missing");
        }
    }

    void FixedUpdate()
    {
        if (PlayerInput.Instance.LockPressed())
        {
            isActiveLock = !isActiveLock;
        }
        if (isActiveLock)
            {
                Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, sightRange, whatIsEnemy);
            //bool enemiesPresent = enemiesInRange.Length > 0;
                if (enemiesInRange.Length > 0)
                {
                    // We must inform PlayerShoot of the combat status
                    //playerShoot.isInCombat = enemiesPresent;

                    if (input.Vertical == 0 && input.Horizontal == 0 && !playerShoot.cannotAttack)
                    {
                        LookAtClosestEnemy(enemiesInRange);
                    }
                }
            }
    }

    private Vector3 LookAtClosestEnemy(Collider[] enemies)
    {
        Collider closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Collider enemyCollider in enemies)
        {
            //float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
            //if (distance < minDistance) {
            //    minDistance = distance;
            //    closestEnemy = enemyCollider.transform;
            //}
            float distance = Vector3.Angle(mainCamera.transform.forward, (enemyCollider.transform.position - mainCamera.transform.position));
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemyCollider;

            }
        }

        if (closestEnemy != null)
        {
            Vector3 direction = closestEnemy.transform.position - transform.position;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime * maxRotationSpeed);
            transform.rotation = rotation;
        }
        
        float h1 = closestEnemy.bounds.size.y;
        float delta = h1*0.7f;
        Vector3 tarPos = closestEnemy.transform.position + new Vector3(0.0f, delta, 0.0f);
        if(Blocked(tarPos)) return Vector3.zero;
        return tarPos;
    }


    bool Blocked(Vector3 t){
        RaycastHit hit;
        if(Physics.Linecast(transform.position + Vector3.up * 0.5f, t, out hit)){
            if(!hit.transform.CompareTag("Enemy")) return true;
        }
        return false;
    }    
}