using System;
using UnityEngine;

public class IncognitoAnimation : MonoBehaviour
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
    public IncognitoAnimation(Animator incognitoAC)
    {
        _incognitoAC = incognitoAC;
    }

    void Awake()
    {
        _deathTriggerHash = Animator.StringToHash("Death");
        _runTriggerHash = Animator.StringToHash("Run");
        _shortSpitTriggerHash = Animator.StringToHash("ShortSpit");
        _longSpitTriggerHash = Animator.StringToHash("LongSpit");
        _dirXVarHash = Animator.StringToHash("DirX");
        _dirZVarHash = Animator.StringToHash("DirZ");
        _hitTriggerHash = Animator.StringToHash("Hit");
        _idleTriggerHash = Animator.StringToHash("Idle");
    }

    public void lunchRunAnim()
    {
        if (this != null && _incognitoAC != null && _incognitoAC.gameObject != null && _incognitoAC.isActiveAndEnabled)
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
}