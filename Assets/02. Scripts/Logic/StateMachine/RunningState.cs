using Flowy.Logic.StateMachine;
using System;

/// <summary>
/// 가동 중
/// </summary>
public class RunningState : IWorkProcessState
{
    private static readonly Random rand = new Random(); // 판정에 사용 (난수 생성)

    public ProcessStateType StateType => ProcessStateType.Running;

    public void Enter(WorkProcess process)
    {
        
    }

    public void Exit(WorkProcess process)
    {

    }

    public IWorkProcessState Tick(WorkProcess process)
    {
        // 1. 낮은 확률로 고장 발생 -> ErrorState 반환
        // TODO: 고장 확률 밸런스 조정 필요
        if (rand.NextDouble() < 0.02) 
        {
            return new ErrorState();
        }
        
        // 2. 처리할 제품 없으면 -> IdleState 반환
        if (!process.HasProduct())
        {
            return new IdleState();
        }

        // 3. 그 외 -> null (계속 가동)
        return null;
    }
}
