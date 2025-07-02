public class MaynardPatrolState : State
{
    private Maynard _maynard;

    public MaynardPatrolState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }

    public override void Enter()
    {
        _maynard.anim.lunchRunAnim();
    }

    public override void Tik()
    {
        _maynard.Patroling();
    }

    public override void Exit()
    {
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
        _maynard.anim.lunchIdleAnim();
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
    }

    public override void Tik()
    {
        _maynard.ScreamAttackPlayer();
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
    }

    public override void Tik()
    {
        _maynard.CloseAttackPlayer();
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