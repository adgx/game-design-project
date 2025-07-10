using System;
using UnityEngine;

public class MaynardAnimation
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
    private int _runASHash;
    private int _idleASHash;

    private bool _endScream = true;
     private bool _endCloseAttack = true;

    public bool EndScream { get { return _endScream; } set { _endScream = value; } }
    public bool EndCloseAttack { get { return _endCloseAttack; } set { _endCloseAttack = value; } }


    public MaynardAnimation(Animator maynardAC)
    {
        _maynardAC = maynardAC;
        _idleASHash = Animator.StringToHash("Base Layer.Idle");
        _runASHash = Animator.StringToHash("Base Layer.Run");
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

    public void lunchIdleAnim()
    {
        AnimatorStateInfo stateInfo = _maynardAC.GetCurrentAnimatorStateInfo(0);

        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled && !stateInfo.IsTag("Idle"))
        {
            _maynardAC.SetTrigger(_idleTriggerHash);
        }
    }

    public void lunchRunAnim()
    {
        AnimatorStateInfo stateInfo = _maynardAC.GetCurrentAnimatorStateInfo(0);

        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled && !stateInfo.IsTag("Run"))
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
        _endScream = false;
        if (this != null && _maynardAC != null && _maynardAC.gameObject != null && _maynardAC.isActiveAndEnabled)
        {
            _maynardAC.SetTrigger(_screamTriggerHash);
        }
    }

    public void lunchAttackAnim()
    {
        _endCloseAttack = false;
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