
using System.Collections;
using Enemy.EnemyData;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;
using RoomManager;

public class Drake : MonoBehaviour, IEnemy
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;
    private FiniteStateMachine<Drake> _stateMachine;

    // This variable increases (> 1) or reduces (< 1) the damage taken by this enemy type when attacked
    private float _distanceAttackDamageMultiplier;
    private float _closeAttackDamageMultiplier;
    private float _closeAttackDamage;
    private Transform _playerTransform;
    private float _health;

    //Patroling
    private Vector3 _walkPoint;
    private bool _walkPointSet;
    private float _walkPointRange;

    //Attacking 
    private float _timeBetweenAttacks;
    private bool _alreadyAttacked;
    private GameObject _bulletPrefab;


    private float _sightRange, _attackRange;

    /*likely States
    private bool playerInSightRange, playerInAttackRange;
    */

    private RoomManager.RoomManager _roomManager;

    public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
    {
        _roomManager = roomManager;

        if (!_agent) _agent = GetComponent<NavMeshAgent>();

        if (enemyData == null || enemyData is not EnemyDrakeData drakeData) return;

        _agent.speed = enemyData.baseMoveSpeed;

        _health = drakeData.maxHealth;
        _walkPointRange = drakeData.walkPointRange;
        _timeBetweenAttacks = drakeData.timeBetweenAttacks;

        if (drakeData.bulletPrefab)
        {
            _bulletPrefab = drakeData.bulletPrefab;
        }
        else if (!_bulletPrefab)
        {
            Debug.LogError(
                $"Drake '{name}' ({enemyData.enemyName}): EnemyDrakeData has no bulletPrefab, and prefab has no default bulletPrefab assigned!",
                this);
        }

        //todo: rework 

        _sightRange = drakeData.sightRange;
        _attackRange = drakeData.attackRange;

        _distanceAttackDamageMultiplier = drakeData.distanceAttackDamageMultiplier;
        _closeAttackDamageMultiplier = drakeData.closeAttackDamageMultiplier;

        _closeAttackDamage = drakeData.closeAttackDamage;

    }

    void Awake()
    {
        _playerTransform = GameObject.Find("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {

    }

    void Update()
    {
        _stateMachine.Tik();
    }
    public void TakeDamage(float damage, string attackType)
    {
        _health -= damage * (attackType == "c" ? _closeAttackDamageMultiplier : _distanceAttackDamageMultiplier);

        StartCoroutine(ChangeColor(transform.GetComponent<Renderer>(), Color.red, 0.8f, 0));

        if (_health <= 0)
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

}