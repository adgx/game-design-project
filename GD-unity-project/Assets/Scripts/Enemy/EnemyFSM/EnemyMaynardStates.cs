using System;
using UnityEngine;

public class MaynardPatrolState : State
{
    private Maynard _maynard;

    public MaynardPatrolState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }

    public override void Enter()
    {
        Debug.Log(base.Name);
        _maynard.SetChaseRange();
        _maynard.clearWaitTime();
        _maynard.SetRandomTimeIdle();
        _maynard.anim.lunchRunAnim();
    }

    public override void Tik()
    {
        _maynard.Patroling();
        _maynard.updateWaitTime();
    }

    public override void Exit()
    {
        _maynard.clearWaitTime();
    }
}

public class MaynardChaseState : State
{
    private Maynard _maynard;
    public MaynardChaseState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }
    public override void Enter()
    {
        Debug.Log(base.Name);
        _maynard.SetChaseRange();
        _maynard.anim.lunchRunAnim();
    }

    public override void Tik()
    {
        _maynard.ChasePlayer();
    }

    public override void Exit()
    {
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
        Debug.Log(base.Name);
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
        Debug.Log(base.Name);
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
        Debug.Log(base.Name);
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
        Debug.Log(base.Name);
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
        Debug.Log(base.Name);
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

    public MaynardIdleState(string name, Maynard maynard) : base(name)
    {
        Debug.Log(base.Name);
        _maynard = maynard;
    }
    public override void Enter()
    {
        _maynard.clearWaitTime();
        _maynard.SetRandomTimeIdle();
        _maynard.anim.lunchIdleAnim();
    }

    public override void Tik()
    {
        _maynard.updateWaitTime();
    }

    public override void Exit()
    {
        _maynard.clearWaitTime();
    }
}

public class MaynardWaitState : State
{
    private Maynard _maynard;
    
    public MaynardWaitState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    } 
    public override void Enter()
    {
        Debug.Log(base.Name);
        _maynard.anim.lunchIdleAnim();
    }

    public override void Tik()
    {
    }

    public override void Exit()
    {
    }
}