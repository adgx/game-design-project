using System.Collections;
using Animations;
using Enemy.EnemyData;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;

public class Maynard : MonoBehaviour, IEnemy
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private LayerMask _whatIsGround, _whatIsPlayer;
    [SerializeField] private GameObject attackSpawn;
    
    //FSM
    private FiniteStateMachine<Maynard> _stateMachine;
    // This variable increases (> 1) or reduces (< 1) the damage taken by this enemy type when attacked

    //animation stuff
    //AnimationManager for Drake
    [HideInInspector]
    public MaynardAnimation anim;

    private float _distanceAttackDamageMultiplier;
    private float _closeAttackDamageMultiplier;

    private float _distanceAttackDamage;
    private float _closeAttackDamage;

    private Transform _playerTransform;

    private float _health;
    private string enemyName;

    //Materials
    private Material[] _materials;

    //Patroling
    private Vector3 _walkPoint;
    private bool _walkPointSet;
    private float _walkPointRange;

    //Idle
    private float _timeIdle;

    private float _waitCurTime;
    //Attacking
    private float _timeBetweenAttacks;
    public bool _alreadyAttacked;
    private GameObject _bulletPrefab;

    //checks
    private float _sightRange, _remoteAttackRange, _closeAttackRange, _chaseRange;
    private bool _playerInSightRange, _playerInRemoteAttackRange, _playerInCloseAttackRange;
    //states
    private State _reactFromFrontS;
    private State _deathS;

    private RoomManager.RoomManager _roomManager;

    private PlayerShoot playerShoot;

    private EnemyManager enemyManager;

    private bool _debug = false;

    private MaynardEvents _events;

    void Awake()
    {
        Animator maynardAC = GetComponent<Animator>();

        if (maynardAC == null)
        {
            Debug.LogError($"{ToString()}: Animator controller not found");
        }

        anim = new MaynardAnimation(maynardAC);
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
        if (_debug)
        {
            _agent = GetComponent<NavMeshAgent>();
            _sightRange = 12f;
            _remoteAttackRange = 7f;
            _closeAttackRange = 1;
            gameObject.name = $"_Instance_{GetInstanceID()}";

            _distanceAttackDamageMultiplier = 1f;
            _closeAttackDamageMultiplier = 1f;

            _closeAttackDamage = 1f;
            _distanceAttackDamage = 13f;

            _agent.speed = 5f;
            _agent.angularSpeed = 200;

            _health = 120f;
            _walkPointRange = 10f;
            _timeBetweenAttacks = 3f;
            enemyName = "";
        }
        
        // Audio management
        _events = GetComponent<MaynardEvents>(); 
    }

    void Start()
    {
        //FMS base
        _stateMachine = new FiniteStateMachine<Maynard>(this);

        //Define states
        State idleS = new MaynardIdleState("Idle", this, _events);
        State patrolS = new MaynardPatrolState("Patrol", this, _events);
        State chaseS = new MaynardChaseState("Chase", this, _events);
        State wonderS = new MaynardWonderState("Wonder", this);
        State screamAttackS = new MaynardScreamAttackState("ScreamAttack", this);
        State closeAttackS = new MaynardCloseAttackState("CloseAttack", this);
        State waitS = new MaynardWaitState("Wait", this, _events);
        _reactFromFrontS = new MaynardReactFromFrontState("Hit", this);
        _deathS = new MaynardDeathState("Death", this);
        //take attention on the order with the transitions are added
        //idle Transitions
        _stateMachine.AddTransition(idleS, chaseS, () => _playerInSightRange && (_playerInRemoteAttackRange || _playerInCloseAttackRange));
        _stateMachine.AddTransition(idleS, patrolS, () => !_playerInSightRange && _waitCurTime >= _timeIdle);
        //Patrol
        _stateMachine.AddTransition(patrolS, chaseS, () => _playerInSightRange && (!_playerInCloseAttackRange || !_playerInRemoteAttackRange));
        //chase
        _stateMachine.AddTransition(chaseS, patrolS, () => !_playerInSightRange);
        _stateMachine.AddTransition(chaseS, wonderS, () => _playerInSightRange && (_playerInRemoteAttackRange || _playerInRemoteAttackRange)); // funzione obbiettivo
        //wonder
        _stateMachine.AddTransition(wonderS, patrolS, () => !_playerInSightRange);
        _stateMachine.AddTransition(wonderS, chaseS, () => _playerInSightRange && !_playerInRemoteAttackRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(wonderS, waitS, () => _alreadyAttacked);
        _stateMachine.AddTransition(wonderS, closeAttackS, () => _playerInCloseAttackRange && _playerInSightRange && !_alreadyAttacked);
        _stateMachine.AddTransition(wonderS, screamAttackS, () => _playerInRemoteAttackRange && _playerInSightRange && !_alreadyAttacked);
        //wait
        _stateMachine.AddTransition(waitS, wonderS, () => !_alreadyAttacked);
        //attacks
        _stateMachine.AddTransition(closeAttackS, wonderS, () => anim.EndCloseAttack);
        _stateMachine.AddTransition(screamAttackS, wonderS, () => anim.EndScream);
        //react
        _stateMachine.AddTransition(_reactFromFrontS, patrolS, () => !_playerInSightRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(_reactFromFrontS, chaseS, () => _playerInSightRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(_reactFromFrontS, wonderS, () => (_playerInCloseAttackRange && _playerInSightRange) || (_playerInRemoteAttackRange && _playerInSightRange));
        
        //Set Initial state
        _stateMachine.SetState(idleS);
    }

    void Update()
    {
		// Maybe not a great idea to have this check here, but I don't know where to put it
		if(!playerShoot.magneticShieldOpen)
			_closeAttackRange = 1;
		else
			_closeAttackRange = 2;

		//Check for sight and attack range
		_playerInSightRange = Physics.CheckSphere(transform.position, _sightRange, _whatIsPlayer);
        _playerInRemoteAttackRange = Physics.CheckSphere(transform.position, _remoteAttackRange, _whatIsPlayer);
        _playerInCloseAttackRange = Physics.CheckSphere(transform.position, _closeAttackRange, _whatIsPlayer);

        _stateMachine.Tik();
    }

    public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
    {
        _roomManager = roomManager;

        if (!_agent) _agent = GetComponent<NavMeshAgent>();

        if (!enemyData || enemyData is not EnemyManyardData maynardData) return;

        _agent.speed = enemyData.baseMoveSpeed;
        _agent.angularSpeed = enemyData.angularSpeed;

        _health = maynardData.maxHealth;
        _walkPointRange = maynardData.walkPointRange;
        _timeBetweenAttacks = maynardData.timeBetweenAttacks;
        enemyName = maynardData.enemyName;

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

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-_walkPointRange, _walkPointRange);
        float randomX = Random.Range(-_walkPointRange, _walkPointRange);

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

        if (_debug || _roomManager.IsNavMeshBaked)
        {
            _agent.SetDestination(_playerTransform.position);
        }
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(_timeBetweenAttacks);
        _alreadyAttacked = false;
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

    public void ScreamAttackPlayer()
    {
        _alreadyAttacked = true;
        StartCoroutine(ResetAttack());
    }

    public void EmitScream()
    {
        //Attack code here
        if (!_debug)
        {
            GameObject bullet = Instantiate(_bulletPrefab, attackSpawn.transform.position, Quaternion.identity);
            bullet.tag = "EnemyAttack";
            bullet.GetComponent<GetCollisions>().enemyBulletDamage = _distanceAttackDamage;

            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
            rbBullet.AddForce(transform.up * 1f, ForceMode.Impulse);
        }
        //End of attack code
    }

    public void CloseAttackPlayer()
    {
        _alreadyAttacked = true;
		StartCoroutine(ResetAttack());
	}

    public void CheckCloseAttackDamage()
    {
        if (Physics.CheckSphere(transform.position, 2f, _whatIsPlayer) && !playerShoot.magneticShieldOpen)
        {
            playerShoot.TakeDamage(_closeAttackDamage, PlayerShoot.DamageTypes.CloseAttack, 5, 5);
        }
        if (Physics.CheckSphere(transform.position, 2f, _whatIsPlayer))
        {
            if (!_debug)
            {
                playerShoot.TakeDamage(_closeAttackDamage, PlayerShoot.DamageTypes.CloseAttack, 5, 5);
            }
        }
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

    public void SetChaseRange()
    {
        int choice = Random.Range(0, 2);

        _chaseRange = choice == 0 ? _remoteAttackRange : _closeAttackRange;
    }

    public bool CheckeChaseRange()
    {
        if (_chaseRange == _remoteAttackRange)
        {
            return _playerInRemoteAttackRange;
        }
        else return _playerInCloseAttackRange;
    }
}