using System.Collections;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.EnemyData.EnemyMovement
{
    public class EnemyIncognitoMovement : MonoBehaviour, IEnemy
    {
        private NavMeshAgent agent;
        private Transform playerTransform;

        public LayerMask whatIsGround, whatIsPlayer;

        public float health;

        //Patroling
        public Vector3 walkPoint;
        bool walkPointSet;
        public float walkPointRange;

        //Attacking
        public float timeBetweenAttacks;
        bool alreadyAttacked;
        public GameObject bulletPrefab;

        //States
        public float sightRange, attackRange;
        public bool playerInSightRange, playerInAttackRange;

        private RoomManager.RoomManager roomManager;

        public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
        {
            this.roomManager = roomManager;

            if (!agent) agent = GetComponent<NavMeshAgent>();

            if (enemyData || enemyData is not EnemyIncognitoData incognitoData) return;

            health = incognitoData.maxHealth;
            walkPointRange = incognitoData.walkPointRange;
            timeBetweenAttacks = incognitoData.timeBetweenAttacks;

            if (incognitoData.bulletPrefab)
            {
                bulletPrefab = incognitoData.bulletPrefab;
            }
            else if (!bulletPrefab)
            {
                Debug.LogError(
                    $"Incognito '{name}' ({enemyData.enemyName}): EnemyIncognitoData has no bulletPrefab, and prefab has no default bulletPrefab assigned!",
                    this);
            }

            sightRange = incognitoData.sightRange;
            attackRange = incognitoData.attackRange;
        }

        void SearchWalkPoint()
        {
            //Calculate random point in range
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y,
                transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            {
                walkPointSet = true;
            }
        }

        void Patroling()
        {
            if (!walkPointSet)
                SearchWalkPoint();

            if (walkPointSet)
                agent.SetDestination(walkPoint);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //Walkpoint reached
            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
        }

        void ChasePlayer()
        {
            if (roomManager != null && roomManager.navMashBaked)
                agent.SetDestination(playerTransform.position);
        }

        void ResetAttack()
        {
            alreadyAttacked = false;
        }

        void AttackPlayer()
        {
            //Make sure enemy doesn't move
            agent.SetDestination(transform.position);

            transform.LookAt(playerTransform);

            if (!alreadyAttacked)
            {
                //Attack code here
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.tag = "EnemyProjectile";

                Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
                rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
                rbBullet.AddForce(transform.up * 2f, ForceMode.Impulse);
                //End of attack code

                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }

        public void TakeDamage(int damage)
        {
            health -= damage;

            StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));

            if (health <= 0)
                Invoke(nameof(DestroyEnemy), 0.05f);
        }

        // Change enemy color when hit and change it back to normal after "duration" seconds
        IEnumerator ChangeColor(Renderer renderer, Color dmgColor, float duration, float delay)
        {
            // Save the original color of the enemy
            Color originColor = renderer.material.color;

            renderer.material.color = dmgColor;

            yield return new WaitForSeconds(delay);

            // Lerp animation with given duration in seconds
            for (float t = 0; t < 1.0f; t += Time.deltaTime / duration)
            {
                renderer.material.color = Color.Lerp(dmgColor, originColor, t);

                yield return null;
            }

            renderer.material.color = originColor;
        }

        private void DestroyEnemy()
        {
            Destroy(gameObject);
        }

        void Awake()
        {
			roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager.RoomManager>();
			playerTransform = GameObject.Find("Player").transform;
            agent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            //Check for sight and attack range
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            if (!playerInSightRange && !playerInAttackRange)
                Patroling();
            if (playerInSightRange && !playerInAttackRange)
                ChasePlayer();
            if (playerInAttackRange && playerInSightRange)
                AttackPlayer();
        }
    }
}