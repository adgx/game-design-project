using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    [SerializeField] private float maxRotationSpeed = 200f;
    [SerializeField] private float sightRange = 20;
    [SerializeField] private LayerMask whatIsEnemy;

    private float playerPivotOffset;
    private float heightY;

    private PlayerInput input;
    private PlayerShoot playerShoot;

    public void Awake() {
        input = GetComponent<PlayerInput>();
        playerShoot = GetComponent<PlayerShoot>();
    }

    void FixedUpdate()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, sightRange, whatIsEnemy);
        bool enemiesPresent = enemiesInRange.Length > 0;

        // We must inform PlayerShoot of the combat status
        playerShoot.isInCombat = enemiesPresent;
        
        if (input.Vertical == 0 && input.Horizontal == 0 && enemiesPresent && !playerShoot.cannotAttack) 
        {
            LookAtClosestEnemy(enemiesInRange);
        }
    }
    
    private void LookAtClosestEnemy(Collider[] enemies)
    {
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Collider enemyCollider in enemies) {
            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
            if (distance < minDistance) {
                minDistance = distance;
                closestEnemy = enemyCollider.transform;
            }
        }

        if (closestEnemy != null) {
            Vector3 direction = closestEnemy.transform.position - transform.position;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime * maxRotationSpeed);
            transform.rotation = rotation;
        }
    }
}