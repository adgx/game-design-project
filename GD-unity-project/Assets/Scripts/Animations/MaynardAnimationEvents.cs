using FMOD.Studio;
using UnityEngine;

public class MaynardAnimationEvents : MonoBehaviour
{
    // Audio management
    private EventInstance maynardFootsteps;
    private EventInstance maynardIdle;
    
    private bool isRunning = false; // TODO: to be removed once we have Maynard's FSM
    private bool isIdle = false; // TODO: to be removed once we have Maynard's FSM
    
    public void Idle()
    {
        Debug.Log("Idle");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        isIdle = true; // TODO: to be removed once we have Maynard's FSM
    }
    public void Run()
    {
        Debug.Log("Run");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        isRunning = true; // TODO: to be removed once we have Maynard's FSM

    }
    public void Scream()
    {
        Debug.Log("Scream");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardDistanceAttack1, transform.position);
    }
    
    public void MutantRoaring()
    {
        Debug.Log("MutantRoaring");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardDistanceAttack2, transform.position);
    }
    
    public void Attack()
    {
        Debug.Log("Attack");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardCloseAttack, transform.position);
    }
    
    public void Fall()
    {
        Debug.Log("Fall");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardHitFall, transform.position);
    }
    
    public void StandUpRoar()
    {
        Debug.Log("StandUpRoar");
        
        // Audio management;
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardStandUpRoar, transform.position);
    }
    
    public void StandUpFootstep1()
    {
        Debug.Log("StandUpFootstep1");
        
        // Audio management;
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardFootstep1, transform.position);
    }
    
    public void StandUpFootstep2()
    {
        Debug.Log("StandUpFootstep2");
        
        // Audio management;
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardFootstep2, transform.position);
    }
    
    public void StandUpBreath()
    {
        Debug.Log("StandUpBreath");
        
        // Audio management;
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardStandUpBreath, transform.position);
    }
    
    public void ReactLargeFromRight()
    {
        Debug.Log("ReactLargeFromRight");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardHitFromLeftOrRight, transform.position);
    }
    
    public void ReactLargeFromLeft()
    {
        Debug.Log("ReactLargeFromLeft");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardHitFromLeftOrRight, transform.position);
    }
    
    public void ReactLargeFromFront()
    {
        Debug.Log("ReactLargeFromFront");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardHitFromFront, transform.position);
    }
    
    public void ReactLargeFromBack()
    {
        Debug.Log("ReactLargeFromBack");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardHitFromBack, transform.position);
    }

    public void Death()
    {
        Debug.Log("Death");
        
        // Audio management
        ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
        GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.maynardDie, transform.position);
    }
    
    // Audio management
    private void Start()
    {
        maynardFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.instance.maynardFootsteps);
        maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        
        maynardIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.instance.maynardIdle);
        maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
    }
    
    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        // Audio management
        UpdateSound();
    }
    
    private void UpdateSound()
    {
        maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

        // Start footsteps event if Maynard is moving
        if (isRunning) // TODO: check Maynard's state (something like <<MaynardState != Run>>)
        {
            // Get the playback state for the footsteps event
            PLAYBACK_STATE footstepsPlaybackState;
            maynardFootsteps.getPlaybackState(out footstepsPlaybackState);
            if (footstepsPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                maynardFootsteps.start();
            }
        }
        // Otherwise, stop the footsteps event
        else
        {
            maynardFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        // Start idle event if Maynard is using the idle animation
        if (isIdle) // TODO: check Maynard's state (something like <<MaynardState != Idle>>)
        {
            // Get the playback state for the idle event
            PLAYBACK_STATE idlePlaybackState;
            maynardIdle.getPlaybackState(out idlePlaybackState);
            if (idlePlaybackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                maynardIdle.start();
            }
        }
        // Otherwise, stop the idle event
        else
        {
            maynardIdle.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
    }
    
    // TODO: to be removed once we have Maynard's FSM
    private void ResetAudioState()
    {
        isRunning = false;
        isIdle = false;
    }
}

