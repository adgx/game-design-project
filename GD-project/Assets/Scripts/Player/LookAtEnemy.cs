using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    public GameObject enemy;

    public float maxRotationSpeed = 200f;
    public float sightRange = 20;
    public LayerMask whatIsEnemy;

    private PlayerInput input;

	public void Awake() {
		input = GetComponent<PlayerInput>();
	}

	// Update is called once per frame
	void FixedUpdate()
    {
		bool enemyInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsEnemy);

		if(input.Vertical == 0 && input.Horizontal == 0 && enemyInSightRange) {
            Vector3 direction = enemy.transform.position - transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime * maxRotationSpeed);
            transform.rotation = rotation;
        }
    }
}
