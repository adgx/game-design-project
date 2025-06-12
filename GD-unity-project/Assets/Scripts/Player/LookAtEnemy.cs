using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    [SerializeField] private float maxRotationSpeed = 200f;
    [SerializeField] private float sightRange = 20;
    [SerializeField] private LayerMask whatIsEnemy;

    private PlayerInput input;
    private PlayerShoot playerShoot;

	public void Awake() {
		input = GetComponent<PlayerInput>();
        playerShoot = GetComponent<PlayerShoot>();
    }

	// Update is called once per frame
	void FixedUpdate()
    {
		Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, sightRange, whatIsEnemy);

		if(input.Vertical == 0 && input.Horizontal == 0 && enemiesInRange.Length > 0) {
            Transform closestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (Collider enemyCollider in enemiesInRange) {
                float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestEnemy = enemyCollider.transform;
                }
            }

            if (closestEnemy != null) {
                Vector3 direction = closestEnemy.transform.position - transform.position;

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime * maxRotationSpeed);
                transform.rotation = rotation;
            }
        }
        else {
            if (enemiesInRange.Length == 0 && playerShoot.sphereStamina < playerShoot.maxSphereStamina && !playerShoot.increasingStamina)
            {
                playerShoot.increaseStamina = true;
                playerShoot.RecoverStamina();
            }
        }
        
        if (transform.position.y < 2)
            transform.position = new Vector3(transform.position.x, 3, transform.position.z);
    }
}
