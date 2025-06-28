using System.Collections;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.EnemyData.EnemyMovement
{
    public class EnemyMaynardMovement : MonoBehaviour, IEnemy
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private LayerMask whatIsGround, whatIsPlayer;
        
        // This variable increases (> 1) or reduces (< 1) the damage taken by this enemy type when attacked
        private float distanceAttackDamageMultiplier;
        private float closeAttackDamageMultiplier;

        private float distanceAttackDamage;
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
        private float sightRange, remoteAttackRange, closeAttackRange;
        private bool playerInSightRange, playerInRemoteAttackRange, playerInCloseAttackRange;

        private RoomManager.RoomManager roomManager;

        public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
        {
            this.roomManager = roomManager;

            if (!agent) agent = GetComponent<NavMeshAgent>();

            if (!enemyData || enemyData is not EnemyManyardData maynardData) return;

            agent.speed = enemyData.baseMoveSpeed;

            health = maynardData.maxHealth;
            walkPointRange = maynardData.walkPointRange;
            timeBetweenAttacks = maynardData.timeBetweenAttacks;

            if (maynardData.bulletPrefab)
            {
                bulletPrefab = maynardData.bulletPrefab;
            }
            else if (!bulletPrefab)
            {
                Debug.LogError(
                    $"Maynard '{name}' ({enemyData.enemyName}): EnemyMaynardData has no bulletPrefab, and prefab has no default bulletPrefab assigned!",
                    this);
            }

			sightRange = maynardData.sightRange;
            remoteAttackRange = maynardData.remoteAttackRange;
            closeAttackRange = maynardData.closeAttackRange;
            gameObject.name = $"{maynardData.enemyName}_Instance_{GetInstanceID()}";
            
            distanceAttackDamageMultiplier = maynardData.distanceAttackDamageMultiplier;
            closeAttackDamageMultiplier = maynardData.closeAttackDamageMultiplier;

            closeAttackDamage = maynardData.closeAttackDamage;
            distanceAttackDamage = maynardData.distanceAttackDamage;
        }

        void SearchWalkPoint()
        {
            //Calculate random point in range
            walkPointRange = 10;
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

        void RemoteAttackPlayer()
        {
            if (agent == null || !agent.isOnNavMesh) return;

			//Make sure enemy doesn't move
			agent.SetDestination(transform.position);

			transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));

			if (!alreadyAttacked)
            {
                //Attack code here
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.tag = "EnemyProjectile";
                bullet.GetComponent<GetCollisions>().enemyBulletDamage = distanceAttackDamage;

                Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
                rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
                rbBullet.AddForce(transform.up * 2f, ForceMode.Impulse);
                //End of attack code

                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
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
                bullet.transform.GetComponent<Renderer>().material.color = Color.red;
                bullet.tag = "EnemyProjectile";
                bullet.GetComponent<GetCollisions>().enemyBulletDamage = closeAttackDamage;
                    
                Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
                rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
                rbBullet.AddForce(transform.up * 2f, ForceMode.Impulse);
                //End of attack code

                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }

        public void TakeDamage(float damage, string attackType)
        {
            health -= damage * (attackType == "c" ? closeAttackDamageMultiplier : distanceAttackDamageMultiplier);

            if((attackType == "c" && closeAttackDamageMultiplier != 0) || (attackType == "d" && distanceAttackDamageMultiplier != 0)) {
                StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));

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
            playerInRemoteAttackRange = Physics.CheckSphere(transform.position, remoteAttackRange, whatIsPlayer);
            playerInCloseAttackRange = Physics.CheckSphere(transform.position, closeAttackRange, whatIsPlayer);

            if (!playerInSightRange && !playerInCloseAttackRange)
                Patroling();
            if (playerInSightRange && !playerInCloseAttackRange)
                ChasePlayer();
            if (playerInCloseAttackRange && playerInSightRange)
                CloseAttackPlayer();
            else if (playerInRemoteAttackRange && playerInSightRange)
            {
                RemoteAttackPlayer();
            }
        }
    }
}