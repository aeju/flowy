using Flowy.Logic.Event;

namespace Flowy.Logic.StateMachine
{
    /// <summary>
    /// 공정(WorkProcess)의 현재 상태를 기억하고, 전이를 관리
    /// </summary>
    public class WorkProcessStateMachine
    {
        private IWorkProcessState currentState; // 현재 상태 저장
        private ProcessEventBus processEventBus; // 상태 변화 이벤트 발행용

        // 외부에서 현재 상태 타입을 확인할 수 있도록 프로퍼티 제공
        public ProcessStateType CurrentStateType => currentState.StateType;


        public WorkProcessStateMachine(ProcessEventBus eventBus)
        {
            // 초기 상태: Idle
            currentState = new IdleState();
            this.processEventBus = eventBus;
        }

        // 상태 전이: Exit -> 교체 -> Enter
        private void ChangeState(WorkProcess process, IWorkProcessState nextState)
        {
            currentState.Exit(process);
            currentState = nextState;                                         
            currentState.Enter(process);

            // 상태 변화 이벤트 발행
            processEventBus?.PublishStateChanged(process);
        }

        // 매 Tick 호출
        // 현재 상태에게 위임하고, 전이가 필요하면 처리
        public void Tick(WorkProcess process)
        {
            IWorkProcessState nextState = currentState.Tick(process);
            if (nextState != null) // null이면 상태 유지
            { 
                ChangeState(process, nextState);
            }
        }
    }
}

