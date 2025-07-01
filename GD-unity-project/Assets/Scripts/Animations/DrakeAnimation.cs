using System;
using UnityEngine;

public class DrakeAnimation : MonoBehaviour
{
    private Animator _drakeAC;

    private int _deathTriggerHash;
    private int _runTriggerHash;
    private int _swipingTriggerHash;
    private int _defenseTriggerHash;
    private int _biteTriggerHash;
    private int _dirXVarHash;
    private int _dirZVarHash;
    private int _hitTriggerHash;
    private int _idleTriggerHash;
    public DrakeAnimation(Animator drakeAC)
    {
        _drakeAC = drakeAC;
    }

    void Awake()
    {
        _deathTriggerHash = Animator.StringToHash("Death");
        _runTriggerHash = Animator.StringToHash("Run");
        _swipingTriggerHash = Animator.StringToHash("Swiping");
        _defenseTriggerHash = Animator.StringToHash("Defense");
        _biteTriggerHash = Animator.StringToHash("Bite");
        _dirXVarHash = Animator.StringToHash("DirX");
        _dirZVarHash = Animator.StringToHash("DirZ");
        _hitTriggerHash = Animator.StringToHash("Hit");
        _idleTriggerHash = Animator.StringToHash("Idle");
    }

    public void lunchSwipingAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetTrigger(_swipingTriggerHash);
        }
    }

    public void lunchRunAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetTrigger(_runTriggerHash);
        }
    }

    public void lunchDefenseAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetTrigger(_defenseTriggerHash);
        }
    }

    public void lunchBiteAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetTrigger(_biteTriggerHash);
        }
    }

    public void lunchDeathAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetTrigger(_deathTriggerHash);
        }
    }

    public void lunchReactFromLeftAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetInteger(_dirXVarHash,-1);
            _drakeAC.SetInteger(_dirZVarHash, 0);
            _drakeAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchReactFromRightAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetInteger(_dirXVarHash, 1);
            _drakeAC.SetInteger(_dirZVarHash, 0);
            _drakeAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchReactFromFrontAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetInteger(_dirXVarHash, 0);
            _drakeAC.SetInteger(_dirZVarHash, 1);
            _drakeAC.SetTrigger(_hitTriggerHash);
        }
    }
    
    public void lunchReactFromBackAnim()
    {
        if (this != null && _drakeAC != null && _drakeAC.gameObject != null && _drakeAC.isActiveAndEnabled)
        {
            _drakeAC.SetInteger(_dirXVarHash, 0);
            _drakeAC.SetInteger(_dirZVarHash, -1);
            _drakeAC.SetTrigger(_hitTriggerHash);
        }
    }
}