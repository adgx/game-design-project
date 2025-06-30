
using System.Collections;
using Enemy.EnemyData;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;
using RoomManager;
using System.Collections.Generic;

public class Drake : MonoBehaviour, IEnemy
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;
    //FSM
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

    //Materials
    private Material[] _materials; 
    //checkers
    private bool _playerInSightRange, _playerInAttackRange;
    //Room stuff
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
        //we suppose that all enemy have an one SkinnedMeshRenderer 
        SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();

        if (smr == null)
        {
            Debug.LogError(this.ToString() + ": No SkinnedMeshRenderer is found");
        }

        _materials = smr.materials;

        if (_materials == null)
        {
            Debug.LogError(this.ToString() + ": No materials are found");
        }
    }

    void Start()
    {
        //FMS base
        _stateMachine = new FiniteStateMachine<Drake>(this);

        //Define states
        State patrolS = new DrakePatrolState("Patrol", this);
        State chaseS = new DrakeChaseState("Chase", this);
        State attackS = new DrakeAttackState("Attack", this);

        //Transition
        _stateMachine.AddTransition(patrolS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(chaseS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(chaseS, attackS, () => _playerInSightRange && _playerInAttackRange);
        _stateMachine.AddTransition(attackS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(attackS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);

        //Set Initial state
        _stateMachine.SetState(patrolS);
    }

    void Update()
    {
        //Check for sight and attack range
        _playerInSightRange = Physics.CheckSphere(transform.position, _sightRange, whatIsPlayer);
        _playerInAttackRange = Physics.CheckSphere(transform.position, _attackRange, whatIsPlayer);
        _stateMachine.Tik();
    }
    public void TakeDamage(float damage, string attackType)
    {
        _health -= damage * (attackType == "c" ? _closeAttackDamageMultiplier : _distanceAttackDamageMultiplier);

        StartCoroutine(ChangeColor( Color.red, 0.8f, 0));

        if (_health <= 0)
            Invoke(nameof(DestroyEnemy), 0.05f);
    }

    // Change enemy color when hit and change it back to normal after "duration" seconds
    IEnumerator ChangeColor(Color dmgColor, float duration, float delay)
    {
        // Save the original color of the enemy
        Color originColor = Color.white;

        foreach (Material mat in _materials)
            mat.color = dmgColor;

        yield return new WaitForSeconds(delay);

        float t = 0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime / duration;
            foreach (Material mat in _materials)
                // Lerp animation with given duration in seconds
                mat.color = Color.Lerp(dmgColor, originColor, t);
            yield return null;
        }
        
        foreach (Material mat in _materials)
            mat.color = originColor;
    }

    void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-_walkPointRange, _walkPointRange);
        float randomX = Random.Range(-_walkPointRange, _walkPointRange);
        //destination
        _walkPoint = new Vector3(transform.position.x + randomX, transform.position.y,
            transform.position.z + randomZ);

        if (Physics.Raycast(_walkPoint, -transform.up, 2f, whatIsGround))
        {
            _walkPointSet = true;
        }
    }

    public void Patroling()
    {
        if (_agent == null || !_agent.isOnNavMesh)
        {
            Debug.LogError($"{this} Error with _agent: {_agent}");
            return;
        }

        //maybe this could be a condition for a state
        if (!_walkPointSet)
            SearchWalkPoint();

        if (_walkPointSet)
            _agent.SetDestination(_walkPoint);

        Vector3 distanceToWalkPoint = transform.position - _walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            _walkPointSet = false;
    }

    public void ChasePlayer()
    {
        if (_agent == null || !_agent.isOnNavMesh)
        {
            Debug.LogError($"{this} Error with _agent: {_agent}");
            return;
        }

        if (_roomManager.IsNavMeshBaked)
        {
            _agent.SetDestination(_playerTransform.position);
        }
    }

    public void CloseAttackPlayer()
    {
        if (_agent == null || !_agent.isOnNavMesh)
        {
            Debug.LogError($"{this} Error with _agent: {_agent}");
            return;
        }

        //Make sure enemy doesn't move(Maybe is better set the isStopped property)
        _agent.SetDestination(transform.position);

        transform.LookAt(new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z));

        if (!_alreadyAttacked)
        {
            //Attack code here
            GameObject bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
            bullet.tag = "EnemyProjectile";
            bullet.GetComponent<GetCollisions>().enemyBulletDamage = _closeAttackDamage;

            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
            rbBullet.AddForce(transform.up * 2f, ForceMode.Impulse);
            //End of attack code

            _alreadyAttacked = true;
            Invoke(nameof(ResetAttack), _timeBetweenAttacks);
        }
        
        void ResetAttack()
        {
            _alreadyAttacked = false;
        }
    }


    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

}