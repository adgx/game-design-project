

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
    }

    public override void Tik()
    {
        _incognito.Patroling();
    }

    public override void Exit()
    {
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
    }

    public override void Tik()
    {
        _incognito.ChasePlayer();
    }

    public override void Exit()
    {
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
        _incognito.anim.lunchIdleAnim();
    }

    public override void Tik()
    {
        _incognito.WonderAttackPlayer();
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
        _incognito.anim.lunchShortSpitAnim();
    }

    public override void Tik()
    {
        _incognito.ShortSpitAttackPlayer();
    }

    public override void Exit()
    {
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