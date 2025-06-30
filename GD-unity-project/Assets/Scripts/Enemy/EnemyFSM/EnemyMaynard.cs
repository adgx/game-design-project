using System.Collections;
using Enemy.EnemyData;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;

public class Maynard : MonoBehaviour, IEnemy
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private LayerMask _whatIsGround, _whatIsPlayer;

    //FSM
    private FiniteStateMachine<Maynard> _stateMachine;
    // This variable increases (> 1) or reduces (< 1) the damage taken by this enemy type when attacked
    private float _distanceAttackDamageMultiplier;
    private float _closeAttackDamageMultiplier;

    private float _distanceAttackDamage;
    private float _closeAttackDamage;

    private Transform _playerTransform;

    private float _health;
    //Materials
    private Material[] _materials;

    //Patroling
    private Vector3 _walkPoint;
    private bool _walkPointSet;
    private float _walkPointRange;

    //Attacking
    private float _timeBetweenAttacks;
    private bool _alreadyAttacked;
    private GameObject _bulletPrefab;

    //States
    private float _sightRange, _remoteAttackRange, _closeAttackRange;
    private bool _playerInSightRange, _playerInRemoteAttackRange, _playerInCloseAttackRange;

    private RoomManager.RoomManager _roomManager;

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
        _stateMachine = new FiniteStateMachine<Maynard>(this);

        //Define states
        State patrolS = new MaynardPatrolState("Patrol", this);
        State chaseS = new MaynardChaseState("Chase", this);
        State attackS = new MaynardAttackState("Attack", this);

        //Transition
        _stateMachine.AddTransition(patrolS, chaseS, () => _playerInSightRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(chaseS, patrolS, () => !_playerInSightRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(chaseS, attackS, () =>  _playerInCloseAttackRange && _playerInSightRange);
        _stateMachine.AddTransition(attackS, chaseS, () => _playerInSightRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(attackS, patrolS, () => !_playerInSightRange && !_playerInCloseAttackRange);

        //Set Initial state
        _stateMachine.SetState(patrolS);
    }

    void Update()
    {
        //Check for sight and attack range
        _playerInSightRange = Physics.CheckSphere(transform.position, _sightRange, _whatIsPlayer);
        _playerInRemoteAttackRange = Physics.CheckSphere(transform.position, _remoteAttackRange, _whatIsPlayer);
        _playerInCloseAttackRange = Physics.CheckSphere(transform.position, _closeAttackRange, _whatIsPlayer);
        /*    
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
        */
        _stateMachine.Tik();
    }

    public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
    {
        _roomManager = roomManager;

        if (!_agent) _agent = GetComponent<NavMeshAgent>();

        if (!enemyData || enemyData is not EnemyManyardData maynardData) return;

        _agent.speed = enemyData.baseMoveSpeed;

        _health = maynardData.maxHealth;
        _walkPointRange = maynardData.walkPointRange;
        _timeBetweenAttacks = maynardData.timeBetweenAttacks;

        if (maynardData.bulletPrefab)
        {
            _bulletPrefab = maynardData.bulletPrefab;
        }
        else if (!_bulletPrefab)
        {
            Debug.LogError(
                $"Maynard '{name}' ({enemyData.enemyName}): EnemyMaynardData has no bulletPrefab, and prefab has no default bulletPrefab assigned!",
                this);
        }

        _sightRange = maynardData.sightRange;
        _remoteAttackRange = maynardData.remoteAttackRange;
        _closeAttackRange = maynardData.closeAttackRange;
        gameObject.name = $"{maynardData.enemyName}_Instance_{GetInstanceID()}";

        _distanceAttackDamageMultiplier = maynardData.distanceAttackDamageMultiplier;
        _closeAttackDamageMultiplier = maynardData.closeAttackDamageMultiplier;

        _closeAttackDamage = maynardData.closeAttackDamage;
        _distanceAttackDamage = maynardData.distanceAttackDamage;
    }

    public void TakeDamage(float damage, string attackType)
    {
        _health -= damage * (attackType == "c" ? _closeAttackDamageMultiplier : _distanceAttackDamageMultiplier);

        StartCoroutine(ChangeColor(Color.red, 0.8f, 0));

        if (_health <= 0)
            Invoke(nameof(DestroyEnemy), 0.05f);
    }

    private IEnumerator ChangeColor(Color dmgColor, float duration, float delay)
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

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        _walkPointRange = 10;
        float randomZ = Random.Range(-_walkPointRange, _walkPointRange);
        float randomX = Random.Range(-_walkPointRange, _walkPointRange);

        _walkPoint = new Vector3(transform.position.x + randomX, transform.position.y,
            transform.position.z + randomZ);

        if (Physics.Raycast(_walkPoint, -transform.up, 2f, _whatIsGround))
        {
            _walkPointSet = true;
        }
    }

    public void Patroling()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

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
        if (_agent == null || !_agent.isOnNavMesh) return;

        if (_roomManager.IsNavMeshBaked)
        {
            _agent.SetDestination(_playerTransform.position);
        }
    }

    void ResetAttack()
    {
        _alreadyAttacked = false;
    }

    public void RemoteAttackPlayer()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        //Make sure enemy doesn't move
        _agent.SetDestination(transform.position);

        transform.LookAt(new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z));

        if (!_alreadyAttacked)
        {
            //Attack code here
            GameObject bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
            bullet.tag = "EnemyProjectile";
            bullet.GetComponent<GetCollisions>().enemyBulletDamage = _distanceAttackDamage;

            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
            rbBullet.AddForce(transform.up * 2f, ForceMode.Impulse);
            //End of attack code

            _alreadyAttacked = true;
            Invoke(nameof(ResetAttack), _timeBetweenAttacks);
        }
    }
    
    public void CloseAttackPlayer()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        //Make sure enemy doesn't move
        _agent.SetDestination(transform.position);

        transform.LookAt(new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z));

        if (!_alreadyAttacked)
        {
            //Attack code here
            GameObject bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.GetComponent<Renderer>().material.color = Color.red;
            bullet.tag = "EnemyProjectile";
            bullet.GetComponent<GetCollisions>().enemyBulletDamage = _closeAttackDamage;
                
            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
            rbBullet.AddForce(transform.up * 2f, ForceMode.Impulse);
            //End of attack code

            _alreadyAttacked = true;
            Invoke(nameof(ResetAttack), _timeBetweenAttacks);
        }
    }

}