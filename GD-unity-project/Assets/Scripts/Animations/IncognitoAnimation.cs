using System;
using UnityEngine;

public class IncognitoAnimation
{
    private Animator _incognitoAC;

    private int _deathTriggerHash;
    private int _runTriggerHash;
    private int _shortSpitTriggerHash;
    private int _longSpitTriggerHash;
    private int _dirXVarHash;
    private int _dirZVarHash;
    private int _hitTriggerHash;
    private int _idleTriggerHash;
    private bool _endShortSpit = true;
    private bool _endLongSpit = true;

    public bool EndShortSpit { get { return _endShortSpit; } set { _endShortSpit = value; } }
    public bool EndLongSpit { get { return _endLongSpit; } set { _endLongSpit = value; } }

    public IncognitoAnimation(Animator incognitoAC)
    {
        _incognitoAC = incognitoAC;
        _deathTriggerHash = Animator.StringToHash("Death");
        _runTriggerHash = Animator.StringToHash("Run");
        _shortSpitTriggerHash = Animator.StringToHash("ShortSpit");
        _longSpitTriggerHash = Animator.StringToHash("LongSpit");
        _dirXVarHash = Animator.StringToHash("DirX");
        _dirZVarHash = Animator.StringToHash("DirZ");
        _hitTriggerHash = Animator.StringToHash("Hit");
        _idleTriggerHash = Animator.StringToHash("Idle");
    }

    public void lunchIdleAnim()
    {
        AnimatorStateInfo stateInfo = _incognitoAC.GetCurrentAnimatorStateInfo(0);
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled && !stateInfo.IsTag("Idle"))
        {
            _incognitoAC.SetTrigger(_idleTriggerHash);
        }
    }

    public void lunchRunAnim()
    {
        AnimatorStateInfo stateInfo = _incognitoAC.GetCurrentAnimatorStateInfo(0);
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled && !stateInfo.IsTag("Run"))
        {
            _incognitoAC.SetTrigger(_runTriggerHash);
        }
    }

    public void lunchDeathAnim()
    {
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
        {
            _incognitoAC.SetTrigger(_deathTriggerHash);
        }
    }

    public void lunchReactFromLeftAnim()
    {
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
        {
            _incognitoAC.SetInteger(_dirXVarHash, -1);
            _incognitoAC.SetInteger(_dirZVarHash, 0);
            _incognitoAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchReactFromRightAnim()
    {
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
        {
            _incognitoAC.SetInteger(_dirXVarHash, 1);
            _incognitoAC.SetInteger(_dirZVarHash, 0);
            _incognitoAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchReactFromFrontAnim()
    {
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
        {
            _incognitoAC.SetInteger(_dirXVarHash, 0);
            _incognitoAC.SetInteger(_dirZVarHash, 1);
            _incognitoAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchReactFromBackAnim()
    {
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
        {
            _incognitoAC.SetInteger(_dirXVarHash, 0);
            _incognitoAC.SetInteger(_dirZVarHash, -1);
            _incognitoAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchFallAnim()
    {
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
        {
            _incognitoAC.SetInteger(_dirXVarHash, 0);
            _incognitoAC.SetInteger(_dirZVarHash, 0);
            _incognitoAC.SetTrigger(_hitTriggerHash);
        }
    }

    public void lunchShortSpitAnim()
    {
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
        {
            _endShortSpit = false;
            _incognitoAC.SetTrigger(_shortSpitTriggerHash);
        }
    }
    
    public void lunchLongSpitAnim()
    { 
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
        {
            _endLongSpit = false;
            _incognitoAC.SetTrigger(_longSpitTriggerHash);
        }
    }
}