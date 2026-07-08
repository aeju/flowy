using Flowy.Logic.StateMachine;

/// <summary>
/// 대기
/// </summary>
public class IdleState : IWorkProcessState
{
    public void Enter(WorkProcess process)
    {
        
    }

    public void Exit(WorkProcess process)
    {
        
    }

    
    public IWorkProcessState Tick(WorkProcess process)
    {
        // 제품이 들어오면 -> RunningState 반환
        if (process.HasProduct())
        {
            return new RunningState();
        }
        return null; // 제품이 없으면 계속 Idle 유지
    }
}
