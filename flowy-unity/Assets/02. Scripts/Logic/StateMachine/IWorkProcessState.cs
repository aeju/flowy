namespace Flowy.Logic.StateMachine
{
    public enum ProcessStateType
    {
        Idle,
        Running,
        Error,
        Stopped
    }

    public interface IWorkProcessState
    {
        ProcessStateType StateType { get; }

        // 상태 진입 시 1회 호출 (전이가 실제로 일어날 때만 호출)
        void Enter(WorkProcess process);

        // 매 tick 호출 (다음 상태를 반환, 유지면 null)
        IWorkProcessState Tick(WorkProcess process);

        // 상태 이탈 시 1회 호출
        void Exit(WorkProcess process);
    }
}

