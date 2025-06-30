using Unity.VisualScripting;

public class IncognitoPatrolState : State
{
    private Incognito _incognito;

    public IncognitoPatrolState(string name, Incognito incognito) : base(name)
    {
        _incognito = incognito;
    }

    public override void Enter()
    {
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

    public override void Exit()
    {
    }

    public override void Tik()
    {
        _incognito.ChasePlayer();
    }
}

public class IncognitoAttackState : State
{
    private Incognito _incognito;
    public IncognitoAttackState(string name, Incognito incognito) : base(name)
    {
        _incognito = incognito;
    }
    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Tik()
    {
        _incognito.AttackPlayer();
    }
}