using Animations;

public class MaynardPatrolState : State
{
    private Maynard _maynard;
    
    // Audio management
    private MaynardEvents _events;

    public MaynardPatrolState(string name, Maynard maynard, MaynardEvents events) : base(name)
    {
        _maynard = maynard;
        _events = events;
    }

    public override void Enter()
    {
        _maynard.SetChaseRange();
        _maynard.clearWaitTime();
        _maynard.SetRandomTimeIdle();
        _maynard.anim.lunchRunAnim();
        
        // Audio management: start footsteps event if Maynard is patrolling
        _events.StartRunningSound();
    }

    public override void Tik()
    {
        _maynard.Patroling();
        _maynard.updateWaitTime();
    }

    public override void Exit()
    {
        _maynard.clearWaitTime();
        
        // Audio management: stop footsteps event if Maynard is not patrolling anymore
        _events.StopRunningSound();
    }
}

public class MaynardChaseState : State
{
    private Maynard _maynard;
    
    // Audio management
    private MaynardEvents _events;
    
    public MaynardChaseState(string name, Maynard maynard, MaynardEvents events) : base(name)
    {
        _maynard = maynard;
        _events = events;
    }
    public override void Enter()
    {
        _maynard.SetChaseRange();
        _maynard.anim.lunchRunAnim();
        
        // Audio management: start footsteps event if Maynard is chasing
        _events.StartRunningSound();
    }

    public override void Tik()
    {
        _maynard.ChasePlayer();
    }

    public override void Exit()
    {
        // Audio management: stop footsteps event if Maynard is not chasing anymore
        _events.StopRunningSound();
    }
}

public class MaynardWonderState : State
{
    private Maynard _maynard;
    public MaynardWonderState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }
    public override void Enter()
    {
        
    }

    public override void Tik()
    {
        _maynard.WonderAttackPlayer();
    }

    public override void Exit()
    {

    }
}

public class MaynardScreamAttackState : State
{
    private Maynard _maynard;
    public MaynardScreamAttackState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }
    public override void Enter()
    {
        _maynard.anim.lunchScreamAnim();
        _maynard.ScreamAttackPlayer();
    }

    public override void Tik()
    {
        
    }

    public override void Exit()
    {
    }
}

public class MaynardCloseAttackState : State
{
    private Maynard _maynard;
    public MaynardCloseAttackState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }
    public override void Enter()
    {
        _maynard.anim.lunchAttackAnim();
        _maynard.CloseAttackPlayer();
    }

    public override void Tik()
    {
        
    }

    public override void Exit()
    {
    }
}

public class MaynardReactFromFrontState : State {
	private Maynard _maynard;
	public MaynardReactFromFrontState(string name, Maynard maynard) : base(name) {
		_maynard = maynard;
	}
	public override void Enter()
    {
		_maynard.anim.lunchReactFromFront();
	}

	public override void Tik()
    {
	}

    public override void Exit()
    {
        
	}
}

public class MaynardDeathState : State
{
    private Maynard _maynard;
    public MaynardDeathState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }
    public override void Enter()
    {
        _maynard.anim.lunchDeathAnim();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
        
    }
}

public class MaynardIdleState : State
{
    private Maynard _maynard;
    
    // Audio management
    private MaynardEvents _events;

    public MaynardIdleState(string name, Maynard maynard, MaynardEvents events) : base(name)
    {
        _maynard = maynard;
        _events = events;
    }
    public override void Enter()
    {
        _maynard.clearWaitTime();
        _maynard.SetRandomTimeIdle();
        _maynard.anim.lunchIdleAnim();
        
        // Audio management: start idle event if Maynard is idling
        _events.StartIdleSound();
    }

    public override void Tik()
    {
        _maynard.updateWaitTime();
    }

    public override void Exit()
    {
        _maynard.clearWaitTime();
        
        // Audio management: stop idle event if Maynard is not idling anymore
        _events.StopIdleSound();
    }
}

public class MaynardWaitState : State
{
    private Maynard _maynard;
    
    // Audio management
    private MaynardEvents _events;
    
    public MaynardWaitState(string name, Maynard maynard, MaynardEvents events) : base(name)
    {
        _maynard = maynard;
        _events = events;
    } 
    public override void Enter()
    {
        _maynard.anim.lunchIdleAnim();
        
        // Audio management: start idle event if Maynard is waiting
        _events.StartIdleSound();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
        // Audio management: stop idle event if Maynard is not waiting anymore
        _events.StopIdleSound();
    }
}