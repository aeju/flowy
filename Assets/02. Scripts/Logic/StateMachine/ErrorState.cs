using Flowy.Logic.StateMachine;

/// <summary>
/// 이상 (고장 -> 일정 tick 후 자동 복구)
/// </summary>
public class ErrorState : IWorkProcessState
{
    public ProcessStateType StateType => ProcessStateType.Error;

    public void Enter(WorkProcess process)
    {
        process.ErrorRecoveryTicks = 6;
    }

    public void Exit(WorkProcess process)
    {
        
    }

    public IWorkProcessState Tick(WorkProcess process)
    {
        process.ErrorRecoveryTicks--;   // 매 tick 1씩 감소
        if (process.ErrorRecoveryTicks <= 0)
        {
            return new IdleState();   // 다 되면 복구
        }

        return null;   // 아직 남았으면 계속 Error 유지
    }
}
