using Animations;
using UnityEngine;

public class IncognitoPatrolState : State
{
    private Incognito _incognito;

    public IncognitoPatrolState(string name, Incognito incognito) : base(name)
    {
        _incognito = incognito;
    }

    public override void Enter()
    {
        _incognito.anim.lunchRunAnim();
        
        // Audio management: start footsteps event if incognito is moving
        IncognitoEvents.StartRunningSound();
    }

    public override void Tik()
    {
        _incognito.Patroling();
    }

    public override void Exit()
    {
        // Audio management: stops footsteps event if incognito is not moving anymore
        IncognitoEvents.StopRunningSound();
    }
}

public class IncognitoChaseState : State
{
    private Incognito _incognito;
    public IncognitoChaseState(string name, Incognito incognito) : base(name)
    {
        _incognito = incognito;
    }
    public override void Enter()
    {
        _incognito.anim.lunchRunAnim();
        
        // Audio management: start footsteps event if incognito is moving
        IncognitoEvents.StartRunningSound();
    }

    public override void Tik()
    {
        _incognito.ChasePlayer();
    }

    public override void Exit()
    {
        // Audio management: stops footsteps event if incognito is not moving anymore
        IncognitoEvents.StopRunningSound();
    }
}

public class IncognitoWonderState : State
{
    private Incognito _incognito;
    public IncognitoWonderState(string name, Incognito incognito) : base(name)
    {
        _incognito = incognito;
    }
    public override void Enter()
    {
        _incognito.WonderAttackPlayer();
    }

    public override void Tik()
    {
        
    }

    public override void Exit()
    {

    }
}

public class IncognitoShortSpitAttackState : State
{
    private Incognito _incognito;
    public IncognitoShortSpitAttackState(string name, Incognito incognito) : base(name)
    {
        _incognito = incognito;
    }
    public override void Enter()
    {
        if(!_incognito._alreadyAttacked) {
            _incognito.anim.lunchShortSpitAnim();
        }
    }

    public override void Tik()
    {
        _incognito.ShortSpitAttackPlayer();
    }

    public override void Exit()
    {
    }
}

public class IncognitoReactFromFrontState : State {
	private Incognito _incognito;
	public IncognitoReactFromFrontState(string name, Incognito incognito) : base(name) {
		_incognito = incognito;
	}
	public override void Enter() {
		_incognito.anim.lunchReactFromFrontAnim();
	}

	public override void Tik() {
	}

	public override void Exit() {
	}
}

public class IncognitoDeathState : State
{
    private Incognito _incognito;
    public IncognitoDeathState(string name, Incognito incognito) : base(name)
    {
        _incognito = incognito;
    }
    public override void Enter()
    {
        _incognito.anim.lunchDeathAnim();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
    }
}

public class IncognitoWaitState : State
{
    private Incognito _incognito;

    public IncognitoWaitState(string name, Incognito incognito) : base(name)
    {

        _incognito = incognito;
    }
    public override void Enter()
    {
        _incognito.anim.lunchIdleAnim();
        
        // Audio management: start idle event if incognito is moving
        IncognitoEvents.StartIdleSound();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
        // Audio management: stops idle event if incognito is not waiting anymore
        IncognitoEvents.StopIdleSound();
    }
}

public class IncognitoIdleState : State
{
    private Incognito _incognito;

    public IncognitoIdleState(string name, Incognito incognito) : base(name)
    {
        _incognito = incognito;
    }
    public override void Enter()
    {
        _incognito.clearWaitTime();
        _incognito.SetRandomTimeIdle();
        _incognito.anim.lunchIdleAnim();
        
        // Audio management: start idle event if incognito is moving
        IncognitoEvents.StartIdleSound();
    }

    public override void Tik()
    {
        _incognito.updateWaitTime();
    }

    public override void Exit()
    {
        _incognito.clearWaitTime();
        
        // Audio management: stops idle event if incognito is not waiting anymore
        IncognitoEvents.StopIdleSound();
    }
}