using Animations;

public class DrakePatrolState : State
{
    private Drake _drake;
    
    // Audio management
    private DrakeEvents _events;

    public DrakePatrolState(string name, Drake drake, DrakeEvents events) : base(name)
    {
        _drake = drake;
        _events = events;
    }

    public override void Enter()
    {
        _drake.anim.lunchRunAnim();
        
        // Audio management: starts footsteps event if Drake is patrolling
        _events.StartRunningSound();
    }

    public override void Tik()
    {
        _drake.Patroling();
    }

    public override void Exit()
    {
        // Audio management: starts footsteps event if Drake is not patrolling anymore
        _events.StopRunningSound();   
    }
}

public class DrakeChaseState : State
{
    private Drake _drake;
    
    // Audio management
    private DrakeEvents _events;
    
    public DrakeChaseState(string name, Drake drake, DrakeEvents events) : base(name)
    {
        _drake = drake;
        _events = events;
    }
    public override void Enter()
    {
        _drake.anim.lunchRunAnim();
        
        // Audio management: starts footsteps event if Drake is chasing
        _events.StartRunningSound();
    }

    public override void Tik()
    {
        _drake.ChasePlayer();
    }

    public override void Exit()
    {
        // Audio management: stops footsteps event if Drake is not chasing
        _events.StopRunningSound();
    }
}

public class DrakeBiteAttackState : State
{
    private Drake _drake;
    public DrakeBiteAttackState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }
    public override void Enter()
    {
        if(!_drake._alreadyAttacked) {
            _drake.anim.EndBit = false;
            _drake.AttackPlayer();
            _drake.anim.lunchBiteAnim();
        }
    }

    public override void Tik()
    {
        
    }

    public override void Exit()
    {
        _drake.anim.lunchRunAnim();
    }
}

public class DrakeSwipingAttackState : State
{
    private Drake _drake;
    public DrakeSwipingAttackState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }
    public override void Enter()
    {
        if(!_drake._alreadyAttacked) {
            _drake.anim.EndSwiping = false;
            _drake.AttackPlayer();
            _drake.anim.lunchSwipingAnim();
        }
    }

    public override void Tik()
    {
        
    }

    public override void Exit()
    {
    }
}

public class DrakeWonderState : State
{
    private Drake _drake;
    public DrakeWonderState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }
    public override void Enter()
    {
        _drake.WonderAttackPlayer();
    }

    public override void Tik()
    {
        
    }

    public override void Exit()
    {

    }
}

public class DrakeReactFromFrontState : State {
	private Drake _drake;
	public DrakeReactFromFrontState(string name, Drake drake) : base(name) {
		_drake = drake;
	}
	public override void Enter() {
		_drake.anim.lunchReactFromFrontAnim();
	}

	public override void Tik() {
	}

	public override void Exit() {

	}
}

public class DrakeDefenseState : State {
	private Drake _drake;
	public DrakeDefenseState(string name, Drake drake) : base(name) {
		_drake = drake;
	}
	public override void Enter() {
		_drake.anim.lunchDefenseAnim();
	}

	public override void Tik() {
	}

	public override void Exit() {
	}
}

public class DrakeDeathState : State
{
    private Drake _drake;
    public DrakeDeathState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }
    public override void Enter()
    {
        _drake.anim.lunchDeathAnim();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
    }
}

public class DrakeIdleState : State
{
    private Drake _drake;
    
    // Audio management
    private DrakeEvents _events;

    public DrakeIdleState(string name, Drake drake, DrakeEvents events) : base(name)
    {
        _drake = drake;
        _events = events;
    }
    public override void Enter()
    {
        _drake.clearWaitTime();
        _drake.SetRandomTimeIdle();
        _drake.anim.lunchIdleAnim();
        
        // Audio management: starts idle event if Drake is idling
        _events.StartIdleSound();
    }

    public override void Tik()
    {
        _drake.updateWaitTime();
    }

    public override void Exit()
    {
        _drake.clearWaitTime();
        
        // Audio management: stops idle event if Drake is not idling anymore
        _events.StopIdleSound();
    }
}

public class DrakeWaitState : State
{
    private Drake _drake;
 
    // Audio management
    private DrakeEvents _events;
    
    public DrakeWaitState(string name, Drake drake, DrakeEvents events) : base(name)
    {
        _drake = drake;
        _events = events;
    }
    
    public override void Enter()
    {
        _drake.anim.lunchIdleAnim();
        
        // Audio management: starts idle event if Drake is waiting
        _events.StartIdleSound();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
        // Audio management: stops idle event if Drake is not waiting anymore
        _events.StopIdleSound();
    }
}