using System.Collections;
using Enemy.EnemyData;
using Enemy.EnemyManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Incognito : MonoBehaviour, IEnemy
{
    public bool forceInit = true;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private LayerMask _whatIsGround, _whatIsPlayer;
    [SerializeField] private GameObject attackSpawn;

    private float _distanceAttackDamageMultiplier;
    private float _closeAttackDamageMultiplier;
    private float _distanceAttackDamage;
    private Transform _playerTransform;
    private float _health;

    //Materials
    private Material[] _materials;
    //FSM
    private FiniteStateMachine<Incognito> _stateMachine;

    //animation stuff
    //AnimationManager for Drake
    [HideInInspector]
    public IncognitoAnimation anim;

    //Patrolling
    private Vector3 _walkPoint;
    private bool _walkPointSet;
    private float _walkPointRange;

    //Attacking 
    private float _timeBetweenAttacks;
    private bool _alreadyAttacked;
    private GameObject _bulletPrefab;

    //condition 
    private float _sightRange, _attackRange;
    private bool _playerInSightRange, _playerInAttackRange;

    private RoomManager.RoomManager _roomManager;

    //states
    private State _deathS;

    void Awake()
    {
        Animator incognitoAC = GetComponent<Animator>();

        if (incognitoAC == null)
        {
            Debug.LogError($"{ToString()}: Animator controller not found");
        }

        anim = new IncognitoAnimation(incognitoAC);
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
        if (forceInit)
        {
            _agent.speed = 8f;
            _health = 100f;
            _walkPointRange = 12f;
            _timeBetweenAttacks = 2.5f;
            _sightRange = 12f;
            _attackRange = 8f;
            _distanceAttackDamageMultiplier = 1.2f;
            _closeAttackDamageMultiplier = 1.2f;
        } 
        //FMS base
        _stateMachine = new FiniteStateMachine<Incognito>(this);

        //Define states
        State patrolS = new IncognitoPatrolState("Patrol", this);
        State chaseS = new IncognitoChaseState("Chase", this);
        State wonderS = new IncognitoWonderState("Wonder", this);
        State shortSpitAttackS = new IncognitoShortSpitAttackState("ShortSpitAttack", this);
        _deathS = new IncognitoDeathState("Death", this);
        //Transition
        _stateMachine.AddTransition(patrolS, chaseS, () => _playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(chaseS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(chaseS, wonderS, () => _playerInSightRange && _playerInAttackRange);
        _stateMachine.AddTransition(wonderS, patrolS, () => !_playerInSightRange && !_playerInAttackRange);
        _stateMachine.AddTransition(shortSpitAttackS, wonderS, () => _alreadyAttacked);
        _stateMachine.AddTransition(wonderS, shortSpitAttackS, () => !_alreadyAttacked);
        
        //Set Initial state
        _stateMachine.SetState(patrolS);
    }

    void Update()
    {
        //Check for sight and attack range
        _playerInSightRange = Physics.CheckSphere(transform.position, _sightRange, _whatIsPlayer);
        _playerInAttackRange = Physics.CheckSphere(transform.position, _attackRange, _whatIsPlayer);
        _stateMachine.Tik();
    }
    public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
    {
        _roomManager = roomManager;

        if (!_agent) _agent = GetComponent<NavMeshAgent>();

        if (enemyData == null || enemyData is not EnemyIncognitoData incognitoData) return;

        _agent.speed = enemyData.baseMoveSpeed;

        _health = incognitoData.maxHealth;
        _walkPointRange = incognitoData.walkPointRange;
        _timeBetweenAttacks = incognitoData.timeBetweenAttacks;

        if (incognitoData.bulletPrefab)
        {
            _bulletPrefab = incognitoData.bulletPrefab;
        }
        else if (!_bulletPrefab)
        {
            Debug.LogError(
                $"Incognito '{name}' ({enemyData.enemyName}): EnemyIncognitoData has no bulletPrefab, and prefab has no default bulletPrefab assigned!",
                this);
        }

        _sightRange = incognitoData.sightRange;
        _attackRange = incognitoData.attackRange;

        _distanceAttackDamageMultiplier = incognitoData.distanceAttackDamageMultiplier;
        _closeAttackDamageMultiplier = incognitoData.closeAttackDamageMultiplier;

        _distanceAttackDamage = incognitoData.distanceAttackDamage;
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

        
    }

    void SearchWalkPoint()
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

        if (forceInit || _roomManager.IsNavMeshBaked)
        {
            _agent.SetDestination(_playerTransform.position);
        }
    }

/*
    public void AttackPlayer()
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
*/
    public void WonderAttackPlayer()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;
        
        //Make sure enemy doesn't move
        _agent.SetDestination(transform.position);

        transform.LookAt(new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z));
    }

    public void ShortSpitAttackPlayer()
    {
        //Attack code here
        GameObject bullet = Instantiate(_bulletPrefab, attackSpawn.transform.position, Quaternion.identity);
        bullet.tag = "SpitEnemyAttack";
        bullet.GetComponent<GetCollisions>().enemyBulletDamage = _distanceAttackDamage;
        
        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
        rbBullet.AddForce(transform.forward * 16f, ForceMode.Impulse);
        rbBullet.AddForce(transform.up * 1f, ForceMode.Impulse);
        //End of attack code

        _alreadyAttacked = true;
        Invoke(nameof(ResetAttack), _timeBetweenAttacks);
    }

    void ResetAttack()
    {
        _alreadyAttacked = false;
    }

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
    
    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

}