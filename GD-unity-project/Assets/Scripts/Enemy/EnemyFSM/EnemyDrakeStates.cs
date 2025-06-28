using Unity.VisualScripting;

public class DrakePatrolState : State
{
    private Drake _drake;

    public DrakePatrolState(string name, Drake drake) : base(name)
    {
        _drake = drake;
    }

    public override void Enter()
    {
        throw new System.NotImplementedException();
    }

    public override void Tik()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }
}