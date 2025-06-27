
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

    /*likely States
    private float sightRange, attackRange;
    private bool playerInSightRange, playerInAttackRange;
    */

    public void Initialize(EnemyData enemyData, RoomManager.RoomManager roomManager)
    {

    }

    public void TakeDamage(float damege, string attackType)
    { 

    }

    void Start()
    {

    }

    void Update()
    {
        _stateMachine.Tik();   
    }
}