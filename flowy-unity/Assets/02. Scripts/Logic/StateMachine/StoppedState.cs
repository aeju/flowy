namespace Flowy.Logic.StateMachine
{
    /// <summary>
    /// 강제로 정지시킨 상태. 자동 복구 없이, 재가동 명령이 올 때까지 유지
    /// </summary>
    public class StoppedState : IWorkProcessState
    {
        public ProcessStateType StateType => ProcessStateType.Stopped;

        public void Enter(WorkProcess process)
        {

        }

        public void Exit(WorkProcess process)
        {

        }

        // 자동으로 안 풀림 — 오직 재가동 버튼으로만 풀림
        public IWorkProcessState Tick(WorkProcess process)
        {
            return null;
        }
    }
}

