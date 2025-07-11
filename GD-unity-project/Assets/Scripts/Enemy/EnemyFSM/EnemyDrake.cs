
using System.Collections;
using Enemy.EnemyData;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;
using RoomManager;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class Drake : MonoBehaviour, IEnemy
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;
    //FSM
    private FiniteStateMachine<Drake> _stateMachine;

    //animation stuff
    //AnimationManager for Drake
    [HideInInspector]
    public DrakeAnimation anim;

    // This variable increases (> 1) or reduces (< 1) the damage taken by this enemy type when attacked
    private float _distanceAttackDamageMultiplier;
    private float _closeAttackDamageMultiplier;
    private float _closeAttackDamage;
    private Transform _playerTransform;
    private float _health;
    //player health debug
    private float _playerHDG = 50f;
    private float _playerDRDG = 0.2f;
    private string enemyName;

    //Idle
    private float _timeIdle;
    private float _waitCurTime;

    //Patroling
    private Vector3 _walkPoint;
    private bool _walkPointSet;
    private float _walkPointRange;

    //Attacking 
    private float _timeBetweenAttacks;
    public bool _alreadyAttacked = false;
    private GameObject _bulletPrefab;
    private float _sightRange, _attackRange;

    //Materials
    private Material[] _materials;
    //checkers
    private bool _playerInSightRange, _playerInAttackRange;
    //Room stuff
    private RoomManager.RoomManager _roomManager;

    private PlayerShoot playerShoot;

    //states
    private State _reactFromFrontS;
    private State _defenseS;
    private State _deathS;

    private EnemyManager enemyManager;
    private bool _debug = false;

    void Awake()
    {
        Animator drakeAC = GetComponent<Animator>();

        if (drakeAC == null)
        {
            Debug.LogError($"{ToString()}: Animator controller not found");
        }

        anim = new DrakeAnimation(drakeAC);
        _playerTransform = GameObject.Find("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
        if (!_debug)
        {
            playerShoot = Player.Instance.GetComponent<PlayerShoot>();
            enemyManager = GameObject.Find("RoomManager").GetComponent<EnemyManager>();
        }
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
        //debug 
        if (_debug)
        {
            _agent.speed = 3;
            _agent.angularSpeed = 200f;

            _health = 168f;
            _walkPointRange = 6f;
            _timeBetweenAttacks = 4f;
            enemyName = "";
            _sightRange = 9f;
            _attackRange = 1f;

            _distanceAttackDamageMultiplier = 0f;
            _closeAttackDamageMultiplier = 0.8f;
            _closeAttackDamage = 20f;
        }
        //FMS base
            _stateMachine = new FiniteStateMachine<Drake>(this);

        //Define states
        State idleS = new DrakeIdleState("Idle", this);
        State patrolS = new DrakePatrolState("Patrol", this);
        State chaseS = new DrakeChaseState("Chase", this);
        State wonderS = new DrakeWonderState("Wonder", this);
        State swipingS = new DrakeSwipingAttackState("Swiping", this);
        State biteS = new DrakeBiteAttackState("Bite", this);
        State waitS = new DrakeWaitState("Wait", this);

        _reactFromFrontS = new DrakeReactFromFrontState("Hit", this);
        _defenseS = new DrakeDefenseState("Defense", this);
        _deathS = new DrakeDeathState("Death", this);
        //Transition
        //idle
        _stateMachine.AddTransition(idleS, patrolS, () => !_playerInSightRange && _waitCurTime >= _timeIdle);
        _stateMachine.AddTransition(idleS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        //patrol
        _stateMachine.AddTransition(patrolS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        //chase
        _stateMachine.AddTransition(chaseS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(chaseS, wonderS, () => _playerInSightRange && _playerInAttackRange);
        //wonder
        _stateMachine.AddTransition(wonderS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(wonderS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(wonderS, waitS, () => _alreadyAttacked);
        //change playerShoot.health with playerHDG, and playerShoot.damageReduction with  _playerDRDG if you Debug it  
        _stateMachine.AddTransition(wonderS, swipingS, () => !_alreadyAttacked && _playerInSightRange && _playerInAttackRange && playerShoot.health > _closeAttackDamage * playerShoot.damageReduction);
        _stateMachine.AddTransition(wonderS, biteS, () => !_alreadyAttacked && _playerInSightRange && _playerInAttackRange && playerShoot.health <= _closeAttackDamage * playerShoot.damageReduction);
        //wait
        _stateMachine.AddTransition(waitS, wonderS, () => !_alreadyAttacked);
        //swipingS
        _stateMachine.AddTransition(swipingS, wonderS, () => anim.EndSwiping == true);
        //biteS
        _stateMachine.AddTransition(biteS, wonderS, () => anim.EndBit);
        //reactFromFronts
        _stateMachine.AddTransition(_reactFromFrontS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(_reactFromFrontS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(_reactFromFrontS, wonderS, () => _playerInSightRange && _playerInAttackRange);
        //denfese
        _stateMachine.AddTransition(_defenseS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(_defenseS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(_defenseS, wonderS, () => _playerInSightRange && _playerInAttackRange);

        //Set Initial state
        _stateMachine.SetState(idleS);
    }

    void Update()
    {
        // Maybe not a great idea to have this check here, but I don't know where to put it
        if(!playerShoot.magneticShieldOpen) 
            _attackRange = 1;
        else
            _attackRange = 2;

        //Check for sight and attack range
        _playerInSightRange = Physics.CheckSphere(transform.position, _sightRange, whatIsPlayer);
        _playerInAttackRange = Physics.CheckSphere(transform.position, _attackRange, whatIsPlayer);
        _stateMachine.Tik();
    }

    public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
    {
        _roomManager = roomManager;

        if (!_agent) _agent = GetComponent<NavMeshAgent>();

        if (enemyData == null || enemyData is not EnemyDrakeData drakeData) return;

        _agent.speed = enemyData.baseMoveSpeed;
        _agent.angularSpeed = enemyData.angularSpeed;

        _health = drakeData.maxHealth;
        _walkPointRange = drakeData.walkPointRange;
        _timeBetweenAttacks = drakeData.timeBetweenAttacks;
        enemyName = drakeData.enemyName;

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

    public void TakeDamage(float damage, string attackType)
    {
        _health -= damage * (attackType == "c" ? _closeAttackDamageMultiplier : _distanceAttackDamageMultiplier);

        if (attackType == "c")
        {
            StartCoroutine(ChangeColor(Color.red, 0.8f, 0));

            if (_health <= 0)
            {
                gameObject.layer = 0;
                gameObject.tag = "Untagged";
                enemyManager.removeEnemyFromList(_roomManager.CurrentRoomIndex, gameObject, enemyName);

                _stateMachine.SetState(_deathS);
            }
            else
            {
                _stateMachine.SetState(_reactFromFrontS);
            }
        }
        else
        {
            _stateMachine.SetState(_defenseS);
        }

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

        NavMeshHit hit;
        if (NavMesh.SamplePosition(_walkPoint, out hit, 2f, NavMesh.AllAreas))
        {
            _walkPoint = hit.position;
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

        if (_debug || _roomManager.IsNavMeshBaked)
        {
            _agent.SetDestination(_playerTransform.position);
        }
    }

    public void WonderAttackPlayer()
    {
        if (_agent == null || !_agent.isOnNavMesh)
        {
            Debug.LogError($"{this} Error with _agent: {_agent}");
            return;
        }

        //Make sure enemy doesn't move(Maybe is better set the isStopped property)
        _agent.SetDestination(transform.position);

        transform.LookAt(new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z));
    }

    public void AttackPlayer()
    {
        _alreadyAttacked = true;
        StartCoroutine(ResetAttack());
    }

    public void CheckSwipingAttackDamage()
    {
        if (!_debug)
        {
            if (Physics.CheckSphere(transform.position, 2f, whatIsPlayer) && !playerShoot.magneticShieldOpen)
            {
                playerShoot.TakeDamage(_closeAttackDamage, PlayerShoot.DamageTypes.CloseAttack, 5, 5);
            }
        }
    }

    public void CheckBiteAttackDamage()
    {
        if (Physics.CheckSphere(transform.position, 2f, whatIsPlayer) && !playerShoot.magneticShieldOpen)
        {
            playerShoot.TakeDamage(_closeAttackDamage, PlayerShoot.DamageTypes.DrakeBiteAttack, 5, 5);
        }
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(_timeBetweenAttacks);
        _alreadyAttacked = false;
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }
    
    //Idle Logic
    public void clearWaitTime()
    {
        _waitCurTime = 0;
    }
    public void updateWaitTime()
    {
        _waitCurTime += Time.deltaTime;
    }
    public void SetRandomTimeIdle()
    {
        _timeIdle = Random.Range(1f, 3f);
    }

}