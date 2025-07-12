using Animations;
using System.Diagnostics;
using Unity.VisualScripting.FullSerializer;

public class IncognitoPatrolState : State
{
    private Incognito _incognito;
    
    // Audio management
    private IncognitoEvents _events;

    public IncognitoPatrolState(string name, Incognito incognito, IncognitoEvents events) : base(name)
    {
        _incognito = incognito;
        _events = events;
    }

    public override void Enter()
    {
        _incognito.anim.lunchRunAnim();
        
        // Audio management: start footsteps event if Incognito is patrolling
        _events.StartRunningSound();
    }

    public override void Tik()
    {
        _incognito.Patroling();
    }

    public override void Exit()
    {
        // Audio management: stop footsteps event if Incognito is not patrolling anymore
        _events.StopRunningSound();
    }
}

public class IncognitoChaseState : State
{
    private Incognito _incognito;
    
    // Audio management
    private IncognitoEvents _events;
    public IncognitoChaseState(string name, Incognito incognito, IncognitoEvents events) : base(name)
    {
        _incognito = incognito;
        _events = events;
    }
    public override void Enter()
    {
        _incognito.anim.lunchRunAnim();
        
        // Audio management: start footsteps event if Incognito is chasing
        _events.StartRunningSound();
    }

    public override void Tik()
    {
        _incognito.ChasePlayer();
    }

    public override void Exit()
    {
        // Audio management: stop footsteps event if Incognito is not chasing anymore
        _events.StopRunningSound();
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
        _incognito.SpitAttackPlayer();
    }

    public override void Exit()
    {
    }
}

public class IncognitoLongSpitAttackState : State {
	private Incognito _incognito;
	public IncognitoLongSpitAttackState(string name, Incognito incognito) : base(name) {
		_incognito = incognito;
	}
	public override void Enter() {
		if(!_incognito._alreadyAttacked) {
			_incognito.anim.lunchLongSpitAnim();
		}
	}

	public override void Tik() {
		_incognito.SpitAttackPlayer();
	}

	public override void Exit() {
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

    // Audio management
    private IncognitoEvents _events;
    
    public IncognitoWaitState(string name, Incognito incognito, IncognitoEvents events) : base(name)
    {
        _incognito = incognito;
        _events = events;
    }
    public override void Enter()
    {
        _incognito.anim.lunchIdleAnim();
        
        // Audio management: start idle event if Incognito is waiting
        _events.StartIdleSound();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
        // Audio management: stop idle event if Incognito is not waiting anymore
        _events.StopIdleSound();
    }
}

public class IncognitoIdleState : State
{
    private Incognito _incognito;
    
    // Audio management
    private IncognitoEvents _events;

    public IncognitoIdleState(string name, Incognito incognito, IncognitoEvents events) : base(name)
    {
        _incognito = incognito;
        _events = events;
    }
    
    public override void Enter()
    {
        _incognito.clearWaitTime();
        _incognito.SetRandomTimeIdle();
        _incognito.anim.lunchIdleAnim();
        
        // Audio management: start idle event if Incognito is idling
        _events.StartIdleSound();
    }

    public override void Tik()
    {
        _incognito.updateWaitTime();
    }

    public override void Exit()
    {
        _incognito.clearWaitTime();
        
        // Audio management: stop idle event if Incognito is not idling anymore
        _events.StopIdleSound();
    }
}