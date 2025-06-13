using System.Collections;
using UnityEngine;
using System.Threading.Tasks;


public class RickAnim : MonoBehaviour
{
    //Rick stuff
    [SerializeField] private Animator rickAC;
    [SerializeField] private byte NUM_IDLE_ANIMATIONS = 2;
    [SerializeField] private float WAIT_IDLE_TIME = 10f;
    private bool randomIdleIsDone = true;
    private bool activeRandomIdle = true;
    //AnimatorController parameters
    private int idleTriggerHash;
    private int runTriggerHash;
    private int idleIndexHash;
    private int velocityHash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
    }

    void Update()
    {

        if (randomIdleIsDone && activeRandomIdle)
        {
            randomIdleIsDone = false;
            RandomizeIdleAsync();
        }


    }

    void RandomizeIdleCo()
    {
        Debug.Log("RandomizeIdle called");
        
        StartCoroutine(RandomIdleIndex());
        
    }

    async void RandomizeIdleAsync()
    {
        Debug.Log("RandomizeIdle called");
        await Task.Delay(10000);
        int idleIndex = Random.Range(0, NUM_IDLE_ANIMATIONS);
        Debug.Log($"indexIdle: {idleIndex}");
        rickAC.SetInteger("IdleIndex", idleIndex);
        randomIdleIsDone = true;
    }

    IEnumerator RandomIdleIndex() 
    {
        Debug.Log("Start waiting");
        yield return new WaitForSeconds(WAIT_IDLE_TIME);
        int idleIndex = Random.Range(0, NUM_IDLE_ANIMATIONS);
        Debug.Log($"indexIdle: {idleIndex}");
        rickAC.SetInteger(idleIndexHash, idleIndex);
        randomIdleIsDone = true;
    }

    public void Run()
    {
        activeRandomIdle = false;
        rickAC.SetTrigger(runTriggerHash);

    }
    public void SetRunBledingAnim(float normVel)
    {
        rickAC.SetFloat(velocityHash, normVel);
    }

    public void Idle()
    {
        activeRandomIdle = true;
        rickAC.SetTrigger(idleTriggerHash);
    }
}
