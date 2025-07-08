
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
    
    private PlayerShoot playerShoot;

    //states
    private State _reactFromFrontS;
	private State _defenseS;
	private State _deathS;

    void Awake()
    {
        Animator drakeAC = GetComponent<Animator>();

        if (drakeAC == null)
        {
            Debug.LogError($"{ToString()}: Animator controller not found");
        }

        anim = new DrakeAnimation(drakeAC);
        _playerTransform = GameObject.Find("Player").transform;
        Debug.Log($"{_playerTransform.position}");
        _agent = GetComponent<NavMeshAgent>();
        playerShoot = Player.Instance.GetComponent<PlayerShoot>();
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
        State wonderS = new DrakeWonderState("Wonder", this);
        State swipingS = new DrakeSwipingAttackState("Swiping", this);
        State biteS = new DrakeBiteAttackState("Bite", this);
		_reactFromFrontS = new DrakeReactFromFrontState("Hit", this);
        _defenseS = new DrakeDefenseState("Defense", this);
        _deathS = new DrakeDeathState("Death", this);
        //Transition
        _stateMachine.AddTransition(patrolS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(chaseS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(chaseS, wonderS, () => _playerInSightRange && _playerInAttackRange);
        _stateMachine.AddTransition(wonderS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(wonderS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(swipingS, wonderS, () => _alreadyAttacked);
        _stateMachine.AddTransition(wonderS, swipingS, () => !_alreadyAttacked && _playerInSightRange && _playerInAttackRange && playerShoot.health > _closeAttackDamage * playerShoot.damageReduction);
        _stateMachine.AddTransition(biteS, wonderS, () => _alreadyAttacked);
        _stateMachine.AddTransition(wonderS, biteS, () => !_alreadyAttacked && _playerInSightRange && _playerInAttackRange && playerShoot.health <= _closeAttackDamage * playerShoot.damageReduction);

		_stateMachine.AddTransition(_reactFromFrontS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
		_stateMachine.AddTransition(_reactFromFrontS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
		_stateMachine.AddTransition(_reactFromFrontS, wonderS, () => _playerInSightRange && _playerInAttackRange);

		_stateMachine.AddTransition(_defenseS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
		_stateMachine.AddTransition(_defenseS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
		_stateMachine.AddTransition(_defenseS, wonderS, () => _playerInSightRange && _playerInAttackRange);

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

        if(attackType == "c") {
            StartCoroutine(ChangeColor(Color.red, 0.8f, 0));

			if(_health <= 0) {
				gameObject.layer = 0;
				gameObject.tag = "Untagged";

				_stateMachine.SetState(_deathS);
			}
			else {
				_stateMachine.SetState(_reactFromFrontS);
			}
		}
        else {
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
		if(NavMesh.SamplePosition(_walkPoint, out hit, 2f, NavMesh.AllAreas)) {
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

        if (_roomManager.IsNavMeshBaked)
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
        Invoke(nameof(ResetAttack), _timeBetweenAttacks);
    }
    
    public void CheckSwipingAttackDamage()
    {
        if (Physics.CheckSphere(transform.position, 2f, whatIsPlayer))
        {
            playerShoot.TakeDamage(_closeAttackDamage, PlayerShoot.DamageTypes.CloseAttack, 5, 5);
        }
    }
    
    public void CheckBiteAttackDamage()
    {
        if (Physics.CheckSphere(transform.position, 2f, whatIsPlayer))
        {
            playerShoot.TakeDamage(_closeAttackDamage, PlayerShoot.DamageTypes.DrakeBiteAttack, 5, 5);
        }
    }

    void ResetAttack()
    {
        _alreadyAttacked = false;
    } 

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

}