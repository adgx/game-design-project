using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public enum RickStates
{
    None,
    Idle,
    Run, 
    DefenseStart
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
    private int defenseHash;
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
        defenseHash = Animator.StringToHash("Defense");
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
        rickAC.SetInteger("IdleIndex", idleIndex);
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

    public void DefenseVFX(Vector3 pos)
    {
        Instantiate(prefabSheildVFX, pos, Quaternion.identity); 
    }
}
