using System.Collections;
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

    //checks
    private float _sightRange, _remoteAttackRange, _closeAttackRange;
    private bool _playerInSightRange, _playerInRemoteAttackRange, _playerInCloseAttackRange;
	//states
	private State _reactFromFrontS;
	private State _deathS;

    private RoomManager.RoomManager _roomManager;
    
    private PlayerShoot playerShoot;

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
        _stateMachine = new FiniteStateMachine<Maynard>(this);

        //Define states
        State patrolS = new MaynardPatrolState("Patrol", this);
        State chaseS = new MaynardChaseState("Chase", this);
        State wonderS = new MaynardWonderState("Wonder", this);
        State screamAttackS = new MaynardScreamAttackState("ScreamAttack", this);
        State closeAttackS = new MaynardCloseAttackState("CloseAttack", this);
		_reactFromFrontS = new MaynardReactFromFrontState("Hit", this);
		_deathS = new MaynardDeathState("Death", this);

        //Transition
        _stateMachine.AddTransition(patrolS, chaseS, () => _playerInSightRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(chaseS, patrolS, () => !_playerInSightRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(chaseS, wonderS, () =>  (_playerInCloseAttackRange && _playerInSightRange) || (_playerInRemoteAttackRange && _playerInSightRange));
        _stateMachine.AddTransition(wonderS, patrolS, () => !_playerInSightRange && !_playerInCloseAttackRange);
        _stateMachine.AddTransition(wonderS, closeAttackS, () => _playerInCloseAttackRange && _playerInSightRange && !_alreadyAttacked);
        _stateMachine.AddTransition(closeAttackS, wonderS, () => _alreadyAttacked);
        _stateMachine.AddTransition(wonderS, screamAttackS, () => !_alreadyAttacked);
        _stateMachine.AddTransition(screamAttackS, wonderS, () => _alreadyAttacked);

		_stateMachine.AddTransition(_reactFromFrontS, patrolS, () => !_playerInSightRange && !_playerInCloseAttackRange);
		_stateMachine.AddTransition(_reactFromFrontS, chaseS, () => _playerInSightRange && !_playerInCloseAttackRange);
		_stateMachine.AddTransition(_reactFromFrontS, wonderS, () => (_playerInCloseAttackRange && _playerInSightRange) || (_playerInRemoteAttackRange && _playerInSightRange));
		
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
        {
            gameObject.layer = 0;
            gameObject.tag = "Untagged";
            
            _stateMachine.SetState(_deathS);
        }
		else {
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
        Invoke(nameof(ResetAttack), _timeBetweenAttacks);
    }

    public void EmitScream() {
		//Attack code here
		GameObject bullet = Instantiate(_bulletPrefab, attackSpawn.transform.position, Quaternion.identity);
		bullet.tag = "EnemyAttack";
		bullet.GetComponent<GetCollisions>().enemyBulletDamage = _distanceAttackDamage;

		Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
		rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
		rbBullet.AddForce(transform.up * 1f, ForceMode.Impulse);
		//End of attack code
	}

	public void CloseAttackPlayer()
    {
        _alreadyAttacked = true;
        Invoke(nameof(ResetAttack), _timeBetweenAttacks);
    }

    public void CheckCloseAttackDamage()
    {
        if (Physics.CheckSphere(transform.position, 2f, _whatIsPlayer))
        {
            playerShoot.TakeDamage(_closeAttackDamage, PlayerShoot.DamageTypes.CloseAttack, 5, 5);
        }
    }
}