using System;
using UnityEngine;

public class MaynardAnimation : MonoBehaviour
{
    private Animator _maynardAC;

    private int _deathTriggerHash;
    private int _runTriggerHash;
    private int _attackTriggerHash;
    private int _roaringTriggerHash;
    private int _screamTriggerHash;
    private int _dirXVarHash;
    private int _dirZVarHash;
    private int _hitTriggerHash;
    private int _idleTriggerHash;

    public MaynardAnimation(Animator maynardAC)
    {
        _maynardAC = maynardAC;
    }

    void Awake()
    {
        _deathTriggerHash = Animator.StringToHash("Death");
        _runTriggerHash = Animator.StringToHash("Run");
        _attackTriggerHash = Animator.StringToHash("Attack");
        _roaringTriggerHash = Animator.StringToHash("Roaring");
        _screamTriggerHash = Animator.StringToHash("Scream");
        _dirXVarHash = Animator.StringToHash("DirX");
        _dirZVarHash = Animator.StringToHash("DirZ");
        _hitTriggerHash = Animator.StringToHash("Hit");
        _idleTriggerHash = Animator.StringToHash("Idle");
    }

    public void lunchRunAnim()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetTrigger(_runTriggerHash);
        }
    }

    public void lunchDeathAnim()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetTrigger(_deathTriggerHash);
        }
    }

    public void lunchReactFromLeft()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetInteger(_dirXVarHash, -1);
            _maynardAC.SetInteger(_dirZVarHash, 0);
            _maynardAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchReactFromRight()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetInteger(_dirXVarHash, 1);
            _maynardAC.SetInteger(_dirZVarHash, 0);
            _maynardAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchReactFromFront()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetInteger(_dirXVarHash, 0);
            _maynardAC.SetInteger(_dirZVarHash, 1);
            _maynardAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchReactFromBack()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetInteger(_dirXVarHash, 0);
            _maynardAC.SetInteger(_dirZVarHash, -1);
            _maynardAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchFallAnim()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetInteger(_dirXVarHash, 0);
            _maynardAC.SetInteger(_dirZVarHash, 0);
            _maynardAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchScreamAnim()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetTrigger(_screamTriggerHash);
        }
    }

    public void lunchAttackAnim()
    {
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetTrigger(_attackTriggerHash);
        }
    }

    public void lunchRoaringAnim()
    { 
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetTrigger(_roaringTriggerHash);
        }
    }   
}