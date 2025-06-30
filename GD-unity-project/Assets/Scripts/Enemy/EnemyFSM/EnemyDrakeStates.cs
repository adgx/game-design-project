public class DrakePatrolState : State
{
    private Drake _drake;

    public DrakePatrolState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }

    public override void Enter()
    {
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
    }

    public override void Exit()
    {
    }

    public override void Tik()
    {
        _drake.ChasePlayer();
    }
}

public class DrakeAttackState : State
{
    private Drake _drake;
    public DrakeAttackState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }
    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Tik()
    {
        _drake.CloseAttackPlayer();
    }
}