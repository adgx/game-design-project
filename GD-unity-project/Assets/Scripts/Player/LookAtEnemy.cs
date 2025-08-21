using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    [SerializeField] private float maxRotationSpeed = 200f;
    [SerializeField] private float sightRange = 20;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private float outOfCombatRecoveryDelay = 3f;

    private float playerPivotOffset;
    private float heightY;

    private PlayerInput input;
    private PlayerShoot playerShoot;

    public void Awake() {
        input = GetComponent<PlayerInput>();
        playerShoot = GetComponent<PlayerShoot>();
        
        if (playerShoot != null)
        {
            playerShoot.SetRecoveryDelay(outOfCombatRecoveryDelay);
        }
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
    
    /// <summary>
    /// It contains all the logic to decide whether to start stamina recovery
    /// </summary>
    private void HandleStaminaRecovery(bool enemiesPresent)
    {
        // Basic conditions for not doing anything (charging is already in progress or the stamina is full)
        if (playerShoot.increasingStamina || playerShoot.sphereStamina >= playerShoot.maxSphereStamina)
        {
            return;
        }

        // At this point, we know that the stamina is not full and is not recharging. Now let's decide whether to
        // start reloading based on the presence of enemies
        if (enemiesPresent)
        {
            // Logic in combat: start charging only if the ball is completely discharged
            if (playerShoot.sphereIsDischarged)
            {
                playerShoot.StartStaminaRecovery();
            }
        }
        
        else
        {
            // Out-of-fight logic: start charging as soon as the stamina is not full and
            // if the delay time has elapsed since the last use of the stamina
            
            // Let's calculate the time elapsed since the last use of stamina
            float timeSinceLastUse = Time.time - playerShoot.LastStaminaUseTime;
            
            // If the elapsed time is greater than or equal to our delay, we initiate recovery
            if (timeSinceLastUse >= outOfCombatRecoveryDelay)
            {
                playerShoot.StartStaminaRecovery();  
            }
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