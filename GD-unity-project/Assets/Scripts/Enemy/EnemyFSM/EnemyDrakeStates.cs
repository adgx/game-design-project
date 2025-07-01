public class DrakePatrolState : State
{
    private Drake _drake;

    public DrakePatrolState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }

    public override void Enter()
    {
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
    }

    public override void Exit()
    {
    }

    public override void Tik()
    {
        _drake.ChasePlayer();
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
        _drake.anim.lunchSwipingAnim();
    }

    public override void Tik()
    {
        _drake.SwipingAttackPlayer();
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
        _drake.anim.lunchIdleAnim();
    }

    public override void Tik()
    {
        _drake.WonderCloseAttackPlayer();
    }

    public override void Exit()
    {

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