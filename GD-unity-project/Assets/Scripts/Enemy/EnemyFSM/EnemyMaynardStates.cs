public class MaynardPatrolState : State
{
    private Maynard _maynard;

    public MaynardPatrolState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }

    public override void Enter()
    {
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

    public override void Exit()
    {
    }

    public override void Tik()
    {
        _maynard.ChasePlayer();
    }
}

public class MaynardAttackState : State
{
    private Maynard _maynard;
    public MaynardAttackState(string name, Maynard maynard) : base(name)
    {
        _maynard = maynard;
    }
    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Tik()
    {
        _maynard.CloseAttackPlayer();
    }
}