using System.Collections;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.EnemyData.EnemyMovement
{
    public class EnemyDrakeMovement : MonoBehaviour, IEnemy
    {
        
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private LayerMask whatIsGround, whatIsPlayer;
        
        // This variable increases (> 1) or reduces (< 1) the damage taken by this enemy type when attacked
        private float distanceAttackDamageMultiplier;
        private float closeAttackDamageMultiplier;

        private float closeAttackDamage;
        
        private Transform playerTransform;

        private float health;

        //Patroling
        private Vector3 walkPoint;
        private bool walkPointSet;
        private float walkPointRange;

        //Attacking
        private float timeBetweenAttacks;
        private bool alreadyAttacked;
        private GameObject bulletPrefab;

        //States
        private float sightRange, attackRange;
        private bool playerInSightRange, playerInAttackRange;

        private RoomManager.RoomManager roomManager;

        public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
        {
            this.roomManager = roomManager;

            if (!agent) agent = GetComponent<NavMeshAgent>();

            if (enemyData == null || enemyData is not EnemyDrakeData drakeData) return;
            
            agent.speed = enemyData.baseMoveSpeed;

            health = drakeData.maxHealth;
            walkPointRange = drakeData.walkPointRange;
            timeBetweenAttacks = drakeData.timeBetweenAttacks;

            if (drakeData.bulletPrefab)
            {
                bulletPrefab = drakeData.bulletPrefab;
            }
            else if (!bulletPrefab)
            {
                Debug.LogError(
                    $"Drake '{name}' ({enemyData.enemyName}): EnemyDrakeData has no bulletPrefab, and prefab has no default bulletPrefab assigned!",
                    this);
            }

            sightRange = drakeData.sightRange;
            attackRange = drakeData.attackRange;

            distanceAttackDamageMultiplier = drakeData.distanceAttackDamageMultiplier;
            closeAttackDamageMultiplier = drakeData.closeAttackDamageMultiplier;

            closeAttackDamage = drakeData.closeAttackDamage;
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
            if (agent == null || !agent.isOnNavMesh) return;
            
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
            if (agent == null || !agent.isOnNavMesh) return;

            if (roomManager.IsNavMeshBaked)
            {
                agent.SetDestination(playerTransform.position);
            }
        }

        void ResetAttack()
        {
            alreadyAttacked = false;
        }

        void CloseAttackPlayer()
        {
            if (agent == null || !agent.isOnNavMesh) return;
            
            //Make sure enemy doesn't move
            agent.SetDestination(transform.position);

            transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));

            if (!alreadyAttacked)
            {
                //Attack code here
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.tag = "DrakeEnemyAttack";
                
                ParticleAttackController projectileHandler = bullet.GetComponent<ParticleAttackController>();
                if (projectileHandler == null)
                {
                    projectileHandler = bullet.AddComponent<ParticleAttackController>();
                }
                projectileHandler.enemyBulletDamage = closeAttackDamage;
                
                // For Drake, who has a close attack, targetPos may not be strictly necessary if the "bullet" is more
                // of an immediate collision effect or a wave. However, if your ParticleAttackController also handles
                // movement for melee attacks (eg an expanding shock wave), you may want to pass playerTransform.
                // projectileHandler.targetPos = playerTransform; 

                Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
                if (rbBullet == null)
                {
                    rbBullet = bullet.AddComponent<Rigidbody>();
                }
                rbBullet.useGravity = false; // We assume it doesn't want gravity, but it depends on the desired effect
                rbBullet.isKinematic = false; // If Drake "shoots" something with AddForce, it shouldn't be Kinematic

                Collider bulletCollider = bullet.GetComponent<Collider>();
                if (bulletCollider == null)
                {
                    // Add a Collider if not present, such as a SphereCollider
                    SphereCollider sphereCol = bullet.AddComponent<SphereCollider>();
                    sphereCol.isTrigger = true;
                    sphereCol.radius = 0.5f; // Adjust the radius based on the Drake effect
                }
                else
                {
                    bulletCollider.isTrigger = true; // Make sure it's a trigger
                }

                // We leave AddForce as Drake seems to be "throwing" something.
                rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
                rbBullet.AddForce(transform.up * 2f, ForceMode.Impulse);
                //End of attack code

                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }

        public void TakeDamage(float damage, string attackType, bool isShield)
        {
            health -= damage * (attackType == "c" ? closeAttackDamageMultiplier : distanceAttackDamageMultiplier);

            if((attackType == "c" && closeAttackDamageMultiplier != 0) || (attackType == "d" && distanceAttackDamageMultiplier != 0)) {
                
                if (!isShield)
                {
                    StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));   
                }

                if(health <= 0)
                    Invoke(nameof(DestroyEnemy), 0.05f);
            }
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
            playerTransform = GameObject.Find("Player").transform;
            agent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            //Check for sight and attack range
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
            //condition for a transition
            if (!playerInSightRange && !playerInAttackRange)
                Patroling();
            //condition for a transition
            if (playerInSightRange && !playerInAttackRange)
                ChasePlayer();
            //condition for a transition
            if (playerInAttackRange && playerInSightRange)
                CloseAttackPlayer();
        }
    }
}