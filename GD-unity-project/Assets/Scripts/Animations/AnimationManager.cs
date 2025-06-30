using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public enum RickStates
{
    None,
    Idle,
    Run,
    Attack,
    EndAttack,
    AreaAttack,
    EndAreaAttack,
    DefenseStart,
    DirHitX,
    HitSpit,
    Death,
    StandUp
}

//used to handle the character animations across of the scripts 
public class AnimationManager : MonoBehaviour
{
    [SerializeField] private Animator rickAC;
    [SerializeField] private byte NUM_IDLE_ANIMATIONS = 2;
    [SerializeField] private int WAIT_IDLE_TIME = 2;
    private int idleTriggerHash;
    private int runTriggerHash;
    private int idleIndexHash;
    private int velocityHash;
    private int attackHash;
    private int endAttackHash;
    private int areaAttackHash;
    private int endAreaAttackHash;
    private int defenseHash;
    private int dirHitXHash;
    private int hitSpitHash;
    private int deathHash;
    private int standUpHash;
    //rick current state
    public RickStates rickState;
    //for switch from animation to another for the idle
    private bool randomIdleIsDone = true;
    private bool activeRandomIdle = true;
    //sheildVFX  gameObj
    [SerializeField] GameObject prefabSheildVFX;
    private static AnimationManager instance;

    public static AnimationManager Instance { get { return instance; } }

    protected void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        else {
            instance = this;
        }
    }

    private void Start()
    {
        if (rickAC == null)
        {
            Debug.LogWarning("None Animation Controller insert on inspector");
            rickAC = GetComponent<Animator>();
        }

        idleTriggerHash = Animator.StringToHash("Idle");
        runTriggerHash = Animator.StringToHash("Run");
        velocityHash = Animator.StringToHash("Velocity");
        idleIndexHash = Animator.StringToHash("IdleIndex");
        attackHash = Animator.StringToHash("Attack");
        endAttackHash = Animator.StringToHash("EndAttack");
        areaAttackHash = Animator.StringToHash("AreaAttack");
        endAreaAttackHash = Animator.StringToHash("EndAreaAttack");
        defenseHash = Animator.StringToHash("Defense");
        dirHitXHash = Animator.StringToHash("DirHitX");
        hitSpitHash = Animator.StringToHash("HitSpit");
        deathHash = Animator.StringToHash("Death");
        standUpHash = Animator.StringToHash("StandUp");
        //set the initial state for the rick character
        rickState = RickStates.Idle;

        //sheild check
        if (prefabSheildVFX == null)
            Debug.LogError("The sheild VFX not found!");
    }

    private void Update()
    {
        if (randomIdleIsDone && activeRandomIdle)
        {
            randomIdleIsDone = false;
            RandomizeIdleAsync(WAIT_IDLE_TIME);
        }
    }

    async void RandomizeIdleAsync(int sec)
    {
        await Task.Delay(sec*1000);
        int idleIndex = Random.Range(0, NUM_IDLE_ANIMATIONS);
        if (this != null && rickAC != null && rickAC.gameObject != null && rickAC.isActiveAndEnabled)
        {
            rickAC.SetInteger("IdleIndex", idleIndex);   
        }
        randomIdleIsDone = true;
    }

    public void Run()
    {
        activeRandomIdle = false;
        rickAC.SetTrigger(runTriggerHash);
        rickState = RickStates.Run;

    }
    public void Defense() 
    {
        rickAC.SetTrigger(defenseHash);
        rickState = RickStates.DefenseStart;
    }

    public void SetRunBledingAnim(float normVel)
    {
        rickAC.SetFloat(velocityHash, normVel);
    }

    public void Idle()
    {
        activeRandomIdle = true;
        rickAC.SetTrigger(idleTriggerHash);
        rickState = RickStates.Idle;
    }

    public void Attack()
    {
        rickAC.SetTrigger(attackHash);
        rickState = RickStates.Attack;
    }
    
    public void EndAttack()
    {
        rickAC.SetTrigger(endAttackHash);
        rickState = RickStates.EndAttack;
    }
    
    public void AreaAttack()
    {
        rickAC.SetTrigger(areaAttackHash);
        rickState = RickStates.AreaAttack;
    }
    
    public void EndAreaAttack()
    {
        rickAC.SetTrigger(endAreaAttackHash);
        rickState = RickStates.EndAreaAttack;
    }

    public void DefenseVFX(Vector3 pos)
    {
        Instantiate(prefabSheildVFX, pos, Quaternion.identity); 
    }
    
    public void DirHitX()
    {
        rickAC.SetTrigger(dirHitXHash);
        rickState = RickStates.DirHitX;
    }
    
    public void HitSpit()
    {
        rickAC.SetTrigger(hitSpitHash);
        rickState = RickStates.HitSpit;
    }
    
    public void Death()
    {
        rickAC.SetTrigger(deathHash);
        rickState = RickStates.Death;
    }

    public void StandUp()
    {
        rickAC.SetTrigger(standUpHash);
        rickState = RickStates.StandUp;
    }
}
