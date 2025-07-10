using UnityEngine;

public class DrakePatrolState : State
{
    private Drake _drake;

    public DrakePatrolState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }

    public override void Enter()
    {
        Debug.Log(base.Name);
        _drake.anim.lunchRunAnim();
    }

    public override void Tik()
    {
        _drake.Patroling();
    }

    public override void Exit()
    {
        
    }
}

public class DrakeChaseState : State
{
    private Drake _drake;
    public DrakeChaseState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }
    public override void Enter()
    {
        Debug.Log(base.Name);
    }

    public override void Tik()
    {
        _drake.ChasePlayer();
    }

    public override void Exit()
    {
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
		Debug.Log(base.Name);
        _drake.anim.EndBit = false;
        _drake.AttackPlayer();
		_drake.anim.lunchBiteAnim();
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
        Debug.Log(base.Name);
        _drake.anim.EndSwiping = false;
        _drake.AttackPlayer();
        _drake.anim.lunchSwipingAnim();
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
        Debug.Log(base.Name);
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

    public DrakeIdleState(string name, Drake drake) : base(name)
    {
        
        _drake = drake;
    }
    public override void Enter()
    {
        Debug.Log(base.Name);
        _drake.clearWaitTime();
        _drake.SetRandomTimeIdle();
        _drake.anim.lunchIdleAnim();
    }

    public override void Tik()
    {
        _drake.updateWaitTime();
    }

    public override void Exit()
    {
        _drake.clearWaitTime();
    }
}

public class DrakeWaitState : State
{
    private Drake _drake;
    
    public DrakeWaitState(string name, Drake drake) : base(name)
    {
        
        _drake = drake;
    }
    public override void Enter()
    {
        Debug.Log(base.Name);
        _drake.anim.lunchIdleAnim();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
    }
}